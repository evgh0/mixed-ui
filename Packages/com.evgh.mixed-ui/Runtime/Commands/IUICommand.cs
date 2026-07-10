using System;

namespace Evgh.MixedUI
{
    public interface IUICommand
    {
        bool CanExecute { get; }
        event Action CanExecuteChanged;
        void Execute();
    }
}

