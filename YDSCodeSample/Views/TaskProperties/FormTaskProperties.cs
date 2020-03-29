using System;
using System.Windows.Forms;

namespace YDSCodeSample.Views.TaskProperties
{
    public partial class FormTaskProperties : Form, ITaskPropertiesView
    {
        private ApplicationContext appContext;
        public FormTaskProperties(ApplicationContext appContext)
        {
            InitializeComponent();

            this.appContext = appContext;
        }

        public string TaskTitle { get => textBoxTitle.Text; set => textBoxTitle.Text = value; }
        public bool CreatingNewTask
        {
            set
            {
                if (value)
                    Text = "New task";
                else
                    Text = "Task properties";
            }
        }

        public event Action OK;
        public event Action Cancel;

        public void Open()
        {
            ShowDialog(appContext.MainForm);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            OK?.Invoke();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Cancel?.Invoke();
        }
    }
}
