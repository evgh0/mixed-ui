using System;

namespace Evgh.MixedUI
{
    /// <summary>Typed entry point for contextual composition builders.</summary>
    public sealed class UIComposer
    {
        private readonly UIContext _context;

        public UIComposer(UIContext context) => _context = context ?? throw new ArgumentNullException(nameof(context));

        public ContextualUIBuilder<TTarget> For<TTarget>(TTarget target) where TTarget : class =>
            new(_context, target ?? throw new ArgumentNullException(nameof(target)));
    }
}

