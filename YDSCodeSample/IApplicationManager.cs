using YDSCodeSample.Presenters;
using YDSCodeSample.Views;

namespace YDSCodeSample
{
    public interface IApplicationManager
    {
        IApplicationManager RegisterInstance<TImplementation>(TImplementation instance);

        IApplicationManager RegisterService<TService, TImplementation>() where TImplementation : class, TService;

        IApplicationManager RegisterView<TView, TImplementation>()
            where TView : IView
            where TImplementation : class, TView;

        void Run<TPresenter, TArg>(TArg arg)
            where TPresenter : class, IPresenter<TArg>;

        void Run<TPresenter>() where TPresenter : class, IPresenter;
    }
}