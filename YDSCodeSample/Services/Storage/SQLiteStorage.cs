using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using YDSCodeSample.Models;
using YDSCodeSample.Services.EventSink;
using YDSCodeSample.Services.UndoStack;

namespace YDSCodeSample.Services.Storage
{
    public class SQLiteStorage : IStorage
    {
        private SQLiteConnection connection;
        private IEventSink eventSink;

        public string FilePath { get; protected set; }

        public SQLiteStorage(IEventSink eventSink, IUndoStack undoStack)
        {
            this.eventSink = eventSink;
        }

        public bool CreateFile(string path)
        {
            try
            {
                SQLiteConnection.CreateFile(path);
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return false;
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
                command.CommandText = "CREATE TABLE IF NOT EXISTS Tasks (Id INTEGER PRIMARY KEY NOT NULL, Completed BOOL NOT NULL, Title TEXT, DeadlineTime TEXT, NotificationTime TEXT, CompletionTime TEXT, ModificationTime TEXT, CreationTime TEXT, Description TEXT)";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS Tags (Id INTEGER PRIMARY KEY NOT NULL, Title TEXT, CategoryId INTEGER REFERENCES TagCategories(Id))";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS TaskTagRelations (TaskId INTEGER REFERENCES Tasks(Id), TagId INTEGER REFERENCES Tags(Id))";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS TagCategories (Id INTEGER PRIMARY KEY NOT NULL, Title TEXT)";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS DbInfo (Id INTEGER, Version INTEGER)";
                command.ExecuteNonQuery();
                transaction.Commit();
                FilePath = path;
            }
            catch (SQLiteException e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return false;
            }
            eventSink.InvokeFileOpened(this, new FileOpenedEventArgs(path));
            return true;
        }

