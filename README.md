# Mixed UI

Mixed UI is a Unity 6 visual component kit for XR Interaction Toolkit world-space uGUI.

It supplies a neutral-dark mixed-reality theme, reusable control and layout prefabs, XRI-ready Canvas prefabs, and small style components. XRI owns ray and poke input. uGUI owns clicks, values, navigation, and interactability. Mixed UI only owns presentation.

- Package ID: `com.evgh.mixed-ui`
- Current version: `0.2.0`
- Namespace: `Evgh.MixedUI`

## What the package includes

- World-space Canvas configured with `TrackedDeviceGraphicRaycaster`.
- Head-follow Canvas configured with XRI `LazyFollow`.
- Body, heading, and caption TMP labels.
- Primary, secondary, subtle, destructive, and success buttons.
- Toggle and slider controls.
- Card, vertical stack, horizontal stack, modal, and scroll panel layouts.
- A runtime-switchable `MixedUITheme`.
- A visual gallery prefab and importable sample scene.
- EditMode and PlayMode validation without requiring XR hardware.

Mixed UI does not contain an application UI framework. It has no factory, command system, contextual target model, builder API, selection state, or custom input event layer.

## Requirements

| Dependency | Version |
| --- | --- |
| Unity | `6000.0` or newer |
| Unity UI | `2.0.0` |
| XR Interaction Toolkit | `3.0.9` |
| TextMeshPro | Included in Unity UI 2.0 |

The repository is validated with Unity `6000.4.2f1`.

## Installation

### Unity Package Manager from Git

Open **Window → Package Management → Package Manager**, select **Install package from Git URL**, and enter:

```text
https://github.com/evgh0/mixed-ui.git?path=/Packages/com.evgh.mixed-ui#main
```

The GitHub `/tree/main/...` webpage URL cannot be installed directly.

You can also add the dependency manually:

```json
{
  "dependencies": {
    "com.evgh.mixed-ui": "https://github.com/evgh0/mixed-ui.git?path=/Packages/com.evgh.mixed-ui#main"
  }
}
```

### Local package

In Package Manager, select **Install package from disk** and choose:

```text
Packages/com.evgh.mixed-ui/package.json
```

Alternatively, copy the complete `Packages/com.evgh.mixed-ui` directory into another project's `Packages` directory.

## Required project setup

### Import TMP resources

Select:

```text
Window → TextMeshPro → Import TMP Essential Resources
```

Then assign the consuming project's TMP font to `DefaultMixedUITheme` or to an application-owned theme copy.

### Configure XRI UI input

Use XRI's built-in helpers:

```text
GameObject → XR → UI EventSystem
```

The scene should contain:

```text
EventSystem
├── EventSystem
├── XRUIInputModule
└── CanvasOptimizer (optional)
```

The application's XR ray and poke interactors must have UI interaction enabled. Mixed UI does not create or configure an XR Origin.

## Quick start

1. Drag `World Space Canvas.prefab` into the scene.
2. Assign its Canvas **Event Camera** to the XR head camera.
3. Add control and layout prefabs beneath its `RectTransform`.
4. Configure uGUI events in the Inspector or normal C# code.
5. Ensure the scene has an `XRUIInputModule` and UI-enabled interactors.

Prefab location:

```text
Packages/com.evgh.mixed-ui/Runtime/Assets/Prefabs
```

Theme location:

```text
Packages/com.evgh.mixed-ui/Runtime/Assets/Themes/DefaultMixedUITheme.asset
```

## Standard uGUI behavior

Mixed UI prefabs expose the standard component APIs:

```csharp
using UnityEngine;
using UnityEngine.UI;

public sealed class SettingsView : MonoBehaviour
{
    [SerializeField] private Button applyButton;
    [SerializeField] private Toggle visibilityToggle;
    [SerializeField] private Slider scaleSlider;

    private void OnEnable()
    {
        applyButton.onClick.AddListener(Apply);
        visibilityToggle.onValueChanged.AddListener(SetVisible);
        scaleSlider.onValueChanged.AddListener(SetScale);
    }

    private void OnDisable()
    {
        applyButton.onClick.RemoveListener(Apply);
        visibilityToggle.onValueChanged.RemoveListener(SetVisible);
        scaleSlider.onValueChanged.RemoveListener(SetScale);
    }

    private void Apply() { }
    private void SetVisible(bool value) { }
    private void SetScale(float value) { }
}
```

There is no Mixed UI click event to learn. `Button.onClick`, `Toggle.onValueChanged`, `Slider.onValueChanged`, `Selectable.interactable`, and `SetValueWithoutNotify` behave exactly as they do in ordinary uGUI.

