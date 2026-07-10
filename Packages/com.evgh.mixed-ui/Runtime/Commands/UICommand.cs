using System;

namespace Evgh.MixedUI
{
    /// <summary>A typed, explicitly refreshable command.</summary>
    public sealed class UICommand<TTarget> : IUICommand where TTarget : class
    {
        private readonly TTarget _target;
        private readonly Action<TTarget> _execute;
        private readonly Func<TTarget, bool> _canExecute;

        public UICommand(TTarget target, Action<TTarget> execute, Func<TTarget, bool> canExecute = null)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute => _canExecute?.Invoke(_target) ?? true;
        public event Action CanExecuteChanged;

        public void Execute()
        {
            if (CanExecute) _execute(_target);
        }

        public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke();
    }
}

