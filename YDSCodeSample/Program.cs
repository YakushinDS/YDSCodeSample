using System;
using System.Windows.Forms;
using YDSCodeSample.Presenters;
using YDSCodeSample.Services;
using YDSCodeSample.Services.EventSink;
using YDSCodeSample.Services.Storage;
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
                .RegisterService<IEventSink, EventSink>();
            manager.Run<MainPresenter>();
            Application.Run(context);
        }
    }
}
