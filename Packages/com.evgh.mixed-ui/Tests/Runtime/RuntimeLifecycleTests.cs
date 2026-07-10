using System.Collections;
using Evgh.MixedUI.XRI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Evgh.MixedUI.Tests
{
    public sealed class RuntimeLifecycleTests
    {
        [UnityTest]
        public IEnumerator ObjectProviderDestroysReleasedInstanceByEndOfFrame()
        {
            var prefabObject = new GameObject("Prefab", typeof(RectTransform));
            var prefab = prefabObject.AddComponent<TestRuntimeElement>();
            var parent = new GameObject("Parent");
            var provider = new InstantiateUIObjectProvider();
            var instance = provider.Get(prefab, parent.transform);

            provider.Release(instance);
            yield return null;

            Assert.That(instance == null, Is.True);
            Object.Destroy(prefabObject);
            Object.Destroy(parent);
        }

        [Test]
        public void XRIAdapterRequiresExactlyOneXRInputModule()
        {
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            var viewer = new GameObject("Viewer", typeof(Camera));
            try
            {
                var adapter = new XRIWorldSpaceCanvasAdapter(eventSystemObject.GetComponent<EventSystem>());
                Assert.Throws<System.InvalidOperationException>(() =>
                    adapter.Configure(canvasObject.GetComponent<Canvas>(), viewer.transform));

                eventSystemObject.AddComponent<XRUIInputModule>();
                adapter.Configure(canvasObject.GetComponent<Canvas>(), viewer.transform);
                Assert.That(canvasObject.GetComponent<TrackedDeviceGraphicRaycaster>(), Is.Not.Null);
                Assert.That(canvasObject.GetComponent<GraphicRaycaster>().enabled, Is.False,
                    "The built-in raycaster is disabled immediately and destroyed at end of frame.");
                Assert.That(canvasObject.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.WorldSpace));
            }
            finally
            {
                Object.DestroyImmediate(eventSystemObject);
                Object.DestroyImmediate(canvasObject);
                Object.DestroyImmediate(viewer);
            }
        }
    }

    public sealed class TestRuntimeElement : UIElement
    {
        protected override void OnThemeApplied(UITheme theme) { }
    }
}
