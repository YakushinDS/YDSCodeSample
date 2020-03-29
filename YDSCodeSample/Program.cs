using System;
using System.Windows.Forms;
using YDSCodeSample.Presenters;
using YDSCodeSample.Services;
using YDSCodeSample.Services.ErrorEventSink;
using YDSCodeSample.Services.UndoStack;
using YDSCodeSample.Views.Main;
using YDSCodeSample.Views.TaskProperties;

namespace YDSCodeSample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ApplicationContext context = new ApplicationContext();
            IApplicationManager manager = new ApplicationManager()
                .RegisterInstance(context)
                .RegisterView<IMainView, FormMain>()
                .RegisterView<ITaskPropertiesView, FormTaskProperties>()
                .RegisterService<IStorage, SQLiteStorage>()
                .RegisterService<IUndoStack, UndoStack>()
                .RegisterService<IErrorEventSink, ErrorEventSink>();
            manager.Run<MainPresenter>();
            Application.Run(context);
        }
    }
}