XRI sends pointer events through `XRUIInputModule`; the same built-in `Selectable.ColorBlock` drives highlighted, pressed, selected, and disabled visuals.

## Canvas prefabs

### World Space Canvas

Contains:

- `Canvas` in World Space mode.
- `CanvasScaler` using 1,000 dynamic pixels per unit.
- `TrackedDeviceGraphicRaycaster`.
- `MixedUIThemeScope` with the default theme.

It intentionally does not contain a standard `GraphicRaycaster` or a custom adapter script.

### Head Follow Canvas

Contains the same XRI/uGUI components plus XRI `LazyFollow` configured for position following and `LookAtWithWorldUp` rotation.

By default, `LazyFollow` uses the main camera when no target is assigned. For deterministic behavior, assign the XR head camera transform to `LazyFollow.target`.

### Canvas Optimizer

The visual gallery includes one `CanvasOptimizer` on its EventSystem. A Canvas present when the scene starts is registered automatically.

If the application instantiates a Canvas later, register it explicitly:

```csharp
canvasOptimizer.RegisterCanvas(canvas);
```

Unregister it before a runtime-destroyed Canvas is released:

```csharp
canvasOptimizer.UnregisterCanvas(canvas);
```

## Control prefabs

### Labels

- `Body Label.prefab`
- `Heading Label.prefab`
- `Caption Label.prefab`

Each contains `TextMeshProUGUI` plus `MixedUITextStyle`. Change the text through the normal TMP component.

### Buttons

- `Button Primary.prefab`
- `Button Secondary.prefab`
- `Button Subtle.prefab`
- `Button Destructive.prefab`
- `Button Success.prefab`

Each contains `Image`, `Button`, TMP text, and `MixedUIButtonStyle`.

Use semantic variants consistently:

- Primary: main action in the current context.
- Secondary: ordinary action.
- Subtle: low-emphasis action.
- Destructive: irreversible or damaging action.
- Success: completion or positive confirmation.

The variant can also be changed at runtime:

```csharp
buttonStyle.SetVariant(MixedUIVariant.Destructive);
```

### Toggle

`Toggle.prefab` is a standard `Toggle` with explicit background, checkmark, label, and `MixedUIToggleStyle` references.

Initialize without invoking application callbacks:

```csharp
toggle.SetIsOnWithoutNotify(initialValue);
```

### Slider

`Slider.prefab` is a standard `Slider` with explicit track, fill, handle, label, and `MixedUISliderStyle` references.

Configure values normally:

```csharp
slider.minValue = 0.1f;
slider.maxValue = 3f;
slider.SetValueWithoutNotify(initialScale);
```

## Layout prefabs

- `Card.prefab`: rounded surface, `CanvasGroup`, and vertical layout.
- `Vertical Stack.prefab`: `VerticalLayoutGroup` plus `ContentSizeFitter`.
- `Horizontal Stack.prefab`: `HorizontalLayoutGroup` plus `ContentSizeFitter`.
- `Modal.prefab`: elevated surface, heading, body, Cancel and Confirm buttons.
- `Scroll Panel.prefab`: themed surface with `ScrollRect`, masked viewport, and vertical content.

These prefabs contain presentation and standard uGUI components only. Modal visibility, confirmation logic, navigation, and data binding belong to the consuming application.

## Theme system

`MixedUITheme` is a small visual asset containing:

- Semantic color palette.
- TMP font and body/heading/caption sizes.
- Small, medium, and large spacing.
- Canvas units per meter and standard control size.
- Selectable fade and state-blend values.
- Nine-sliced control and surface sprites.

### Create a theme

Select:

```text
Create → Mixed-UI → Theme
```

For application-specific styling, duplicate the package theme into `Assets` and edit that copy.

### Apply a theme

Assign it to the `MixedUIThemeScope` on a Canvas. The scope applies it to all `MixedUIStyleBehaviour` children.

Apply changes explicitly:

```csharp
themeScope.ApplyTheme();
```

Switch the complete theme at runtime:

```csharp
themeScope.Theme = darkTheme;
```

### Style bindings

- `MixedUIButtonStyle` writes a semantic `ColorBlock`, control sprite, TMP font, size, and text color.
- `MixedUIToggleStyle` writes toggle states, background, checkmark, and label visuals.
- `MixedUISliderStyle` writes handle states, track/fill/handle sprites, and label visuals.
- `MixedUITextStyle` writes TMP font, role size, and primary/secondary text color.
- `MixedUISurfaceStyle` writes surface color and nine-sliced sprite.

Style bindings never subscribe to application events or change domain values.

## Visual gallery sample

Import **Visual Gallery** from the Mixed UI package's Samples tab.

The sample includes:

