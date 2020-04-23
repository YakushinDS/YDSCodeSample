using System;
using System.Collections.Generic;
using YDSCodeSample.Models;
using YDSCodeSample.Services;
using YDSCodeSample.Services.EventSink;
using YDSCodeSample.Services.Storage;
using YDSCodeSample.Services.UndoStack;
using YDSCodeSample.Views.Main;

namespace YDSCodeSample.Presenters
{
    class MainPresenter : IPresenter
    {
        private IMainView view;
        private IStorage storage;
        private IUndoStack undoStack;
        private IApplicationManager applicationManager;
        public IEventSink eventSink;

        public MainPresenter(IApplicationManager applicationManager, IMainView view, IStorage storage, IUndoStack undoStack, IEventSink eventSink)
        {
            this.applicationManager = applicationManager;
            this.view = view;
            this.storage = storage;
            this.undoStack = undoStack;
            this.eventSink = eventSink;

            view.CreateFileRequest += View_CreateFile;
            view.OpenFileRequest += View_OpenFile;
            view.CreateTaskRequest += View_CreateTask;
            view.ModifyTaskRequest += View_ModifyTask;
            view.DeleteTaskRequest += View_DeleteTask;
            view.SetTaskDoneRequest += View_SetTaskDone;
            view.SetTaskUndoneRequest += View_SetTaskUndone;
            view.UndoRequest += View_Undo;
            view.RedoRequest += View_Redo;
            view.ExitRequest += View_Exit;

            //storage.TaskCreated += UpdateView;
            //storage.TaskDeleted += UpdateView;
            //storage.TaskUpdated += UpdateView;
            //storage.FileOpened += Storage_FileOpened;
            eventSink.FileOpened += Storage_FileOpened;
            eventSink.TaskChanged += Storage_TaskChanged;

            undoStack.CommandPerformed += UndoStack_OperationPerformed;

            eventSink.ErrorOccurred += ErrorSink_ErrorOccurred;
        }

        private void Storage_TaskChanged(object sender, ModelChangedEventArgs<TaskRecord> e)
        {
            if (sender != this)
                switch (e.Action)
                {
                    case ModelChangedAction.Delete:
                        view.DeleteTask(e.Model);
                        break;
                    case ModelChangedAction.Create:
                        view.AddTask(e.Model);
                        break;
                    case ModelChangedAction.Update:
                        List<TaskRecord> tasksToUpdate = new List<TaskRecord>();
                        tasksToUpdate.Add(e.Model);
                        view.RefreshTasks(tasksToUpdate);
                        break;
                }
        }

        private void Storage_FileOpened1(object sender, FileOpenedEventArgs e)
        {
            view.SetFilePath(e.FilePath);
            view.SetTasks(storage.GetTasks());
        }

        private void View_Exit()
        {
            view.Close();
        }

        private void View_DeleteTask(TaskRecord obj)
        {
            if (storage.DeleteTask(obj))
                view.DeleteTask(obj);
        }

        private void View_ModifyTask(TaskRecord obj)
        {
            applicationManager.Run<TaskPropertiesPresenter, TaskRecord>(obj);
        }

        private void ErrorSink_ErrorOccurred(object sender, ErrorEventArgs e)
        {
            view.ShowError(e.Exception.Message);
        }

        private void View_CreateTask()
        {
            applicationManager.Run<TaskPropertiesPresenter, TaskRecord>(null);
        }

        private void View_Redo()
        {
            undoStack.Redo();
        }

        private void View_Undo()
        {
            undoStack.Undo();
        }

        private void UndoStack_OperationPerformed(object sender, EventArgs e)
        {
            view.UndoPossible = undoStack.CanUndo;
            view.RedoPossible = undoStack.CanRedo;
        }

        private void Storage_FileOpened(object sender, FileOpenedEventArgs e)
        {
            view.SetFilePath(e.FilePath);
            var tasks = storage.GetTasks();
            if(tasks != null)
                view.SetTasks(tasks);
        }

        private void UpdateView(TaskRecord obj)
        {
            view.SetTasks(storage.GetTasks());
        }

        private void View_SetTaskUndone(TaskRecord obj)
        {
            obj.Completed = false;
            //storage.ModifyTask(obj);
        }

        private void View_SetTaskDone(TaskRecord obj)
        {
            obj.Completed = true;
            //storage.ModifyTask(obj);
        }

        private void View_OpenFile(string obj)
        {
            storage.OpenFile(obj);
        }

        private void View_CreateFile(string obj)
        {
            storage.CreateFile(obj);
        }

        public void Run()
        {
            view.Open();
        }
    }
}
