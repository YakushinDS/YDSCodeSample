namespace YDSCodeSample.Presenters
{
    public interface IPresenter
    {
        void Run();
    }

    public interface IPresenter<in TArg>
    {
        void Run(TArg arg);
    }
}
