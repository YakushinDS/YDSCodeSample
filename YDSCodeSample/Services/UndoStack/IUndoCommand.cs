namespace YDSCodeSample.Services.UndoStack
{
    public interface IUndoCommand
    {
        string Description { get; }

        void Perform();

        void Rollback();
    }
}