        public bool CreateTag(Tag tag)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(connection);
                command.CommandText = @"INSERT INTO Tags (Id, Title, CategoryId) VALUES (@Id, @Title, @CategoryId)";
                SQLiteParameter id = new SQLiteParameter("@Id", tag.Id);
                command.Parameters.Add(id);
                SQLiteParameter title = new SQLiteParameter("@Title", tag.Title);
                command.Parameters.Add(title);
                SQLiteParameter categoryId = new SQLiteParameter("@CategoryId");
                if (tag.Category != null)
                    categoryId.Value = tag.Category.Id;
                command.Parameters.Add(categoryId);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return false;
            }
            return true;
        }

        public bool CreateTask(TaskRecord task)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(connection);
                command.CommandText = "INSERT INTO Tasks (Id, Completed, Title, Description, CreationTime, ModificationTime, CompletionTime, DeadlineTime, NotificationTime)" +
                                      "VALUES (@Id, @Completed, @Title, @Description, @CreationTime, @ModificationTime, @CompletionTime, @DeadlineTime, @NotificationTime)";
                SQLiteParameter id = new SQLiteParameter("@Id", task.Id);
                command.Parameters.Add(id);
                SQLiteParameter completed = new SQLiteParameter("@Completed", task.Completed) { DbType = System.Data.DbType.Boolean };
                command.Parameters.Add(completed);
                SQLiteParameter title = new SQLiteParameter("@Title", task.Title);
                command.Parameters.Add(title);
                SQLiteParameter description = new SQLiteParameter("@Description", task.Description);
                command.Parameters.Add(description);
                SQLiteParameter creationTime = new SQLiteParameter("@CreationTime", task.CreationTime) { DbType = System.Data.DbType.DateTime };
                command.Parameters.Add(creationTime);
                SQLiteParameter modificationTime = new SQLiteParameter("@ModificationTime", task.ModificationTime) { DbType = System.Data.DbType.DateTime };
                command.Parameters.Add(modificationTime);
                SQLiteParameter completionTime = new SQLiteParameter("@CompletionTime", task.CompletionTime) { DbType = System.Data.DbType.DateTime };
                command.Parameters.Add(completionTime);
                SQLiteParameter deadlineTime = new SQLiteParameter("@DeadlineTime", task.DeadlineTime) { DbType = System.Data.DbType.DateTime };
                command.Parameters.Add(deadlineTime);
                SQLiteParameter notificationTime = new SQLiteParameter("@NotificationTime", task.NotificationTime) { DbType = System.Data.DbType.DateTime };
                command.Parameters.Add(notificationTime);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return false;
            }
            return true;
        }

        public bool DeleteTag(Tag tag)
        {
            try
            {
                SQLiteTransaction transaction = connection.BeginTransaction();
                SQLiteCommand command = new SQLiteCommand(connection);
                command.CommandText = "DELETE FROM Tags WHERE Id=@Id";
                SQLiteParameter id = new SQLiteParameter("@Id", tag.Id);
                command.Parameters.Add(id);
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM TaskTagRelations WHERE TagId=@Id";
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return false;
            }
            return true;
        }

        public bool DeleteTask(TaskRecord task)
        {
            try
            {
                SQLiteTransaction transaction = connection.BeginTransaction();
                SQLiteCommand command = new SQLiteCommand(connection);
                command.CommandText = "DELETE FROM Tasks WHERE Id=@Id";
                SQLiteParameter id = new SQLiteParameter("@Id", task.Id);
                command.Parameters.Add(id);
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM TaskTagRelations WHERE TaskId=@Id";
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return false;
            }
            return true;
        }

        public List<Tag> GetTags()
        {
            List<Tag> tags = new List<Tag>();
            try
            {
                SQLiteCommand command = new SQLiteCommand(connection);
                command.CommandText = "SELECT * FROM Tags";
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Tag tag = new Tag();
                        tag.Id = reader.GetInt32(0);
                        tag.Title = reader.GetString(1);
                        tags.Add(tag);
                    }
                }
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return null;
            }
            return tags;
        }

        public List<TaskRecord> GetTasks()
        {
            List<TaskRecord> tasks = new List<TaskRecord>();
            try
            {
                SQLiteCommand command = new SQLiteCommand(connection);
                command.CommandText = "SELECT Id, Completed, Title, Description, CreationTime, ModificationTime, CompletionTime, DeadlineTime, NotificationTime FROM Tasks";
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        TaskRecord task = new TaskRecord()
                        {
                            Id = reader.GetInt32(0),
                            Completed = reader.GetBoolean(1),
                        };
                        if (!reader.IsDBNull(2))
                            task.Title = reader.GetString(2);
                        if (!reader.IsDBNull(3))
                            task.Description = reader.GetString(3);
                        if (!reader.IsDBNull(4))
                            task.CreationTime = reader.GetDateTime(4);
                        if (!reader.IsDBNull(5))
                            task.ModificationTime = reader.GetDateTime(5);
                        if (!reader.IsDBNull(6))
                            task.CompletionTime = reader.GetDateTime(6);
                        if (!reader.IsDBNull(7))
                            task.DeadlineTime = reader.GetDateTime(7);
                        if (!reader.IsDBNull(8))
                            task.NotificationTime = reader.GetDateTime(8);
                        task.Tags = GetTaskTags(task);
                        tasks.Add(task);
                    }
                }
                reader.Close();
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return null;
            }
            return tasks;
        }

        private List<Tag> GetTaskTags(TaskRecord task)
        {
            List<Tag> tags = new List<Tag>();
            try
            {
                SQLiteCommand command = new SQLiteCommand(connection);
                command.CommandText = "SELECT Tags.Id, Tags.Title, Tags.CategoryId FROM Tags INNER JOIN TaskTagRelations ON TaskTagRelations.TagId = Tags.Id WHERE TaskTagRelations.TaskId = @Id";
                SQLiteParameter id = new SQLiteParameter("@Id", task.Id);
                command.Parameters.Add(id);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        Tag tag = new Tag();
                        tag.Id = reader.GetInt32(0);
                        tag.Title = reader.GetString(1);
                        task.Tags.Add(tag);
                    }
                reader.Close();
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
            }
            return tags;
        }

        public bool OpenFile(string path)
        {
            if (!File.Exists(path))
            {
                eventSink.InvokeErrorOccurred(this, new Exception("Can't open \"" + path + "\". File doesn't exists."));
                return false;
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
                FilePath = path;
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return false;
            }
            eventSink.InvokeFileOpened(this, new FileOpenedEventArgs(path));
            return true;
        }

        public bool CloseFile()
        {
            throw new NotImplementedException();
        }

        private bool SetTaskTags(TaskRecord task)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(connection);
                command.CommandText = "DELETE FROM TaskTagRelations WHERE TaskId=@Id";
                SQLiteParameter id = new SQLiteParameter("@Id", task.Id);
                command.Parameters.Add(id);
                command.ExecuteNonQuery();
                foreach (Tag tag in task.Tags)
                {
                    command.Parameters.Clear();
                    command.CommandText = "INSERT INTO TaskTagRelations (TaskId, TagId) VALUES (@TaskId, @TagId)";
                    SQLiteParameter taskId = new SQLiteParameter("@TaskId", task.Id);
                    command.Parameters.Add(taskId);
                    SQLiteParameter tagId = new SQLiteParameter("@TagId", tag.Id);
                    command.Parameters.Add(tagId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return false;
            }
            return true;
        }

        public bool UpdateTag(Tag tag)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(connection);
                command.CommandText = @"UPDATE Tags SET Title = @Title, CategoryId = @CategoryId WHERE Id = @Id";
                SQLiteParameter id = new SQLiteParameter("@Id", tag.Id);
                command.Parameters.Add(id);
                SQLiteParameter title = new SQLiteParameter("@Title", tag.Title);
                command.Parameters.Add(title);
                SQLiteParameter categoryId = new SQLiteParameter("@CategoryId");
                if (tag.Category != null)
                    categoryId.Value = tag.Category.Id;
                command.Parameters.Add(categoryId);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return false;
            }
            return true;
        }

        public bool UpdateTask(TaskRecord task)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(connection);
                command.CommandText = @"UPDATE Tasks SET Completed = @Completed, Title = @Title, Description = @Description, CreationTime = @CreationTime, ModificationTime = @ModificationTime, CompletionTime = @CompletionTime, DeadlineTime = @DeadlineTime, NotificationTime = @NotificationTime WHERE Id = @Id";
                SQLiteParameter id = new SQLiteParameter("@Id", task.Id);
                command.Parameters.Add(id);
                SQLiteParameter completed = new SQLiteParameter("@Completed", task.Completed) { DbType = System.Data.DbType.Boolean };
                command.Parameters.Add(completed);
                SQLiteParameter title = new SQLiteParameter("@Title", task.Title);
                command.Parameters.Add(title);
                SQLiteParameter description = new SQLiteParameter("@Description", task.Description);
                command.Parameters.Add(description);
                SQLiteParameter creationTime = new SQLiteParameter("@CreationTime", task.CreationTime) { DbType = System.Data.DbType.DateTime };
                command.Parameters.Add(creationTime);
                SQLiteParameter modificationTime = new SQLiteParameter("@ModificationTime", task.ModificationTime) { DbType = System.Data.DbType.DateTime };
                command.Parameters.Add(modificationTime);
                SQLiteParameter completionTime = new SQLiteParameter("@CompletionTime", task.CompletionTime) { DbType = System.Data.DbType.DateTime };
                command.Parameters.Add(completionTime);
                SQLiteParameter deadlineTime = new SQLiteParameter("@DeadlineTime", task.DeadlineTime) { DbType = System.Data.DbType.DateTime };
                command.Parameters.Add(deadlineTime);
                SQLiteParameter notificationTime = new SQLiteParameter("@NotificationTime", task.NotificationTime) { DbType = System.Data.DbType.DateTime };
                command.Parameters.Add(notificationTime);
                command.ExecuteNonQuery();
                SetTaskTags(task);
            }
            catch (Exception e)
            {
                eventSink.InvokeErrorOccurred(this, e);
                return false;
            }
            return true;
        }
    }
}