- A world-space XRI Canvas.
- All semantic button variants.
- Toggle, slider, and disabled control states.
- Card, modal, and scroll panel examples.
- `EventSystem`, `XRUIInputModule`, and `CanvasOptimizer`.
- A camera so the scene can load and validate without XR hardware.

Add or merge the gallery into a scene with an XR Origin to test ray and poke interaction on a headset or simulator.

## Regenerating package assets

Run:

```text
Tools → Mixed-UI → Regenerate Default Assets
```

This replaces all package-owned default prefabs, the default theme, rounded sprite import settings, gallery prefab, and sample scene.

Do not customize generated package assets directly. Duplicate them into the application's `Assets` directory first.

## Extending the visual kit

To add a styled uGUI control:

1. Use the standard uGUI component as the behavior API.
2. Derive a visual binding from `MixedUIStyleBehaviour`.
3. Serialize explicit references to graphics and TMP text.
4. Apply only properties from `MixedUITheme`.
5. Add a prefab and validation tests.

Do not add commands, target models, data binding, factories, scene searches, or input abstractions to a style component.

## Migrating from 0.1.0

Version 0.2.0 is intentionally breaking.

| Removed 0.1 API | 0.2 replacement |
| --- | --- |
| `UIButton` | `UnityEngine.UI.Button` + `MixedUIButtonStyle` |
| `UIToggle` | `UnityEngine.UI.Toggle` + `MixedUIToggleStyle` |
| `UISlider` | `UnityEngine.UI.Slider` + `MixedUISliderStyle` |
| `UILabel` | `TextMeshProUGUI` + `MixedUITextStyle` |
| `UIVisualStateController` | uGUI `Selectable.ColorBlock` |
| `UITheme` | `MixedUITheme` |
| `UIFactory` / `UIPrefabCatalog` | Direct prefab references or the consuming project's asset system |
| `UIComposer` / contextual builders | Consumer-owned view/controller code and normal UnityEvents |
| `IUICommand` / `UICommand<T>` | `Button.onClick` or application command architecture |
| `UISpatialFollower` | XRI `LazyFollow` |
| `XRIWorldSpaceCanvasAdapter` | XRI-ready Canvas prefabs |
| Object configuration sample | Visual Gallery sample |

Remove missing 0.1 components from existing prefabs and replace them with the standard uGUI component plus the corresponding style binding.

## Testing

Run both suites from **Window → General → Test Runner**.

Batch mode:

```bash
/home/evgh/Unity/Hub/Editor/6000.4.2f1/Editor/Unity \
  -batchmode -projectPath /home/evgh/Fun/mixed-ui \
  -runTests -testPlatform EditMode \
  -testResults /tmp/mixed-ui-edit.xml
```

```bash
/home/evgh/Unity/Hub/Editor/6000.4.2f1/Editor/Unity \
  -batchmode -projectPath /home/evgh/Fun/mixed-ui \
  -runTests -testPlatform PlayMode \
  -testResults /tmp/mixed-ui-play.xml
```

Tests cover prefab integrity, standard component composition, XRI Canvas setup, semantic theme application, runtime theme replacement, standard uGUI callbacks, gallery loading, and XRI `LazyFollow` without XR hardware.

## Troubleshooting

### Text is invisible

Import TMP Essential Resources and assign a TMP font to the active `MixedUITheme`.

### XR ray or poke does not interact

Verify that the scene uses `XRUIInputModule`, the Canvas contains `TrackedDeviceGraphicRaycaster`, and the interactor has UI interaction enabled.

### The Canvas cannot be hit

Verify that it is in World Space mode, its event camera is the XR head camera, and interactive `Graphic` components have `Raycast Target` enabled.

### Theme changes are not visible

Call `MixedUIThemeScope.ApplyTheme()` after editing a theme at runtime, or reassign the `Theme` property.

### Head-follow Canvas reports an unassigned target

Assign the XR head camera transform to `LazyFollow.target`, or ensure the scene camera is tagged `MainCamera` before the Canvas is enabled.

## Limitations

- The package styles Canvas-based uGUI; it does not ship collider-based `XRSimpleInteractable` controls.
- The consuming application owns XR Origin, interactors, input actions, data binding, navigation policy, and modal behavior.
- The default font is not embedded; use the project's TMP resources and font assets.
- Visuals are a coherent neutral-dark baseline, not a complete application brand.

## Package structure

```text
Packages/com.evgh.mixed-ui/
├── Runtime/
│   ├── Styling/       Theme scope and visual bindings
│   └── Assets/        Themes and XRI/uGUI prefabs
├── Editor/            Default asset generator
├── Tests/             EditMode and PlayMode validation
├── Samples~/          Importable Visual Gallery
└── Documentation~/    Package Manager documentation entry
```
