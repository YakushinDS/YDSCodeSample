using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using YDSCodeSample.Models;
using YDSCodeSample.Services.ErrorEventSink;
using YDSCodeSample.Services.UndoStack;

namespace YDSCodeSample.Services
{
    public class SQLiteStorage : IStorage
    {
        private SQLiteConnection connection;
        private string filePath;
        private IErrorEventSink errorSink;
        private IUndoStack undoStack;

        public event Action<TaskRecord> TaskCreated;
        public event Action<TaskRecord> TaskUpdated;
        public event Action<TaskRecord> TaskDeleted;
        public event Action<string> FileOpened;

        public SQLiteStorage(IErrorEventSink errorSink, IUndoStack undoStack)
        {
            this.errorSink = errorSink;
            this.undoStack = undoStack;
        }

        public void CreateFile(string path)
        {
            try
            {
                SQLiteConnection.CreateFile(path);
            }
            catch (Exception e)
            {
                errorSink.ReportError(this, e);

                return;
            }

            SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder()
            {
                DataSource = path,
                Version = 3
            };

            try
            {
                connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();

                SQLiteTransaction transaction = connection.BeginTransaction();

                SQLiteCommand command = new SQLiteCommand()
                {
                    Transaction = transaction,
                    Connection = connection
                };

                command.CommandText = "CREATE TABLE IF NOT EXISTS Tasks (Id INTEGER PRIMARY KEY NOT NULL, Completed BOOL NOT NULL, Title TEXT)";
                command.ExecuteNonQuery();

                transaction.Commit();

                filePath = path;

                FileOpened?.Invoke(path);
            }
            catch (SQLiteException e)
            {
                errorSink.ReportError(this, e);

                return;
            }
        }

        public void OpenFile(string path)
        {
            if (!File.Exists(path))
            {
                errorSink.ReportError(this, new Exception("Can't open \"" + path + "\". File doesn't exists."));

                return;
            }
            SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder()
            {
                DataSource = path,
                Version = 3
            };
            try
            {
                connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);

                filePath = path;

                FileOpened?.Invoke(path);
            }
            catch (Exception e)
            {
                errorSink.ReportError(this, e);

                return;
            }
        }

        public List<TaskRecord> GetTasks()
        {
            using (DataContext context = new DataContext(connection) { ObjectTrackingEnabled = false })
            {
                var tasks = context.GetTable<TaskRecord>();
                return tasks.ToList();
            }
        }

        public void CreateTask(TaskRecord task)
        {
            CreateTaskOperation operation = new CreateTaskOperation(task, connection);
            operation.TaskCreated += (t => TaskCreated?.Invoke(t));
            operation.TaskDeleted += (t => TaskDeleted?.Invoke(t));
            undoStack.PerformCommand(operation);
        }

        public void ModifyTask(TaskRecord task)
        {
            ModifyTaskOperation operation = new ModifyTaskOperation(task, connection);
            operation.TaskUpdated += (t => TaskUpdated?.Invoke(t));
            undoStack.PerformCommand(operation);
        }

        public void DeleteTask(TaskRecord task)
        {
            DeleteTaskOperation operation = new DeleteTaskOperation(task, connection);
            operation.TaskCreated += (t => TaskCreated?.Invoke(t));
            operation.TaskDeleted += (t => TaskDeleted?.Invoke(t));
            undoStack.PerformCommand(operation);
        }

        private class CreateTaskOperation : IUndoCommand
        {
            private TaskRecord task;
            private SQLiteConnection connection;

            public string Description { get; }
            public event Action<TaskRecord> TaskCreated;
            public event Action<TaskRecord> TaskDeleted;

            public CreateTaskOperation(TaskRecord task, SQLiteConnection connection)
            {
                this.task = task;
                this.connection = connection;
                Description = "Create new task: " + task.Title;
            }

