using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using YDSCodeSample.Models;

namespace YDSCodeSample.Views.Main
{
    public partial class FormMain : Form, IMainView
    {
        private ApplicationContext appContext;

        public bool UndoPossible { set => undoToolStripMenuItem.Enabled = value; }
        public bool RedoPossible { set => redoToolStripMenuItem.Enabled = value; }

        public event Action<string> OpenFile;
        public event Action<string> CreateFile;
        public event Action CreateTask;
        public event Action<TaskRecord> DeleteTask;
        public event Action<TaskRecord> ModifyTask;
        public event Action<TaskRecord> SetTaskDone;
        public event Action<TaskRecord> SetTaskUndone;
        public event Action Exit;
        public event Action Undo;
        public event Action Redo;

        public FormMain(ApplicationContext appContext)
        {
            InitializeComponent();

            this.appContext = appContext;
            appContext.MainForm = this;

            ConfigureObjectListView();
        }

        private void ConfigureObjectListView()
        {
            objectListView.CheckStateGetter = CheckStateGetter;
            objectListView.CheckStatePutter = CheckStatePutter;
        }

        private CheckState CheckStatePutter(object rowObject, CheckState newValue)
        {
            if (newValue == CheckState.Checked)
                SetTaskDone?.Invoke((TaskRecord)rowObject);
            else if (newValue == CheckState.Unchecked)
                SetTaskUndone?.Invoke((TaskRecord)rowObject);

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
            objectListView.SetObjects(value);
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
                CreateFile?.Invoke(saveFileDialog.FileName);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                OpenFile?.Invoke(openFileDialog.FileName);
            }
        }

        private void createTaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateTask?.Invoke();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo?.Invoke();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Redo?.Invoke();
        }

        public void ShowError(string message)
        {
            MessageBox.Show(this, message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModifyTask?.Invoke((TaskRecord)objectListView.SelectedObjects[0]);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteTask?.Invoke((TaskRecord)objectListView.SelectedObjects[0]);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Exit?.Invoke();
        }

        private void objectListView_DoubleClick(object sender, EventArgs e)
        {
            ModifyTask?.Invoke((TaskRecord)objectListView.SelectedObjects[0]);
        }

        private void objectListView_SelectionChanged(object sender, EventArgs e)
        {
            bool enabled = (objectListView.SelectedObjects.Count != 0);

            propertiesToolStripMenuItem.Enabled = enabled;
            deleteToolStripMenuItem.Enabled = enabled;
        }
    }
}
