using System;
using YDSCodeSample.Models;
using YDSCodeSample.Services;
using YDSCodeSample.Services.ErrorEventSink;
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
        public IErrorEventSink errorSink;

        public MainPresenter(IApplicationManager applicationManager, IMainView view, IStorage storage, IUndoStack undoStack, IErrorEventSink errorSink)
        {
            this.applicationManager = applicationManager;
            this.view = view;
            this.storage = storage;
            this.undoStack = undoStack;
            this.errorSink = errorSink;

            view.CreateFile += View_CreateFile;
            view.OpenFile += View_OpenFile;
            view.CreateTask += View_CreateTask;
            view.ModifyTask += View_ModifyTask;
            view.DeleteTask += View_DeleteTask;
            view.SetTaskDone += View_SetTaskDone;
            view.SetTaskUndone += View_SetTaskUndone;
            view.Undo += View_Undo;
            view.Redo += View_Redo;
            view.Exit += View_Exit;

            storage.TaskCreated += UpdateView;
            storage.TaskDeleted += UpdateView;
            storage.TaskUpdated += UpdateView;
            storage.FileOpened += Storage_FileOpened;

            undoStack.CommandPerformed += UndoStack_OperationPerformed;

            errorSink.ErrorOccurred += ErrorSink_ErrorOccurred;
        }

        private void View_Exit()
        {
            view.Close();
        }

        private void View_DeleteTask(TaskRecord obj)
        {
            storage.DeleteTask(obj);
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

        private void Storage_FileOpened(string obj)
        {
            view.SetFilePath(obj);
            view.SetTasks(storage.GetTasks());
        }

        private void UpdateView(TaskRecord obj)
        {
            view.SetTasks(storage.GetTasks());
        }

        private void View_SetTaskUndone(TaskRecord obj)
        {
            obj.Completed = false;
            storage.ModifyTask(obj);
        }

        private void View_SetTaskDone(TaskRecord obj)
        {
            obj.Completed = true;
            storage.ModifyTask(obj);
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