            public void Perform()
            {
                using (DataContext context = new DataContext(connection))
                {
                    Table<TaskRecord> tasks = context.GetTable<TaskRecord>();

                    tasks.InsertOnSubmit(task);
                    context.SubmitChanges();

                    TaskCreated?.Invoke(task);
                }
            }

            public void Rollback()
            {
                using (DataContext context = new DataContext(connection))
                {
                    Table<TaskRecord> tasks = context.GetTable<TaskRecord>();

                    tasks.Attach(task);
                    tasks.DeleteOnSubmit(task);
                    context.SubmitChanges();

                    TaskDeleted?.Invoke(task);
                }
            }
        }

        private class ModifyTaskOperation : IUndoCommand
        {
            private TaskRecord modifiedTask;
            private TaskRecord originalTask;
            private SQLiteConnection connection;

            public string Description { get; }
            public event Action<TaskRecord> TaskUpdated;

            public ModifyTaskOperation(TaskRecord modifiedTask, SQLiteConnection connection)
            {
                originalTask = new TaskRecord();
                this.modifiedTask = modifiedTask;
                this.connection = connection;
                Description = "Modify task: " + modifiedTask.Title;
            }

            public void Perform()
            {
                using (DataContext context = new DataContext(connection))
                {
                    Table<TaskRecord> tasks = context.GetTable<TaskRecord>();

                    // LINQ генерирует запрос с синтаксисом SQL Server,
                    // который не совместим с SQLite:
                    // TaskRecord task = tasks.First(t => t.Id == modifiedTask.Id);
                    //
                    // Поэтому используем цикл.
                    foreach (TaskRecord task in tasks)
                    {
                        if (task.Id == modifiedTask.Id)
                        {
                            originalTask.Id = task.Id;
                            originalTask.Completed = task.Completed;
                            originalTask.Title = task.Title;

                            task.Completed = modifiedTask.Completed;
                            task.Title = modifiedTask.Title;

                            break;
                        }
                    }

                    context.SubmitChanges();
                }

                TaskUpdated?.Invoke(modifiedTask);
            }

            public void Rollback()
            {
                using (DataContext context = new DataContext(connection))
                {
                    Table<TaskRecord> tasks = context.GetTable<TaskRecord>();

                    // LINQ генерирует запрос с синтаксисом SQL Server,
                    // который не совместим с SQLite:
                    // TaskRecord task = tasks.First(t => t.Id == modifiedTask.Id);
                    //
                    // Поэтому используем цикл.
                    foreach (TaskRecord task in tasks)
                    {
                        if (task.Id == originalTask.Id)
                        {
                            task.Completed = originalTask.Completed;
                            task.Title = originalTask.Title;

                            break;
                        }
                    }

                    context.SubmitChanges();
                }

                TaskUpdated?.Invoke(originalTask);
            }
        }

        private class DeleteTaskOperation : IUndoCommand
        {
            private TaskRecord task;
            private SQLiteConnection connection;

            public string Description { get; }
            public event Action<TaskRecord> TaskCreated;
            public event Action<TaskRecord> TaskDeleted;

            public DeleteTaskOperation(TaskRecord task, SQLiteConnection connection)
            {
                this.task = task;
                this.connection = connection;
                Description = "Delete task: " + task.Title;
            }

            public void Perform()
            {
                using (DataContext context = new DataContext(connection))
                {
                    Table<TaskRecord> tasks = context.GetTable<TaskRecord>();

                    tasks.Attach(task);
                    tasks.DeleteOnSubmit(task);
                    context.SubmitChanges();

                    TaskDeleted?.Invoke(task);
                }
            }

            public void Rollback()
            {
                using (DataContext context = new DataContext(connection))
                {
                    Table<TaskRecord> tasks = context.GetTable<TaskRecord>();

                    tasks.InsertOnSubmit(task);
                    context.SubmitChanges();

                    TaskCreated?.Invoke(task);
                }
            }
        }
    }
}