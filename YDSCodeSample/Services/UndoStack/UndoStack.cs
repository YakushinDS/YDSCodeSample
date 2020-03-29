using System;
using System.Collections.Generic;
using System.Linq;
using YDSCodeSample.Services.ErrorEventSink;

namespace YDSCodeSample.Services.UndoStack
{
    public class UndoStack : IUndoStack
    {
        private int capacity = Int32.MaxValue - 1;
        private int index = -1;
        private bool performingCommandSequence = false;
        private List<IUndoCommand> commands = new List<IUndoCommand>();
        private UndoSequence commandSequence;
        private IErrorEventSink errorSink;

        public event EventHandler CommandPerformed;
        
        public UndoStack(IErrorEventSink errorSink)
        {
            this.errorSink = errorSink;
        }

        public int Capacity
        {
            get
            {
                return capacity;
            }
            set
            {
                if (commands.Count > value)
                {
                    if (index < value)
                        commands.RemoveRange(value, commands.Count - value);
                    else
                        commands.RemoveRange(0, commands.Count - value);
                }

                capacity = value;
            }
        }

        public int Count
        {
            get
            {
                return commands.Count;
            }
        }

        public string LastCommandDescription
        {
            get
            {
                if (index >= 0)
                    return commands.ElementAt(index).Description;
                else
                    return null;
            }
        }

        public string NextCommandDescription
        {
            get
            {
                if (index + 1 <= commands.Count)
                    return commands.ElementAt(index + 1).Description;
                else
                    return null;
            }
        }

        public bool CanRedo
        {
            get
            {
                return !(index + 1 == commands.Count);
            }
        }

        public bool CanUndo
        {
            get
            {
                return !(index < 0);
            }
        }

        public void Undo()
        {
            if (index < 0)
                return;

            IUndoCommand command = commands.ElementAt(index);

            try
            {
                command.Rollback();

                index--;

                CommandPerformed?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                HandleError(command, ex);
            }
        }

        public void Redo()
        {
            if (index + 1 == commands.Count)
                return;

            IUndoCommand command = commands.ElementAt(index + 1);

            try
            {
                command.Perform();

                index++;

                CommandPerformed?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                HandleError(command, ex);
            }
        }

        public void BeginCommandSequence(string description)
        {
            performingCommandSequence = true;

            commandSequence = new UndoSequence(description, errorSink);
            commandSequence.CommandPerformed += (sender, e) => CommandPerformed?.Invoke(this, e);
        }

        public void EndCommandSequence()
        {
            performingCommandSequence = false;

            PerformCommand(commandSequence);
        }

        public void PerformCommand(IUndoCommand command)
        {
            if (performingCommandSequence)
            {
                commandSequence.PerformCommand(command);
            }
            else
            {
                try
                {
                    command.Perform();

                    index++;

                    commands.Insert(index, command);

                    if (index < capacity && commands.Count > index + 1)
                        commands.RemoveRange(index + 1, commands.Count - (index + 1));

                    if (commands.Count > capacity)
                        commands.RemoveRange(0, commands.Count - capacity);

                    CommandPerformed?.Invoke(this, new EventArgs());
                }
                catch (Exception ex)
                {
                    HandleError(command, ex);
                }
            }
        }

        private void HandleError(IUndoCommand command, Exception exception)
        {
            errorSink.ReportError(this, exception);
        }

        private class UndoSequence : UndoStack, IUndoCommand
        {
            private bool overfillDetected;

            public string Description { get; }

            public UndoSequence(string description, IErrorEventSink errorSink) : base(errorSink)
            {
                this.Description = description;
                capacity = Int32.MaxValue - 1;
            }

            public void Perform()
            {
                if (index + 1 == commands.Count)
                    return;

                if (overfillDetected)
                {
                    errorSink.ReportError(this, new Exception("Command sequence is too large to perform"));

                    return;
                }

                while (index > 0)
                    Redo();
            }

            public void Rollback()
            {
                if (overfillDetected)
                {
                    errorSink.ReportError(this, new Exception("Command sequence is too large to rollback"));

                    return;
                }

                while (index < commands.Count)
                    Undo();
            }

            public new void PerformCommand(IUndoCommand command)
            {
                if (index + 1 > capacity)
                    overfillDetected = true;

                base.PerformCommand(command);
            }
        }
    }
}
