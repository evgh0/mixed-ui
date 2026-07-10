using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Evgh.MixedUI.XRI
{
    /// <summary>Configures a world-space Canvas for XRI tracked-device rays.</summary>
    public sealed class XRIWorldSpaceCanvasAdapter : IWorldSpaceCanvasAdapter
    {
        private readonly EventSystem _eventSystem;

        public XRIWorldSpaceCanvasAdapter(EventSystem eventSystem)
        {
            _eventSystem = eventSystem != null ? eventSystem : throw new ArgumentNullException(nameof(eventSystem));
        }

        public void Configure(Canvas canvas, Transform viewer)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));
            if (viewer == null) throw new ArgumentNullException(nameof(viewer));

            var inputModules = _eventSystem.GetComponents<BaseInputModule>();
            if (inputModules.Length != 1 || inputModules[0] is not XRUIInputModule)
                throw new InvalidOperationException(
                    "The supplied EventSystem must contain exactly one active input module, and it must be XRUIInputModule.");

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = viewer.GetComponent<Camera>();
            if (canvas.worldCamera == null)
                throw new InvalidOperationException("The viewer transform supplied to the XRI Canvas adapter requires a Camera component.");

            var graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = false;
                if (Application.isPlaying) UnityEngine.Object.Destroy(graphicRaycaster);
                else UnityEngine.Object.DestroyImmediate(graphicRaycaster);
            }

            if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }
    }
}
