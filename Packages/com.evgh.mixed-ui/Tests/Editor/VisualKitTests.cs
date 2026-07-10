using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Evgh.MixedUI.Tests
{
    public sealed class VisualKitTests
    {
        private const string Root = "Packages/com.evgh.mixed-ui/Runtime/Assets";
        private const string ThemePath = Root + "/Themes/DefaultMixedUITheme.asset";
        private const string Controls = Root + "/Prefabs/Controls/";
        private const string Layout = Root + "/Prefabs/Layout/";
        private const string Canvases = Root + "/Prefabs/Canvases/";
        private const string Gallery = Root + "/Prefabs/Gallery/Visual Gallery.prefab";

        private static readonly string[] ExpectedPrefabs =
        {
            Controls + "Body Label.prefab",
            Controls + "Heading Label.prefab",
            Controls + "Caption Label.prefab",
            Controls + "Button Primary.prefab",
            Controls + "Button Secondary.prefab",
            Controls + "Button Subtle.prefab",
            Controls + "Button Destructive.prefab",
            Controls + "Button Success.prefab",
            Controls + "Toggle.prefab",
            Controls + "Slider.prefab",
            Layout + "Vertical Stack.prefab",
            Layout + "Horizontal Stack.prefab",
            Layout + "Card.prefab",
            Layout + "Modal.prefab",
            Layout + "Scroll Panel.prefab",
            Canvases + "World Space Canvas.prefab",
            Canvases + "Head Follow Canvas.prefab",
            Gallery,
        };

        private readonly List<Object> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var value in _created)
                if (value != null) Object.DestroyImmediate(value);
            _created.Clear();
        }

        [Test]
        public void EveryDocumentedPrefabExistsWithoutMissingScripts()
        {
            foreach (var path in ExpectedPrefabs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab), Is.Zero, path);
            }
        }

        [Test]
        public void CanvasPrefabsUseXRIComponentsDirectly()
        {
            var world = LoadPrefab(Canvases + "World Space Canvas.prefab");
            Assert.That(world.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.WorldSpace));
            Assert.That(world.GetComponent<TrackedDeviceGraphicRaycaster>(), Is.Not.Null);
            Assert.That(world.GetComponent<GraphicRaycaster>(), Is.Null);
            Assert.That(world.GetComponent<MixedUIThemeScope>(), Is.Not.Null);

            var follow = LoadPrefab(Canvases + "Head Follow Canvas.prefab");
            Assert.That(follow.GetComponent<LazyFollow>(), Is.Not.Null);
        }

        [Test]
        public void ControlPrefabsUseStandardUGUIComponentsAndVisualBindings()
        {
            var button = LoadPrefab(Controls + "Button Primary.prefab");
            Assert.That(button.GetComponent<Button>(), Is.Not.Null);
            Assert.That(button.GetComponent<MixedUIButtonStyle>(), Is.Not.Null);
            Assert.That(button.GetComponentInChildren<TMP_Text>(true), Is.Not.Null);

            var toggle = LoadPrefab(Controls + "Toggle.prefab");
            Assert.That(toggle.GetComponent<Toggle>(), Is.Not.Null);
            Assert.That(toggle.GetComponent<MixedUIToggleStyle>(), Is.Not.Null);

            var slider = LoadPrefab(Controls + "Slider.prefab");
            Assert.That(slider.GetComponent<Slider>(), Is.Not.Null);
            Assert.That(slider.GetComponent<MixedUISliderStyle>(), Is.Not.Null);

            var scroll = LoadPrefab(Layout + "Scroll Panel.prefab");
            var scrollRect = scroll.GetComponent<ScrollRect>();
            Assert.That(scrollRect, Is.Not.Null);
            Assert.That(scrollRect.viewport, Is.Not.Null);
            Assert.That(scrollRect.content, Is.Not.Null);
        }

        [Test]
        public void ThemeAppliesSemanticSelectableAndTextValues()
        {
            var theme = Load<MixedUITheme>(ThemePath);
            var instance = Object.Instantiate(LoadPrefab(Controls + "Button Destructive.prefab"));
            _created.Add(instance);
            var style = instance.GetComponent<MixedUIButtonStyle>();

            style.ApplyTheme(theme);

            Assert.That(style.Button.transition, Is.EqualTo(Selectable.Transition.ColorTint));
            Assert.That(style.Button.colors.normalColor, Is.EqualTo(theme.colors.destructive));
            Assert.That(style.Button.colors.disabledColor, Is.EqualTo(theme.colors.disabled));
            Assert.That(style.Label.fontSize, Is.EqualTo(theme.typography.bodySize));
            Assert.That(style.Label.color, Is.EqualTo(theme.colors.textPrimary));
        }

        [Test]
        public void GeneratedPrefabsBakeTheDefaultThemeForEditorPreview()
        {
            var theme = Load<MixedUITheme>(ThemePath);
            var primary = LoadPrefab(Controls + "Button Primary.prefab");
            var card = LoadPrefab(Layout + "Card.prefab");

            Assert.That(primary.GetComponent<Button>().colors.normalColor, Is.EqualTo(theme.colors.primary));
            Assert.That(card.GetComponent<Image>().color, Is.EqualTo(theme.colors.surface));
        }

        [Test]
        public void ThemeScopeSupportsRuntimeStyleReplacement()
        {
            var original = Load<MixedUITheme>(ThemePath);
            var alternate = Object.Instantiate(original);
            alternate.colors.primary = Color.magenta;
            _created.Add(alternate);

            var canvas = Object.Instantiate(LoadPrefab(Canvases + "World Space Canvas.prefab"));
            var button = Object.Instantiate(LoadPrefab(Controls + "Button Primary.prefab"), canvas.transform);
            _created.Add(canvas);
            var scope = canvas.GetComponent<MixedUIThemeScope>();

            scope.Theme = alternate;

            Assert.That(button.GetComponent<Button>().colors.normalColor, Is.EqualTo(Color.magenta));
        }

        [Test]
        public void GalleryContainsAllSemanticButtonVariantsAndCorePanels()
        {
            var gallery = LoadPrefab(Gallery);
            var variants = gallery.GetComponentsInChildren<MixedUIButtonStyle>(true)
                .Select(value => value.Variant)
                .ToHashSet();

            Assert.That(variants, Does.Contain(MixedUIVariant.Primary));
            Assert.That(variants, Does.Contain(MixedUIVariant.Secondary));
            Assert.That(variants, Does.Contain(MixedUIVariant.Subtle));
            Assert.That(variants, Does.Contain(MixedUIVariant.Destructive));
            Assert.That(variants, Does.Contain(MixedUIVariant.Success));
            Assert.That(gallery.GetComponentInChildren<ScrollRect>(true), Is.Not.Null);
            Assert.That(gallery.GetComponentsInChildren<CanvasGroup>(true).Length, Is.GreaterThanOrEqualTo(2));
        }

        private static GameObject LoadPrefab(string path) => Load<GameObject>(path);

        private static T Load<T>(string path) where T : Object
        {
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.That(value, Is.Not.Null, path);
            return value;
        }
    }
}
