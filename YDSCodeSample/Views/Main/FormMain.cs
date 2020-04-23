using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using YDSCodeSample.Models;

namespace YDSCodeSample.Views.Main
{
    public partial class FormMain : Form, IMainView
    {
        private ApplicationContext appContext;
        private bool spacebarPressed;

        public bool UndoPossible { set => undoToolStripMenuItem.Enabled = value; }
        public bool RedoPossible { set => redoToolStripMenuItem.Enabled = value; }

        public event Action<string> OpenFileRequest;
        public event Action<string> CreateFileRequest;
        public event Action CreateTaskRequest;
        public event Action<TaskRecord> DeleteTaskRequest;
        public event Action<TaskRecord> ModifyTaskRequest;
        public event Action<TaskRecord> SetTaskDoneRequest;
        public event Action<TaskRecord> SetTaskUndoneRequest;
        public event Action ExitRequest;
        public event Action UndoRequest;
        public event Action RedoRequest;

        public FormMain(ApplicationContext appContext)
        {
            InitializeComponent();

            this.appContext = appContext;
            appContext.MainForm = this;

            ConfigureObjectListView();
        }

        [DllImport("uxtheme.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private void ConfigureObjectListView()
        {
            SetWindowTheme(objectListView.Handle, "explorer", null);
            objectListView.CheckStateGetter = CheckStateGetter;
            objectListView.CheckStatePutter = CheckStatePutter;
        }

        private CheckState CheckStatePutter(object rowObject, CheckState newValue)
        {
            if (newValue == CheckState.Checked)
                SetTaskDoneRequest?.Invoke((TaskRecord)rowObject);
            else if (newValue == CheckState.Unchecked)
                SetTaskUndoneRequest?.Invoke((TaskRecord)rowObject);

            return newValue;
        }

        private CheckState CheckStateGetter(object rowObject)
        {
            if ((bool)((TaskRecord)rowObject).Completed)
                return CheckState.Checked;
            else
                return CheckState.Unchecked;
        }

        public void SetTasks(List<TaskRecord> value)
        {
            objectListView.SetObjects(value, true);
        }

        public void SetFilePath(string value)
        {
            Text = Path.GetFileName(value) + " - Task List";

            createTaskToolStripMenuItem.Enabled = true;
        }

        public void Open()
        {
            Show();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                CreateFileRequest?.Invoke(saveFileDialog.FileName);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                OpenFileRequest?.Invoke(openFileDialog.FileName);
            }
        }

        private void createTaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateTaskRequest?.Invoke();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoRequest?.Invoke();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RedoRequest?.Invoke();
        }

        public void ShowError(string message)
        {
            MessageBox.Show(this, message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModifyTaskRequest?.Invoke((TaskRecord)objectListView.SelectedObjects[0]);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteTaskRequest?.Invoke((TaskRecord)objectListView.SelectedObjects[0]);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitRequest?.Invoke();
        }

        private void objectListView_DoubleClick(object sender, EventArgs e)
        {
            ModifyTaskRequest?.Invoke((TaskRecord)objectListView.SelectedObjects[0]);
        }

        private void objectListView_SelectionChanged(object sender, EventArgs e)
        {
            bool enabled = (objectListView.SelectedObjects.Count != 0);

            propertiesToolStripMenuItem.Enabled = enabled;
            deleteToolStripMenuItem.Enabled = enabled;
        }

        public void DeleteTask(TaskRecord task)
        {
            int select = objectListView.SelectedIndex;
            objectListView.RemoveObject(task);

            if (select != -1 && objectListView.GetItemCount() != 0)
                if (select == objectListView.GetItemCount())
                    objectListView.SelectedIndex = select - 1;
                else
                    objectListView.SelectedIndex = select;
        }

        public void AddTask(TaskRecord item)
        {
            objectListView.AddObject(item);
            objectListView.SelectedObject = item;
        }

        public void RefreshTasks(List<TaskRecord> tasks)
        {
            objectListView.UpdateObjects(tasks);
        }
    }
}
