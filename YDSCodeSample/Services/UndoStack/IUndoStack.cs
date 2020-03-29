using System;

namespace YDSCodeSample.Services.UndoStack
{
    public interface IUndoStack
    {
        event EventHandler CommandPerformed;

        int Capacity { get; set; }
        int Count { get; }
        string LastCommandDescription { get; }
        string NextCommandDescription { get; }
        bool CanRedo { get; }
        bool CanUndo { get; }

        void BeginCommandSequence(string description);

        void EndCommandSequence();

        void PerformCommand(IUndoCommand command);

        void Redo();

        void Undo();
    }
}