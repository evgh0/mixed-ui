using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Object = UnityEngine.Object;

namespace Evgh.MixedUI.Editor
{
    /// <summary>Regenerates the committed neutral-dark XRI/uGUI visual assets.</summary>
    public static class DefaultAssetGenerator
    {
        private const string Root = "Packages/com.evgh.mixed-ui/Runtime/Assets";
        private const string Prefabs = Root + "/Prefabs";
        private const string Controls = Prefabs + "/Controls";
        private const string Layout = Prefabs + "/Layout";
        private const string Canvases = Prefabs + "/Canvases";
        private const string Gallery = Prefabs + "/Gallery";
        private const string Themes = Root + "/Themes";
        private const string Sample = "Packages/com.evgh.mixed-ui/Samples~/VisualGallery";

        [MenuItem("Tools/Mixed-UI/Regenerate Default Assets")]
        public static void GenerateAll()
        {
            foreach (var legacyRuntimeFolder in new[]
                     {
                         "Adapters", "Builders", "Commands", "Context", "Core", "Elements",
                         "Factory", "Interaction", "Layout", "Spatial",
                     })
                AssetDatabase.DeleteAsset($"Packages/com.evgh.mixed-ui/Runtime/{legacyRuntimeFolder}");

            var legacySample = Path.GetFullPath("Packages/com.evgh.mixed-ui/Samples~/ObjectConfiguration");
            if (Directory.Exists(legacySample)) Directory.Delete(legacySample, true);

            AssetDatabase.DeleteAsset(Prefabs);

            EnsureFolder("Packages/com.evgh.mixed-ui/Runtime", "Assets");
            EnsureFolder(Root, "Themes");
            EnsureFolder(Root, "Prefabs");
            EnsureFolder(Prefabs, "Controls");
            EnsureFolder(Prefabs, "Layout");
            EnsureFolder(Prefabs, "Canvases");
            EnsureFolder(Prefabs, "Gallery");

            EnsureTemporaryTMPSettings();
            var roundedSprite = CreateRoundedSprite();
            var theme = CreateTheme(roundedSprite);

            var body = CreateTextPrefab("Body Label", MixedUITextRole.Body, false);
            var heading = CreateTextPrefab("Heading Label", MixedUITextRole.Heading, false);
            var caption = CreateTextPrefab("Caption Label", MixedUITextRole.Caption, true);

            var primaryButton = CreateButtonPrefab("Button Primary", MixedUIVariant.Primary);
            var secondaryButton = CreateButtonPrefab("Button Secondary", MixedUIVariant.Secondary);
            var subtleButton = CreateButtonPrefab("Button Subtle", MixedUIVariant.Subtle);
            var destructiveButton = CreateButtonPrefab("Button Destructive", MixedUIVariant.Destructive);
            var successButton = CreateButtonPrefab("Button Success", MixedUIVariant.Success);
            var toggle = CreateTogglePrefab();
            var slider = CreateSliderPrefab();
            var verticalStack = CreateStackPrefab("Vertical Stack", true);
            var horizontalStack = CreateStackPrefab("Horizontal Stack", false);
            var card = CreateCardPrefab();
            var modal = CreateModalPrefab(heading, body, secondaryButton, primaryButton);
            var scrollPanel = CreateScrollPanelPrefab(body);

            foreach (var visualPrefab in new[]
                     {
                         body, heading, caption,
                         primaryButton, secondaryButton, subtleButton, destructiveButton, successButton,
                         toggle, slider, card, modal, scrollPanel,
                     })
                BakeTheme(visualPrefab, theme);

            var worldCanvas = CreateCanvasPrefab("World Space Canvas", theme, false);
            var headFollowCanvas = CreateCanvasPrefab("Head Follow Canvas", theme, true);
            var gallery = CreateGalleryPrefab(
                theme, heading, body, caption,
                primaryButton, secondaryButton, subtleButton, destructiveButton, successButton,
                toggle, slider, card, modal, scrollPanel);
            BakeTheme(gallery, theme);

            CreateVisualGalleryScene(gallery);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Mixed-UI visual assets generated: {worldCanvas.name}, {headFollowCanvas.name}, " +
                      $"{verticalStack.name}, {horizontalStack.name}, and gallery controls.");
        }

