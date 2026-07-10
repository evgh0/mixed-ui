using System;

namespace Evgh.MixedUI
{
    /// <summary>Owns one factory-created UI instance and releases it at most once.</summary>
    public sealed class UIInstanceHandle<TElement> : IDisposable where TElement : UIElement
    {
        private readonly UIFactory _factory;
        private TElement _instance;

        internal UIInstanceHandle(UIFactory factory, TElement instance)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _instance = instance != null ? instance : throw new ArgumentNullException(nameof(instance));
        }

        public bool IsClosed => _instance == null;
        public TElement Instance => _instance != null
            ? _instance
            : throw new ObjectDisposedException(typeof(TElement).Name);

        public void Close()
        {
            if (_instance == null) return;
            var instance = _instance;
            _instance = null;
            _factory.Release(instance);
        }

        public void Dispose() => Close();
    }
}

