using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    public sealed class ContextualPanelBuilder<TTarget> where TTarget : class, IConfigurableObject
    {
        private readonly ContextualUIBuilder<TTarget> _builder;
        private readonly string _title;
        private bool _showVisibility;
        private bool _showScale;
        private bool _showDelete;
        private bool _built;
        private float _minimumScale = 0.1f;
        private float _maximumScale = 3f;

        internal ContextualPanelBuilder(ContextualUIBuilder<TTarget> builder, string title)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("A panel title is required.", nameof(title));
            _builder = builder;
            _title = title;
        }

        public ContextualPanelBuilder<TTarget> ShowVisibilityControl() { _showVisibility = true; return this; }

        public ContextualPanelBuilder<TTarget> ShowScaleControl(float minimum, float maximum)
        {
            if (minimum >= maximum) throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum must exceed minimum.");
            _showScale = true;
            _minimumScale = minimum;
            _maximumScale = maximum;
            return this;
        }

        public ContextualPanelBuilder<TTarget> ShowDeleteAction() { _showDelete = true; return this; }

        public UIInstanceHandle<ObjectConfigurationMenu> Build(Transform parent)
        {
            if (_built) throw new InvalidOperationException("This builder has already built an instance.");
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (_builder.Anchor == null) throw new InvalidOperationException("AnchoredTo must be called before Build.");
            _built = true;

            var spatial = new UISpatialContext(
                _builder.Anchor,
                _builder.Offset,
                _builder.OffsetSpace,
                _builder.ShouldFaceViewer,
                _builder.PlacementPolicy);

            var handle = _builder.Context.Factory.CreateConfigurationMenu(parent, _builder.Context.Viewer);
            try
            {
                var menu = handle.Instance;
                menu.Configure(_title, _showVisibility, _showScale, _minimumScale, _maximumScale, _showDelete);
                var follower = menu.GetComponent<UISpatialFollower>() ?? menu.gameObject.AddComponent<UISpatialFollower>();
                follower.Initialize(spatial, _builder.Context.Viewer);
                menu.CloseRequested += handle.Close;
                menu.Bind(new UICreationContext<IConfigurableObject>(_builder.Context, _builder.Target, spatial));
                return handle;
            }
            catch
            {
                handle.Close();
                throw;
            }
        }
    }
}