        private static MixedUITheme CreateTheme(Sprite sprite)
        {
            const string path = Themes + "/DefaultMixedUITheme.asset";
            AssetDatabase.DeleteAsset(path);
            var theme = ScriptableObject.CreateInstance<MixedUITheme>();
            theme.sprites.roundedSurface = sprite;
            theme.sprites.roundedControl = sprite;
            AssetDatabase.CreateAsset(theme, path);
            return theme;
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
                var inside = new Vector2(x - cx, y - cy).sqrMagnitude <= radius * radius;
                pixels[y * 32 + x] = inside
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(255, 255, 255, 0);
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

        private static GameObject CreateTextPrefab(string name, MixedUITextRole role, bool secondary)
        {
            var root = UIObject(name, null, role == MixedUITextRole.Heading
                ? new Vector2(520f, 56f)
                : new Vector2(520f, 40f));
            var text = root.AddComponent<TextMeshProUGUI>();
            text.text = name;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            var style = root.AddComponent<MixedUITextStyle>();
            Set(style, "text", text);
            SetEnum(style, "role", (int)role);
            SetBool(style, "secondaryColor", secondary);
            return SavePrefab(root, Controls + "/" + name + ".prefab");
        }

        private static GameObject CreateButtonPrefab(string name, MixedUIVariant variant)
        {
            var root = UIObject(name, null, new Vector2(240f, 56f));
            var image = root.AddComponent<Image>();
            var button = root.AddComponent<Button>();
            button.targetGraphic = image;

            var labelObject = UIObject("Label", root.transform, Vector2.zero);
            Stretch(labelObject.GetComponent<RectTransform>(), 14f, 6f);
            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = name.Replace("Button ", string.Empty);
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            var style = root.AddComponent<MixedUIButtonStyle>();
            Set(style, "button", button);
            Set(style, "background", image);
            Set(style, "label", label);
            SetEnum(style, "variant", (int)variant);
            return SavePrefab(root, Controls + "/" + name + ".prefab");
        }

        private static GameObject CreateTogglePrefab()
        {
            var root = UIObject("Toggle", null, new Vector2(320f, 56f));
            var toggle = root.AddComponent<Toggle>();

            var backgroundObject = UIObject("Background", root.transform, new Vector2(40f, 40f));
            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = backgroundRect.anchorMax = new Vector2(0f, 0.5f);
            backgroundRect.anchoredPosition = new Vector2(24f, 0f);
            var background = backgroundObject.AddComponent<Image>();

            var checkObject = UIObject("Checkmark", backgroundObject.transform, Vector2.zero);
            Stretch(checkObject.GetComponent<RectTransform>(), 9f, 9f);
            var checkmark = checkObject.AddComponent<Image>();

            var labelObject = UIObject("Label", root.transform, Vector2.zero);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(58f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);
            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = "Toggle";
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;

            toggle.targetGraphic = background;
            toggle.graphic = checkmark;
            var style = root.AddComponent<MixedUIToggleStyle>();
            Set(style, "toggle", toggle);
            Set(style, "background", background);
            Set(style, "checkmark", checkmark);
            Set(style, "label", label);
            return SavePrefab(root, Controls + "/Toggle.prefab");
        }

        private static GameObject CreateSliderPrefab()
        {
            var root = UIObject("Slider", null, new Vector2(520f, 76f));
            var slider = root.AddComponent<Slider>();

            var labelObject = UIObject("Label", root.transform, Vector2.zero);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = "Slider";
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;

            var trackObject = UIObject("Track", root.transform, Vector2.zero);
            var trackRect = trackObject.GetComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, 0.5f);
            trackRect.offsetMin = new Vector2(0f, 13f);
            trackRect.offsetMax = new Vector2(0f, -13f);
            var track = trackObject.AddComponent<Image>();

            var fillObject = UIObject("Fill", trackObject.transform, Vector2.zero);
            Stretch(fillObject.GetComponent<RectTransform>(), 0f, 0f);
            var fill = fillObject.AddComponent<Image>();

            var handleObject = UIObject("Handle", root.transform, new Vector2(30f, 30f));
            var handleRect = handleObject.GetComponent<RectTransform>();
            handleRect.anchorMin = handleRect.anchorMax = new Vector2(0f, 0.25f);
            var handle = handleObject.AddComponent<Image>();

            slider.fillRect = fillObject.GetComponent<RectTransform>();
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.6f;

            var style = root.AddComponent<MixedUISliderStyle>();
            Set(style, "slider", slider);
            Set(style, "track", track);
            Set(style, "fill", fill);
            Set(style, "handle", handle);
            Set(style, "label", label);
            return SavePrefab(root, Controls + "/Slider.prefab");
        }

