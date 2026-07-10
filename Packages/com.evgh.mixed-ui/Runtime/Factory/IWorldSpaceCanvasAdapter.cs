using System;
using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    /// <summary>Framework boundary for configuring a world-space Canvas.</summary>
    public interface IWorldSpaceCanvasAdapter
    {
        void Configure(Canvas canvas, Transform viewer);
    }

    public sealed class UGUIWorldSpaceCanvasAdapter : IWorldSpaceCanvasAdapter
    {
        public void Configure(Canvas canvas, Transform viewer)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));
            if (viewer == null) throw new ArgumentNullException(nameof(viewer));
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = viewer.GetComponent<Camera>();
            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }
}

