using LightInject;
using System;
using YDSCodeSample.Presenters;
using YDSCodeSample.Views;

namespace YDSCodeSample
{
    public class ApplicationManager : IApplicationManager
    {
        private ServiceContainer container;

        public ApplicationManager()
        {
            container = new ServiceContainer();
            container.RegisterInstance<IApplicationManager>(this);
        }
        
        public IApplicationManager RegisterService<TService, TImplementation>() where TImplementation : class, TService
        {
            container.Register<TService, TImplementation>(new PerContainerLifetime());
            return this;
        }

        public IApplicationManager RegisterView<TView, TImplementation>()
            where TView : IView
            where TImplementation : class, TView
        {
            container.Register<TView, TImplementation>();
            return this;
        }

        public IApplicationManager RegisterInstance<TImplementation>(TImplementation instance)
        {
            container.RegisterInstance(instance);
            return this;
        }

        public void Run<TPresenter>()
            where TPresenter : class, IPresenter
        {
            if (!container.CanGetInstance(typeof(TPresenter), String.Empty))
                container.Register<TPresenter>();
            IPresenter presenter = container.GetInstance<TPresenter>();
            presenter.Run();
        }

        public void Run<TPresenter, TArg>(TArg arg)
            where TPresenter : class, IPresenter<TArg>
        {
            if (!container.CanGetInstance(typeof(TPresenter), String.Empty))
                container.Register<TPresenter>();
            IPresenter<TArg> presenter = container.GetInstance<TPresenter>();
            presenter.Run(arg);
        }

    }
}