        private static GameObject CreateStackPrefab(string name, bool vertical)
        {
            var root = UIObject(name, null, new Vector2(520f, 120f));
            HorizontalOrVerticalLayoutGroup layout = vertical
                ? root.AddComponent<VerticalLayoutGroup>()
                : root.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            var fitter = root.AddComponent<ContentSizeFitter>();
            if (vertical) fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            else fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return SavePrefab(root, Layout + "/" + name + ".prefab");
        }

        private static GameObject CreateCardPrefab()
        {
            var root = UIObject("Card", null, new Vector2(600f, 420f));
            var image = root.AddComponent<Image>();
            root.AddComponent<CanvasGroup>();
            var style = root.AddComponent<MixedUISurfaceStyle>();
            Set(style, "surface", image);
            SetBool(style, "elevated", false);
            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 16f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            return SavePrefab(root, Layout + "/Card.prefab");
        }

        private static GameObject CreateModalPrefab(
            GameObject headingPrefab,
            GameObject bodyPrefab,
            GameObject secondaryButtonPrefab,
            GameObject primaryButtonPrefab)
        {
            var root = UIObject("Modal", null, new Vector2(560f, 300f));
            var image = root.AddComponent<Image>();
            root.AddComponent<CanvasGroup>();
            var style = root.AddComponent<MixedUISurfaceStyle>();
            Set(style, "surface", image);
            SetBool(style, "elevated", true);
            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 28, 28);
            layout.spacing = 18f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            var heading = InstantiatePrefab(headingPrefab, root.transform);
            heading.GetComponent<TMP_Text>().text = "Modal title";
            var body = InstantiatePrefab(bodyPrefab, root.transform);
            body.GetComponent<TMP_Text>().text = "Use standard UnityEvents to close the modal or confirm an action.";

