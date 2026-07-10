using System;

namespace Evgh.MixedUI
{
    /// <summary>All explicit inputs required to create and bind contextual UI.</summary>
    public sealed class UICreationContext<TTarget> where TTarget : class
    {
        public UIContext UI { get; }
        public TTarget Target { get; }
        public UISpatialContext Spatial { get; }

        public UICreationContext(UIContext ui, TTarget target, UISpatialContext spatial)
        {
            UI = ui ?? throw new ArgumentNullException(nameof(ui));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Spatial = spatial ?? throw new ArgumentNullException(nameof(spatial));
        }
    }
}

