using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Evgh.MixedUI.Tests
{
    public sealed class VisualRuntimeTests
    {
        [Test]
        public void StandardUGUIEventsRemainTheBehaviorAPI()
        {
            var buttonObject = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            var toggleObject = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            var sliderObject = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            try
            {
                var clicks = 0;
                var toggleValue = false;
                var sliderValue = 0f;
                var button = buttonObject.GetComponent<Button>();
                var toggle = toggleObject.GetComponent<Toggle>();
                var slider = sliderObject.GetComponent<Slider>();
                button.onClick.AddListener(() => clicks++);
                toggle.onValueChanged.AddListener(value => toggleValue = value);
                slider.onValueChanged.AddListener(value => sliderValue = value);

                button.onClick.Invoke();
                toggle.isOn = true;
                slider.value = 0.75f;

                Assert.That(clicks, Is.EqualTo(1));
                Assert.That(toggleValue, Is.True);
                Assert.That(sliderValue, Is.EqualTo(0.75f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(buttonObject);
                Object.DestroyImmediate(toggleObject);
                Object.DestroyImmediate(sliderObject);
            }
        }

        [UnityTest]
        public IEnumerator XRICanvasAndGalleryLoadWithoutXRHardware()
        {
#if UNITY_EDITOR
            const string galleryPath = "Packages/com.evgh.mixed-ui/Runtime/Assets/Prefabs/Gallery/Visual Gallery.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(galleryPath);
            Assert.That(prefab, Is.Not.Null);
            var instance = Object.Instantiate(prefab);
            yield return null;

            Assert.That(instance.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.WorldSpace));
            Assert.That(instance.GetComponent<TrackedDeviceGraphicRaycaster>(), Is.Not.Null);
            Assert.That(instance.GetComponent<MixedUIThemeScope>(), Is.Not.Null);
            Assert.That(instance.GetComponentsInChildren<Button>(true).Length, Is.GreaterThan(5));
            Object.Destroy(instance);
#else
            yield return null;
#endif
        }

        [UnityTest]
        public IEnumerator LazyFollowPrefabUsesXRIImplementation()
        {
#if UNITY_EDITOR
            const string path = "Packages/com.evgh.mixed-ui/Runtime/Assets/Prefabs/Canvases/Head Follow Canvas.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var target = new GameObject("Follow Target", typeof(Camera));
            target.tag = "MainCamera";
            var instance = Object.Instantiate(prefab);
            var follow = instance.GetComponent<LazyFollow>();
            follow.target = target.transform;
            follow.enabled = false;
            follow.enabled = true;
            yield return null;

            Assert.That(follow.target, Is.SameAs(target.transform));
            Object.Destroy(instance);
            Object.Destroy(target);
#else
            yield return null;
#endif
        }

        [Test]
        public void GalleryEventSystemComponentsCanCoexistWithoutHardware()
        {
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            try
            {
                Assert.That(eventSystemObject.AddComponent<XRUIInputModule>(), Is.Not.Null);
                Assert.That(eventSystemObject.AddComponent<CanvasOptimizer>(), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(eventSystemObject);
            }
        }
    }
}