            var footer = UIObject("Actions", root.transform, new Vector2(0f, 56f));
            var footerLayout = footer.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 12f;
            footerLayout.childControlHeight = true;
            footerLayout.childControlWidth = true;
            footerLayout.childForceExpandWidth = true;
            InstantiatePrefab(secondaryButtonPrefab, footer.transform).GetComponentInChildren<TMP_Text>().text = "Cancel";
            InstantiatePrefab(primaryButtonPrefab, footer.transform).GetComponentInChildren<TMP_Text>().text = "Confirm";
            return SavePrefab(root, Layout + "/Modal.prefab");
        }

        private static GameObject CreateScrollPanelPrefab(GameObject bodyPrefab)
        {
            var root = UIObject("Scroll Panel", null, new Vector2(600f, 360f));
            var image = root.AddComponent<Image>();
            var surfaceStyle = root.AddComponent<MixedUISurfaceStyle>();
            Set(surfaceStyle, "surface", image);
            var scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewport = UIObject("Viewport", root.transform, Vector2.zero);
            Stretch(viewport.GetComponent<RectTransform>(), 18f, 18f);
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = UIObject("Content", viewport.transform, new Vector2(0f, 500f));
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 1f);
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (var index = 1; index <= 8; index++)
            {
                var row = InstantiatePrefab(bodyPrefab, content.transform);
                row.GetComponent<TMP_Text>().text = $"Scrollable item {index}";
            }

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRect;
            return SavePrefab(root, Layout + "/Scroll Panel.prefab");
        }

        private static GameObject CreateCanvasPrefab(string name, MixedUITheme theme, bool headFollow)
        {
            var root = UIObject(name, null, new Vector2(1000f, 700f));
            root.transform.localScale = Vector3.one * 0.001f;
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 1000f;
            root.AddComponent<TrackedDeviceGraphicRaycaster>();
            var scope = root.AddComponent<MixedUIThemeScope>();
            Set(scope, "theme", theme);
            if (headFollow)
            {
                var follow = root.AddComponent<LazyFollow>();
                follow.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
                follow.rotationFollowMode = LazyFollow.RotationFollowMode.LookAtWithWorldUp;
                follow.targetOffset = new Vector3(0f, -0.08f, 0.75f);
            }
            return SavePrefab(root, Canvases + "/" + name + ".prefab");
        }

        private static GameObject CreateGalleryPrefab(
            MixedUITheme theme,
            GameObject headingPrefab,
            GameObject bodyPrefab,
            GameObject captionPrefab,
            GameObject primaryButtonPrefab,
            GameObject secondaryButtonPrefab,
            GameObject subtleButtonPrefab,
            GameObject destructiveButtonPrefab,
            GameObject successButtonPrefab,
            GameObject togglePrefab,
            GameObject sliderPrefab,
            GameObject cardPrefab,
            GameObject modalPrefab,
            GameObject scrollPanelPrefab)
        {
            var root = UIObject("Mixed UI Visual Gallery", null, new Vector2(1280f, 820f));
            root.transform.localScale = Vector3.one * 0.001f;
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            root.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 1000f;
            root.AddComponent<TrackedDeviceGraphicRaycaster>();
            var scope = root.AddComponent<MixedUIThemeScope>();
            Set(scope, "theme", theme);

            var card = InstantiatePrefab(cardPrefab, root.transform);
            Stretch(card.GetComponent<RectTransform>(), 0f, 0f);
            var heading = InstantiatePrefab(headingPrefab, card.transform);
            heading.GetComponent<TMP_Text>().text = "Mixed UI — XRI Visual Gallery";
            var body = InstantiatePrefab(bodyPrefab, card.transform);
            body.GetComponent<TMP_Text>().text = "Standard uGUI controls styled for XRI ray and poke input.";

            var buttons = UIObject("Semantic Buttons", card.transform, new Vector2(0f, 56f));
            var buttonLayout = buttons.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 12f;
            buttonLayout.childControlHeight = true;
            buttonLayout.childControlWidth = true;
            buttonLayout.childForceExpandWidth = true;
            InstantiatePrefab(primaryButtonPrefab, buttons.transform);
            InstantiatePrefab(secondaryButtonPrefab, buttons.transform);
            InstantiatePrefab(subtleButtonPrefab, buttons.transform);
            InstantiatePrefab(destructiveButtonPrefab, buttons.transform);
            InstantiatePrefab(successButtonPrefab, buttons.transform);

            var states = UIObject("Control States", card.transform, new Vector2(0f, 76f));
            var stateLayout = states.AddComponent<HorizontalLayoutGroup>();
            stateLayout.spacing = 20f;
            stateLayout.childControlHeight = true;
            stateLayout.childControlWidth = true;
            stateLayout.childForceExpandWidth = true;
            InstantiatePrefab(togglePrefab, states.transform);
            InstantiatePrefab(sliderPrefab, states.transform);
            var disabled = InstantiatePrefab(secondaryButtonPrefab, states.transform);
            disabled.name = "Disabled Button";
            disabled.GetComponent<Button>().interactable = false;
            disabled.GetComponentInChildren<TMP_Text>().text = "Disabled";

            var lower = UIObject("Panels", card.transform, new Vector2(0f, 360f));
            var lowerLayout = lower.AddComponent<HorizontalLayoutGroup>();
            lowerLayout.spacing = 18f;
            lowerLayout.childControlHeight = true;
            lowerLayout.childControlWidth = true;
            lowerLayout.childForceExpandWidth = true;
            InstantiatePrefab(scrollPanelPrefab, lower.transform);
            InstantiatePrefab(modalPrefab, lower.transform);

            var caption = InstantiatePrefab(captionPrefab, card.transform);
            caption.GetComponent<TMP_Text>().text = "The consuming project supplies its XR Origin and interactors.";
            return SavePrefab(root, Gallery + "/Visual Gallery.prefab");
        }

        private static void CreateVisualGalleryScene(GameObject galleryPrefab)
        {
            const string temporaryScene = "Assets/__MixedUITempVisualGallery.unity";
            Directory.CreateDirectory(Path.GetFullPath(Sample));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Viewer Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -1.5f);
            cameraObject.transform.rotation = Quaternion.identity;

            var eventSystem = new GameObject("XRI UI EventSystem", typeof(EventSystem));
            eventSystem.AddComponent<XRUIInputModule>();
            eventSystem.AddComponent<CanvasOptimizer>();

            var gallery = (GameObject)PrefabUtility.InstantiatePrefab(galleryPrefab);
            gallery.transform.position = Vector3.zero;
            gallery.GetComponent<Canvas>().worldCamera = cameraObject.GetComponent<Camera>();

            EditorSceneManager.SaveScene(scene, temporaryScene);
            AssetDatabase.ImportAsset(temporaryScene, ImportAssetOptions.ForceUpdate);
            File.Copy(Path.GetFullPath(temporaryScene), Path.GetFullPath(Sample + "/Visual Gallery.unity"), true);
            File.Copy(Path.GetFullPath(temporaryScene + ".meta"), Path.GetFullPath(Sample + "/Visual Gallery.unity.meta"), true);
            AssetDatabase.DeleteAsset(temporaryScene);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void EnsureTemporaryTMPSettings()
        {
            if (TMP_Settings.instance != null) return;
            var settings = ScriptableObject.CreateInstance<TMP_Settings>();
            typeof(TMP_Settings).GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic)
                ?.SetValue(null, settings);
        }

        private static GameObject UIObject(string name, Transform parent, Vector2 size)
        {
            var value = new GameObject(name, typeof(RectTransform));
            value.transform.SetParent(parent, false);
            value.GetComponent<RectTransform>().sizeDelta = size;
            return value;
        }

        private static void Stretch(RectTransform rect, float horizontal, float vertical)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(horizontal, vertical);
            rect.offsetMax = new Vector2(-horizontal, -vertical);
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent) =>
            (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);

        private static GameObject SavePrefab(GameObject root, string path)
        {
            AssetDatabase.DeleteAsset(path);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void BakeTheme(GameObject prefab, MixedUITheme theme)
        {
            var path = AssetDatabase.GetAssetPath(prefab);
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (var style in root.GetComponentsInChildren<MixedUIStyleBehaviour>(true))
                    style.ApplyTheme(theme);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void Set(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            var field = serialized.FindProperty(property)
                ?? throw new MissingFieldException(target.GetType().Name, property);
            field.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnum(Object target, string property, int value)
        {
            var serialized = new SerializedObject(target);
            var field = serialized.FindProperty(property)
                ?? throw new MissingFieldException(target.GetType().Name, property);
            field.enumValueIndex = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBool(Object target, string property, bool value)
        {
            var serialized = new SerializedObject(target);
            var field = serialized.FindProperty(property)
                ?? throw new MissingFieldException(target.GetType().Name, property);
            field.boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
