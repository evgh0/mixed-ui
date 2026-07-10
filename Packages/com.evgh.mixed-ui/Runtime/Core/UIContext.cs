using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    /// <summary>Explicit shared services used while building UI.</summary>
    public sealed class UIContext
    {
        public UITheme Theme { get; }
        public UIFactory Factory { get; }
        public Transform Viewer { get; }

        public UIContext(UITheme theme, UIFactory factory, Transform viewer)
        {
            Theme = theme != null ? theme : throw new ArgumentNullException(nameof(theme));
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Viewer = viewer != null ? viewer : throw new ArgumentNullException(nameof(viewer));
        }
    }
}

