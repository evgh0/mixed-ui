using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Evgh.MixedUI.Editor
{
    /// <summary>Creates the documented placeholder theme, catalog, and prefabs.</summary>
    public static class DefaultAssetGenerator
    {
        private const string Root = "Packages/com.evgh.mixed-ui/Runtime/Assets";
        private const string Prefabs = Root + "/Prefabs";
        private const string Themes = Root + "/Themes";

        [MenuItem("Tools/Evgh Mixed UI/Regenerate Default Assets")]
        public static void GenerateAll()
        {
            EnsureFolder("Packages/com.evgh.mixed-ui/Runtime", "Assets");
            EnsureFolder(Root, "Prefabs");
            EnsureFolder(Root, "Themes");
            AssetDatabase.DeleteAsset(Themes + "/Inter-Regular.ttf");

            var sprite = CreateRoundedSprite();
            EnsureTemporaryTMPSettings();
            var theme = CreateTheme(sprite, null);

            var label = CreateLabelPrefab();
            var button = CreateButtonPrefab();
            var toggle = CreateTogglePrefab();
            var slider = CreateSliderPrefab();
            var stack = CreateStackPrefab();
            var card = CreateCardPrefab();
            var menu = CreateConfigurationMenuPrefab(card, label, toggle, slider, button);
            CreateCatalog(label, button, toggle, slider, stack, card, menu);

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Evgh Mixed UI default assets generated.");
        }

        private static Sprite CreateRoundedSprite()
        {
            const string path = Themes + "/RoundedSurface.png";
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var pixels = new Color32[32 * 32];
            const float radius = 8f;
            for (var y = 0; y < 32; y++)
            for (var x = 0; x < 32; x++)
            {
                var cx = Mathf.Clamp(x, radius, 31f - radius);
                var cy = Mathf.Clamp(y, radius, 31f - radius);
                var inside = (new Vector2(x - cx, y - cy)).sqrMagnitude <= radius * radius;
                pixels[y * 32 + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = new Vector4(8f, 8f, 8f, 8f);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureTemporaryTMPSettings()
        {
            var settings = ScriptableObject.CreateInstance<TMP_Settings>();
            typeof(TMP_Settings).GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic)
                ?.SetValue(null, settings);
        }

        private static UITheme CreateTheme(Sprite sprite, TMP_FontAsset font)
        {
            const string path = Themes + "/DefaultUITheme.asset";
            AssetDatabase.DeleteAsset(path);
            var theme = ScriptableObject.CreateInstance<UITheme>();
            theme.geometry.roundedSurfaceSprite = sprite;
            theme.typography.defaultFont = font;
            AssetDatabase.CreateAsset(theme, path);
            return theme;
        }

        private static UILabel CreateLabelPrefab()
        {
            var root = UIObject("Label", null, new Vector2(320f, 48f));
            var text = root.AddComponent<TextMeshProUGUI>();
            text.text = "Label";
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            var element = root.AddComponent<UILabel>();
            Set(element, "textComponent", text);
            return SavePrefab(element, Prefabs + "/UILabel.prefab");
        }

        private static UIButton CreateButtonPrefab()
        {
            var root = UIObject("Button", null, new Vector2(320f, 56f));
            var image = root.AddComponent<Image>();
            var button = root.AddComponent<Button>();
            button.targetGraphic = image;
            var state = root.AddComponent<UIVisualStateController>();
            Set(state, "targetGraphic", image);
            Set(state, "scaleTarget", root.GetComponent<RectTransform>());

            var textObject = UIObject("Label", root.transform, Vector2.zero);
            Stretch(textObject.GetComponent<RectTransform>(), 12f, 6f);
            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = "Button";
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;

            var element = root.AddComponent<UIButton>();
            Set(element, "button", button);
            Set(element, "label", text);
            Set(element, "visualState", state);
            return SavePrefab(element, Prefabs + "/UIButton.prefab");
        }

        private static UIToggle CreateTogglePrefab()
        {
            var root = UIObject("Toggle", null, new Vector2(320f, 56f));
            var toggle = root.AddComponent<Toggle>();

            var backgroundObject = UIObject("Background", root.transform, new Vector2(40f, 40f));
            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = backgroundRect.anchorMax = new Vector2(0f, 0.5f);
            backgroundRect.anchoredPosition = new Vector2(24f, 0f);
            var background = backgroundObject.AddComponent<Image>();

            var checkObject = UIObject("Checkmark", backgroundObject.transform, Vector2.zero);
            Stretch(checkObject.GetComponent<RectTransform>(), 8f, 8f);
            var check = checkObject.AddComponent<Image>();

            var labelObject = UIObject("Label", root.transform, Vector2.zero);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(56f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);
            var text = labelObject.AddComponent<TextMeshProUGUI>();
            text.text = "Toggle";
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;

            toggle.targetGraphic = background;
            toggle.graphic = check;
            var element = root.AddComponent<UIToggle>();
            Set(element, "toggle", toggle);
            Set(element, "label", text);
            Set(element, "background", background);
            return SavePrefab(element, Prefabs + "/UIToggle.prefab");
        }

        private static UISlider CreateSliderPrefab()
        {
            var root = UIObject("Slider", null, new Vector2(320f, 72f));
            var slider = root.AddComponent<Slider>();

            var labelObject = UIObject("Label", root.transform, Vector2.zero);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(0f, 0f);
            labelRect.offsetMax = Vector2.zero;
            var text = labelObject.AddComponent<TextMeshProUGUI>();
            text.text = "Slider";
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;

            var backgroundObject = UIObject("Background", root.transform, Vector2.zero);
            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0f);
            backgroundRect.anchorMax = new Vector2(1f, 0.5f);
            backgroundRect.offsetMin = new Vector2(0f, 14f);
            backgroundRect.offsetMax = new Vector2(0f, -14f);
            backgroundObject.AddComponent<Image>().color = new Color(0.2f, 0.22f, 0.25f, 1f);

            var fillObject = UIObject("Fill", backgroundObject.transform, Vector2.zero);
            Stretch(fillObject.GetComponent<RectTransform>(), 0f, 0f);
            var fill = fillObject.AddComponent<Image>();

            var handleObject = UIObject("Handle", root.transform, new Vector2(28f, 28f));
            var handleRect = handleObject.GetComponent<RectTransform>();
            handleRect.anchorMin = handleRect.anchorMax = new Vector2(0f, 0.25f);
            var handle = handleObject.AddComponent<Image>();

            slider.fillRect = fillObject.GetComponent<RectTransform>();
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            var element = root.AddComponent<UISlider>();
            Set(element, "slider", slider);
            Set(element, "label", text);
            Set(element, "fill", fill);
            return SavePrefab(element, Prefabs + "/UISlider.prefab");
        }

        private static UIStack CreateStackPrefab()
        {
            var root = UIObject("Stack", null, new Vector2(320f, 100f));
            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            var fitter = root.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var element = root.AddComponent<UIStack>();
            Set(element, "layout", layout);
            return SavePrefab(element, Prefabs + "/UIStack.prefab");
        }

        private static UICard CreateCardPrefab()
        {
            var root = UIObject("Card", null, new Vector2(360f, 420f));
            var image = root.AddComponent<Image>();
            var group = root.AddComponent<CanvasGroup>();
            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 16f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            var header = Slot("HeaderRoot", root.transform, 56f);
            var content = Slot("ContentRoot", root.transform, 240f);
            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 12f;
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            var footer = Slot("FooterRoot", root.transform, 64f);
            var footerLayout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 12f;
            footerLayout.childControlHeight = true;
            footerLayout.childControlWidth = true;

            var element = root.AddComponent<UICard>();
            Set(element, "surface", image);
            Set(element, "headerRoot", header);
            Set(element, "contentRoot", content);
            Set(element, "footerRoot", footer);
            Set(element, "canvasGroup", group);
            return SavePrefab(element, Prefabs + "/UICard.prefab");
        }

        private static ObjectConfigurationMenu CreateConfigurationMenuPrefab(
            UICard cardPrefab,
            UILabel labelPrefab,
            UIToggle togglePrefab,
            UISlider sliderPrefab,
            UIButton buttonPrefab)
        {
            var root = UIObject("ObjectConfigurationMenu", null, new Vector2(360f, 420f));
            root.transform.localScale = Vector3.one * 0.001f;
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            root.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 1000f;
            root.AddComponent<GraphicRaycaster>();

            var card = (UICard)PrefabUtility.InstantiatePrefab(cardPrefab, root.transform);
            Stretch(card.RectTransform, 0f, 0f);
            var title = (UILabel)PrefabUtility.InstantiatePrefab(labelPrefab, card.HeaderRoot);
            title.SetStyle(UILabelStyle.Heading);
            var toggle = (UIToggle)PrefabUtility.InstantiatePrefab(togglePrefab, card.ContentRoot);
            var slider = (UISlider)PrefabUtility.InstantiatePrefab(sliderPrefab, card.ContentRoot);
            var delete = (UIButton)PrefabUtility.InstantiatePrefab(buttonPrefab, card.FooterRoot);
            var close = (UIButton)PrefabUtility.InstantiatePrefab(buttonPrefab, card.FooterRoot);
            close.Text = "Close";

            var menu = root.AddComponent<ObjectConfigurationMenu>();
            Set(menu, "card", card);
            Set(menu, "titleLabel", title);
            Set(menu, "visibilityToggle", toggle);
            Set(menu, "scaleSlider", slider);
            Set(menu, "deleteButton", delete);
            Set(menu, "closeButton", close);
            return SavePrefab(menu, Prefabs + "/ObjectConfigurationMenu.prefab");
        }

        private static void CreateCatalog(
            UILabel label, UIButton button, UIToggle toggle, UISlider slider,
            UIStack stack, UICard card, ObjectConfigurationMenu menu)
        {
            const string path = Root + "/DefaultUIPrefabCatalog.asset";
            AssetDatabase.DeleteAsset(path);
            var catalog = ScriptableObject.CreateInstance<UIPrefabCatalog>();
            Set(catalog, "labelPrefab", label);
            Set(catalog, "buttonPrefab", button);
            Set(catalog, "togglePrefab", toggle);
            Set(catalog, "sliderPrefab", slider);
            Set(catalog, "stackPrefab", stack);
            Set(catalog, "cardPrefab", card);
            Set(catalog, "configurationMenuPrefab", menu);
            AssetDatabase.CreateAsset(catalog, path);
        }

        private static GameObject UIObject(string name, Transform parent, Vector2 size)
        {
            var value = new GameObject(name, typeof(RectTransform));
            value.transform.SetParent(parent, false);
            value.GetComponent<RectTransform>().sizeDelta = size;
            return value;
        }

        private static RectTransform Slot(string name, Transform parent, float height)
        {
            var value = UIObject(name, parent, new Vector2(0f, height)).GetComponent<RectTransform>();
            var layout = value.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            return value;
        }

        private static void Stretch(RectTransform rect, float horizontal, float vertical)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(horizontal, vertical);
            rect.offsetMax = new Vector2(-horizontal, -vertical);
        }

        private static T SavePrefab<T>(T component, string path) where T : Component
        {
            AssetDatabase.DeleteAsset(path);
            var prefab = PrefabUtility.SaveAsPrefabAsset(component.gameObject, path);
            Object.DestroyImmediate(component.gameObject);
            return prefab.GetComponent<T>();
        }

        private static void Set(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            var field = serialized.FindProperty(property);
            if (field == null) throw new MissingFieldException(target.GetType().Name, property);
            field.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
