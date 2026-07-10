# Mixed Ui

Mixed Ui is a Unity 6 package for building reusable, consistently styled, context-aware mixed-reality interfaces with Unity's built-in uGUI Canvas system.

The package focuses on a common runtime UI problem: opening a menu for one specific scene object and ensuring every control operates on that object without relying on global selection state. A visibility toggle, scale slider, delete button, or future custom control receives its target once through a typed contextual binding.

The package is located at [`Packages/com.evgh.mixed-ui`](Packages/com.evgh.mixed-ui). This repository is also a minimal Unity project that can compile and test the embedded package directly.

## Contents

- [Features](#features)
- [Requirements and compatibility](#requirements-and-compatibility)
- [Installation](#installation)
- [Required Unity setup](#required-unity-setup)
- [Quick start](#quick-start)
- [Implementing a target](#implementing-a-target)
- [Creating and opening the UI](#creating-and-opening-the-ui)
- [Understanding the builder API](#understanding-the-builder-api)
- [Targets, anchors, parents, and viewers](#targets-anchors-parents-and-viewers)
- [Themes and prefabs](#themes-and-prefabs)
- [Using individual controls](#using-individual-controls)
- [Commands](#commands)
- [Target lifetime and cleanup](#target-lifetime-and-cleanup)
- [XRI interaction setup](#xri-interaction-setup)
- [Extending the package](#extending-the-package)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)
- [Current limitations](#current-limitations)

## Features

- Built on standard `Canvas`, `Button`, `Toggle`, `Slider`, layout groups, and TextMeshPro components.
- World-space UI sized with a default convention of 1,000 Canvas units per meter.
- Typed contextual binding: a menu is bound to exactly one target at a time.
- Explicit target lifetime notifications and Unity destroyed-object detection.
- Semantic theme variants such as primary, secondary, destructive, and success.
- Serialized prefab catalog with no `Resources.Load` or string-based prefab paths.
- Thin, typed runtime builders for object-specific UI.
- Command binding with `CanExecute` and explicit interactability refresh.
- Instance handles that own close and release behavior.
- Replaceable instantiate/release provider for future pooling.
- Spatial placement kept separate from the target object.
- XRI dependencies isolated in the `Evgh.MixedUI.XRI` assembly.
- Placeholder theme, element prefabs, composite prefab, tests, and sample scripts.

The runtime does not use scene-wide searches, hierarchy-name searches, global selected-object state, or a monolithic UI manager.

## Requirements and compatibility

| Requirement | Version or expectation |
| --- | --- |
| Unity | `6000.0` or newer |
| Validation Editor | `6000.4.2f1` |
| Unity UI | uGUI `2.0.0`, bundled with Unity 6 |
| Text | TextMeshPro from the Unity UI package |
| Input System | `1.17.0` in this host project |
| XR Interaction Toolkit | `3.0.9` baseline |
| Initial MR interaction | XRI tracked-device rays |

The core `Evgh.MixedUI` assembly uses uGUI and TextMeshPro but does not reference XRI classes. Only `Evgh.MixedUI.XRI` contains XRI-specific code.

## Installation

### Use this repository directly

Open the repository root as a Unity project with Unity `6000.4.2f1`. The package is already embedded under `Packages/com.evgh.mixed-ui`.

### Copy it into another Unity project

Copy the complete directory:

```text
Packages/com.evgh.mixed-ui
```

into the consuming project's `Packages` directory. Unity detects the package from its `package.json`.

Do not copy only the C# files. The package also contains assembly definitions, prefab assets, the default theme, tests, sample content, and documentation.

### Install from a Git repository

If this package is hosted in a Git repository below the repository root, use Unity's package subdirectory syntax in the consuming project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.evgh.mixed-ui": "https://example.com/your-repository.git?path=/Packages/com.evgh.mixed-ui#v0.1.0"
  }
}
```

Replace the example URL and revision with the actual repository location and tag or commit.

## Required Unity setup

Complete these steps before opening the first menu.

### 1. Import TextMeshPro resources

In the consuming project, select:

```text
Window > TextMeshPro > Import TMP Essential Resources
```

The package deliberately does not install a second global `TMP Settings` resource. This avoids conflicts with a consuming project's existing fonts, fallback configuration, and TMP settings.

After importing the resources, assign a default TMP font to your theme if the supplied theme's font field is empty.

### 2. Enable the Input System

Set **Active Input Handling** to **Input System Package (New)** or **Both**:

```text
Edit > Project Settings > Player > Other Settings > Configuration
```

The included host project already uses the new Input System.

### 3. Configure the EventSystem

For XRI input, the scene must contain one consumer-owned `EventSystem` with exactly one active input module:

```text
EventSystem
└── XRUIInputModule
```

Remove competing `StandaloneInputModule` and `InputSystemUIInputModule` components from that EventSystem. The adapter requires exactly one `BaseInputModule` component in total, so merely disabling an extra module is not sufficient. `XRIWorldSpaceCanvasAdapter` validates this configuration and throws a descriptive error instead of silently changing scene-wide input behavior.

### 4. Configure the XR ray interactor

The XR ray or near-far interactor used by the application must have UI interaction enabled. The specific rig and input actions belong to the consuming application, not this package.

### 5. Create a UI parent

Create an empty scene object to organize generated menu instances:

```text
Runtime UI Root
```

This parent controls hierarchy ownership only. It does not determine which object the menu edits or where the menu is placed.

## Quick start

The smallest complete setup has four pieces:

1. A theme and prefab catalog.
2. A viewer camera and correctly configured EventSystem.
3. A target implementing `IConfigurableObject`.
4. A `UIComposer` used to open the menu.

Add serialized references to a scene component:

```csharp
using Evgh.MixedUI;
using Evgh.MixedUI.XRI;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class ConfigurationMenuOpener : MonoBehaviour
{
    [SerializeField] private UITheme theme;
    [SerializeField] private UIPrefabCatalog prefabCatalog;
    [SerializeField] private Camera viewer;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Transform uiRoot;
    [SerializeField] private ConfigurableSceneObject target;

    private UIComposer _ui;
    private UIInstanceHandle<ObjectConfigurationMenu> _openMenu;

    private void Awake()
    {
        var canvasAdapter = new XRIWorldSpaceCanvasAdapter(eventSystem);
        var factory = new UIFactory(
            prefabCatalog,
            theme,
            canvasAdapter: canvasAdapter);

        var context = new UIContext(theme, factory, viewer.transform);
        _ui = new UIComposer(context);
    }

    public void OpenConfiguration()
    {
        _openMenu?.Close();

        _openMenu = _ui
            .For<IConfigurableObject>(target)
            .AnchoredTo(target.transform)
            .WithLocalOffset(new Vector3(0.15f, 0.1f, 0f))
            .FaceUser()
            .ConfigurationPanel("Object Settings")
            .ShowVisibilityControl()
            .ShowScaleControl(0.1f, 3f)
            .ShowDeleteAction()
            .Build(uiRoot);
    }

    private void OnDestroy()
    {
        _openMenu?.Close();
    }
}
```

In the Inspector, assign:

- `DefaultUITheme` from `Packages/com.evgh.mixed-ui/Runtime/Assets/Themes`.
- `DefaultUIPrefabCatalog` from `Packages/com.evgh.mixed-ui/Runtime/Assets`.
- The XR camera as `Viewer`.
- The scene's XRI EventSystem.
- The empty runtime UI root.
- The scene object implementing `IConfigurableObject`.

The package sample contains equivalent code in [`SampleMenuOpener.cs`](Packages/com.evgh.mixed-ui/Samples~/ObjectConfiguration/SampleMenuOpener.cs).

## Implementing a target

`IConfigurableObject` is a sample capability interface for the initial composite. It is not a mandatory base type for every application object.

```csharp
public interface IConfigurableObject
{
    string DisplayName { get; }
    bool IsVisible { get; set; }
    float Scale { get; set; }
    bool CanDelete { get; }
    void Delete();
}
```

A scene object can implement the interface without referencing any UI component:

```csharp
using System;
using Evgh.MixedUI;
using UnityEngine;

public sealed class ConfigurableSceneObject : MonoBehaviour,
    IConfigurableObject,
    IUITargetLifetime
{
    [SerializeField] private string displayName = "Scene Object";
    [SerializeField] private Renderer[] controlledRenderers = Array.Empty<Renderer>();
    [SerializeField] private bool canDelete = true;

    private bool _isValid = true;

    public string DisplayName => displayName;
    public bool CanDelete => canDelete && _isValid;
    public bool IsValid => _isValid && this != null;

    public event Action Invalidated;

    public bool IsVisible
    {
        get => controlledRenderers.Length == 0 || controlledRenderers[0].enabled;
        set
        {
            foreach (var item in controlledRenderers)
            {
                if (item != null)
                    item.enabled = value;
            }
        }
    }

    public float Scale
    {
        get => transform.localScale.x;
        set => transform.localScale = Vector3.one * Mathf.Max(0.001f, value);
    }

    public void Delete()
    {
        if (!CanDelete)
            return;

        Invalidate();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Invalidate();
    }

    private void Invalidate()
    {
        if (!_isValid)
            return;

        _isValid = false;
        Invalidated?.Invoke();
    }
}
```

The target owns domain behavior. It does not know about `UIButton`, `ObjectConfigurationMenu`, the prefab factory, or XRI.

## Creating and opening the UI

### Create the package services once

The normal application flow constructs one `UIFactory`, `UIContext`, and `UIComposer` for the relevant scene or application scope:

```csharp
var adapter = new XRIWorldSpaceCanvasAdapter(eventSystem);
var provider = new InstantiateUIObjectProvider();
var factory = new UIFactory(prefabCatalog, theme, provider, adapter);
var context = new UIContext(theme, factory, viewer.transform);
var ui = new UIComposer(context);
```

These are ordinary objects, not global singletons. Pass or serialize the owner that exposes the composer according to the consuming application's architecture.

For non-XR desktop testing, omit the XRI adapter:

```csharp
var factory = new UIFactory(prefabCatalog, theme);
```

The default `UGUIWorldSpaceCanvasAdapter` configures a normal `GraphicRaycaster` instead.

### Build a configuration menu

```csharp
UIInstanceHandle<ObjectConfigurationMenu> handle = ui
    .For<IConfigurableObject>(target)
    .AnchoredTo(anchor)
    .WithLocalOffset(new Vector3(0.15f, 0.1f, 0f))
    .FaceUser()
    .ConfigurationPanel("Object Settings")
    .ShowVisibilityControl()
    .ShowScaleControl(0.1f, 3f)
    .ShowDeleteAction()
    .Build(uiRoot);
```

`Build` performs the following operations:

1. Validates the target, parent, anchor, slider range, theme, catalog, prefab, viewer, and Canvas configuration.
2. Instantiates the configuration-menu prefab.
3. Applies the current theme.
4. Configures its world-space Canvas through the selected adapter.
5. Applies the spatial context.
6. Binds the target once.
7. Returns a handle that owns release.

Builders are single-use. Calling `Build` twice on the same builder throws an `InvalidOperationException`.

### Close a menu explicitly

```csharp
handle.Close();
```

`Close` is idempotent. Calling it multiple times is safe. Accessing `handle.Instance` after close throws `ObjectDisposedException`.

Keeping the handle is optional when the menu can close only through its own Close/Delete buttons or target invalidation.

## Understanding the builder API

### `For<TTarget>(target)`

Starts a typed contextual builder. The target is supplied once and remains strongly typed throughout configuration.

```csharp
ui.For<IConfigurableObject>(target)
```

Null targets are rejected immediately. Destroyed Unity objects and targets whose `IUITargetLifetime.IsValid` is false are rejected during binding.

### `AnchoredTo(transform)`

Sets the spatial anchor used to calculate the menu's world position and initial rotation. This call is required before `Build`.

The anchor is not required to be the target's transform:

```csharp
.AnchoredTo(menuAnchor)
```

### `WithLocalOffset(offset)`

Interprets the offset in anchor-local space:

```csharp
.WithLocalOffset(new Vector3(0.15f, 0.1f, 0f))
```

Rotating the anchor also rotates the offset direction.

### `WithWorldOffset(offset)`

Adds the offset directly in world space:

```csharp
.WithWorldOffset(Vector3.up * 0.1f)
```

The last local/world offset method called determines the offset mode.

### `FaceUser()`

Rotates the panel so its front faces the `UIContext.Viewer` while preserving world up. The viewer is explicit; the package does not call `Camera.main`.

### `FollowAnchor()`

Updates position and facing during `LateUpdate`:

```csharp
.AnchoredTo(movingObject.transform)
.FollowAnchor()
```

Without this call, placement is calculated once when the menu is built.

### `ConfigurationPanel(title)`

Selects the concrete `ObjectConfigurationMenu` composite. The displayed heading includes the panel title and the bound target's `DisplayName`.

### Optional controls

```csharp
.ShowVisibilityControl()
.ShowScaleControl(0.1f, 3f)
.ShowDeleteAction()
```

Controls not selected by the builder are hidden. Scale minimum must be strictly less than maximum.

## Targets, anchors, parents, and viewers

These references have distinct responsibilities:

| Reference | Responsibility | Typical value |
| --- | --- | --- |
| Target | Supplies and receives business data | Object implementing `IConfigurableObject` |
| Anchor | Determines spatial placement | Target transform, attachment point, or shared menu anchor |
| Parent | Owns the generated hierarchy | `Runtime UI Root` |
| Viewer | Determines facing and XRI event camera | XR head camera |

They may point to different objects. In particular, `Build(parent)` never changes which target receives toggle, slider, or command operations.

## Themes and prefabs

### Default assets

The package includes:

```text
Runtime/Assets/
├── DefaultUIPrefabCatalog.asset
├── Themes/
│   ├── DefaultUITheme.asset
│   └── RoundedSurface.png
└── Prefabs/
    ├── UILabel.prefab
    ├── UIButton.prefab
    ├── UIToggle.prefab
    ├── UISlider.prefab
    ├── UIStack.prefab
    ├── UICard.prefab
    └── ObjectConfigurationMenu.prefab
```

The supplied assets are functional placeholders intended to establish hierarchy, serialized references, interaction, and sizing. Duplicate them into the application if they will receive project-specific art changes.

### Theme tokens

`UITheme` groups tokens by responsibility:

- `UIColorTokens`: surface, primary, secondary, subtle, destructive, success, text, disabled, hover, and pressed colors.
- `UISpacingTokens`: small, medium, and large spacing.
- `UITypographyTokens`: default TMP font plus body and heading sizes.
- `UIGeometryTokens`: standard control size, Canvas units per meter, and rounded surface sprite.
- `UIInteractionTokens`: pressed scale and transition duration.

Controls request `UIStyleVariant` values rather than arbitrary colors:

```csharp
button.SetStyleVariant(UIStyleVariant.Destructive);
```

Rounded corners are represented by a nine-sliced sprite because standard uGUI does not provide a numeric corner-radius property.

### Creating an application theme

1. Duplicate `DefaultUITheme.asset` into the application's `Assets` directory.
2. Assign the project's TMP font.
3. Adjust semantic colors and token values.
4. Assign the new asset when constructing `UIFactory` and `UIContext`.

Do not apply separate themes manually to each child. `UIFactory` applies the active theme to the created composite, which propagates it to its controls.

### Creating an application prefab catalog

1. Duplicate the package prefabs into the application if their hierarchy or graphics need customization.
2. Keep each wrapper's required serialized references assigned.
3. Duplicate `DefaultUIPrefabCatalog.asset`.
4. Replace catalog references with the customized prefabs.
5. Pass that catalog to `UIFactory`.

External code should use typed component properties and explicit card slots rather than searching prefab children by name.

### Regenerating placeholders

Use:

```text
Tools > Mixed-UI > Regenerate Default Assets
```

The generator replaces the package's default theme, catalog, and prefabs. Do not customize those package assets directly if this command will be used; duplicate them into `Assets` first.

## Using individual controls

The initial elements share the `IUIElement` contract:

```csharp
public interface IUIElement
{
    GameObject GameObject { get; }
    Transform Transform { get; }
    RectTransform RectTransform { get; }
    void ApplyTheme(UITheme theme);
    void SetVisible(bool visible);
    void SetInteractable(bool interactable);
}
```

### Label

```csharp
UILabel label = factory.CreateLabel(parent);
label.Text = "Object name";
label.SetStyle(UILabelStyle.Heading);
```

### Button

```csharp
UIButton button = factory.CreateButton(parent);
button.Text = "Reset";
button.SetStyleVariant(UIStyleVariant.Secondary);
button.Pressed += ResetObject;
```

`Press()` invokes the same command/event path programmatically when the underlying uGUI button is interactable. Normal user input arrives through `Button.onClick`.

### Toggle

```csharp
UIToggle toggle = factory.CreateToggle(parent);
toggle.Text = "Visible";
toggle.SetValueWithoutNotify(target.IsVisible);
toggle.ValueChanged += value => target.IsVisible = value;
```

Use `SetValueWithoutNotify` during initialization or rebinding to prevent setup from being interpreted as user input.

### Slider

```csharp
UISlider slider = factory.CreateSlider(parent);
slider.Text = "Scale";
slider.SetRange(0.1f, 3f);
slider.SetValueWithoutNotify(target.Scale);
slider.ValueChanged += value => target.Scale = value;
```

`SetRange` throws when minimum is greater than or equal to maximum.

### Card and stack

`UICard` exposes explicit content slots:

```csharp
Transform header = card.HeaderRoot;
Transform content = card.ContentRoot;
Transform footer = card.FooterRoot;
```

`UIStack.ContentRoot` is its own transform and uses a standard horizontal or vertical uGUI layout group.

When creating individual elements directly, the caller owns their event subscriptions and lifetime. Contextual composites perform this wiring automatically.

## Commands

Use `UICommand<TTarget>` for consequential actions or actions with availability rules:

```csharp
var deleteCommand = new UICommand<IConfigurableObject>(
    target,
    value => value.Delete(),
    value => value.CanDelete);

deleteButton.BindCommand(deleteCommand);
```

`Execute` does nothing when `CanExecute` is false.

When state affecting `CanExecute` changes, notify the command:

```csharp
deleteCommand.NotifyCanExecuteChanged();
```

The bound button refreshes its `interactable` state through `CanExecuteChanged`.

Before reusing the button for unrelated behavior, remove its binding:

```csharp
deleteButton.UnbindCommand();
```

Confirmation dialogs and undo are intentionally not built into `UICommand<TTarget>`. Wrap or implement `IUICommand` for those policies.

## Target lifetime and cleanup

Targets with an explicit lifecycle should implement:

```csharp
public interface IUITargetLifetime
{
    bool IsValid { get; }
    event Action Invalidated;
}
```

Raise `Invalidated` before destroying or permanently retiring the object:

```csharp
public void Delete()
{
    _isValid = false;
    Invalidated?.Invoke();
    Destroy(gameObject);
}
```

When invalidation occurs, the configuration menu:

1. Disables interaction.
2. Unsubscribes every control and target callback.
3. Clears the contextual target reference.
4. Requests its instance handle to release the menu.

This happens immediately even though Unity may defer target destruction until the end of the frame.

For targets derived from `UnityEngine.Object`, the contextual base also recognizes Unity's destroyed-object null semantics. Implementing `IUITargetLifetime` is still preferred because it closes the menu immediately without waiting for the lifecycle check.

Rebinding an existing contextual element follows the same cleanup rules: the old target is completely unbound before the new target is stored.

## XRI interaction setup

`XRIWorldSpaceCanvasAdapter` configures each created menu Canvas by:

- Setting `RenderMode.WorldSpace`.
- Assigning the explicit viewer camera as `worldCamera`.
- Disabling and releasing the ordinary `GraphicRaycaster`.
- Adding `TrackedDeviceGraphicRaycaster` when absent.
- Verifying that the supplied EventSystem contains exactly one input-module component and that it is `XRUIInputModule`.

Create it with the scene's EventSystem:

```csharp
var adapter = new XRIWorldSpaceCanvasAdapter(eventSystem);
```

The adapter does not find or create an EventSystem. This prevents the package from unexpectedly replacing input modules used elsewhere in the scene.

### XRI checklist

- The viewer transform contains a `Camera`.
- One active `EventSystem` is used for the UI scope.
- That EventSystem has one `XRUIInputModule` component and no other input-module component, including disabled modules.
- The XR ray interactor has UI interaction enabled.
- Input actions needed by the XRI rig are enabled.
- UI graphics that should receive input have `Raycast Target` enabled.
- Decorative child graphics do not unnecessarily block raycasts.

## Extending the package

### Add a reusable element

1. Create a focused component derived from `UIElement`.
2. Serialize explicit uGUI/TMP child references.
3. Validate missing references in `OnValidate` and before use.
4. Apply semantic theme tokens in `OnThemeApplied`.
5. Expose normal C# events for application binding.
6. Subscribe and unsubscribe symmetrically in Unity lifecycle methods.
7. Add a typed prefab reference to `UIPrefabCatalog`.
8. Add a typed factory creation method.
9. Test theming, interactability, notifications, and cleanup.

Do not add target-specific domain behavior to reusable elements.

### Add a contextual composite

Derive from `ContextualUIElement<TTarget>` and implement:

```csharp
protected override void OnBind(UICreationContext<TTarget> context)
{
    // Initialize without notifying, then subscribe.
}

protected override void OnUnbind()
{
    // Remove every control, command, and target subscription.
}
```

The composite owns the target context. Its child controls remain reusable and domain-independent.

### Add another interaction framework

Implement `IWorldSpaceCanvasAdapter` in a separate assembly:

```csharp
public sealed class OtherFrameworkCanvasAdapter : IWorldSpaceCanvasAdapter
{
    public void Configure(Canvas canvas, Transform viewer)
    {
        // Configure the framework-specific Canvas/raycaster boundary.
    }
}
```

Do not introduce framework types into controls, commands, themes, contexts, or builders.

### Add pooling

Implement `IUIObjectProvider`:

```csharp
public interface IUIObjectProvider
{
    T Get<T>(T prefab, Transform parent) where T : Component;
    void Release<T>(T instance) where T : Component;
}
```

Pass the provider to `UIFactory`. The builder and contextual APIs do not need to change.

Pooled implementations must ensure instances are completely unbound and visually reset before reuse.

## Package structure

```text
Packages/com.evgh.mixed-ui/
├── Runtime/
│   ├── Core/           Context and element lifetime contracts
│   ├── Styling/        Theme and semantic tokens
│   ├── Elements/       Label, button, toggle, and slider
│   ├── Layout/         Card and stack
│   ├── Commands/       Command contracts and typed command
│   ├── Context/        Target capabilities and configuration menu
│   ├── Factory/        Catalog, provider, factory, Canvas boundary
│   ├── Builders/       Typed contextual builder API
│   ├── Spatial/        Placement and following
│   ├── Interaction/    Framework-independent visual state
│   ├── Adapters/XRI/   XRI-specific Canvas adapter assembly
│   └── Assets/         Placeholder theme, catalog, and prefabs
├── Editor/             Default asset generator
├── Tests/              EditMode and PlayMode tests
├── Samples~/           Importable object-configuration sample
└── Documentation~/     Package Manager documentation entry
```

## Testing

### Unity Test Runner

Open:

```text
Window > General > Test Runner
```

Run both EditMode and PlayMode suites.

### Batch mode

From the repository root on this development machine:

```bash
/home/evgh/Unity/Hub/Editor/6000.4.2f1/Editor/Unity \
  -batchmode \
  -projectPath /home/evgh/Fun/mixed-ui \
  -runTests \
  -testPlatform EditMode \
  -testResults /tmp/mixed-ui-edit-results.xml \
  -logFile /tmp/mixed-ui-edit-tests.log
```

```bash
/home/evgh/Unity/Hub/Editor/6000.4.2f1/Editor/Unity \
  -batchmode \
  -projectPath /home/evgh/Fun/mixed-ui \
  -runTests \
  -testPlatform PlayMode \
  -testResults /tmp/mixed-ui-play-results.xml \
  -logFile /tmp/mixed-ui-play-tests.log
```

The current implementation passes eight EditMode tests and two PlayMode tests. No test requires XR hardware.

Coverage includes:

- Binding and rebinding order.
- Subscription cleanup.
- Command targeting and `CanExecute` behavior.
- No-notify control initialization.
- Delete behavior against the current target.
- Target invalidation and release.
- Builder type and instance preservation.
- Single-use builder validation.
- Theme application and missing prefab errors.
- Anchor/parent separation.
- Delayed Unity destruction.
- XRI EventSystem and raycaster validation.

## Troubleshooting

### Text is missing or TMP reports missing resources

Import TMP Essential Resources and assign a TMP font in the active `UITheme`.

### `The supplied EventSystem must contain exactly one active input module`

Remove competing input modules and keep one `XRUIInputModule` on the EventSystem passed to `XRIWorldSpaceCanvasAdapter`.

### `The viewer transform ... requires a Camera component`

Pass the XR head camera's transform to `UIContext`, not the XR rig root or an arbitrary tracking transform.

### XR rays hover over the menu but do not click

Check that the ray interactor has UI interaction enabled, its input actions are active, and the scene uses `XRUIInputModule`.

### The menu is in the wrong position

Verify whether the offset should be local or world space. `WithLocalOffset` is rotated by the anchor; `WithWorldOffset` is not.

Also remember that `Build(uiRoot)` sets the hierarchy parent, not the spatial anchor.

### The menu does not move with its object

Add `.FollowAnchor()` to the builder. Placement is static by default.

### The menu faces away from the viewer

Verify the root orientation of custom world-space Canvas prefabs. The supplied prefab follows the expected uGUI forward orientation. Customized prefabs should preserve that root orientation.

### The delete button is disabled

The bound target returned `false` from `CanDelete`. If availability changes while the menu is open, update the associated command and call `NotifyCanExecuteChanged`.

### A menu still references an old target

Custom contextual composites must remove every subscription in `OnUnbind`. Use `SetValueWithoutNotify` before subscribing when initializing controls for the new target.

### `UIPrefabCatalog is missing its required ... prefab`

Assign every prefab required by the factory call, or restore the default catalog. The error intentionally occurs before the package attempts to use a partially configured instance.

### Changes to package placeholder prefabs disappeared

The default asset generator replaces package-owned placeholder assets. Duplicate customized assets into the application's `Assets` directory and reference them from an application-owned catalog.

## Current limitations

- Initial XRI support covers tracked-device rays, not direct near-poke interaction.
- Control values are read when binding and written on user changes. External changes to target properties are not automatically observed while the menu remains open.
- The default object provider uses `Instantiate` and `Destroy`; pooling is an extension point rather than a supplied implementation.
- The initial builder supports the concrete object-configuration menu rather than a fully generic declarative binding language.
- Undoable commands, confirmation-dialog policy, and MRTK adapters are not included in v0.1.
- Placeholder visuals are intended for architecture and testing, not final production art.

## Additional documentation

- [Package README](Packages/com.evgh.mixed-ui/README.md)
- [Object Configuration sample](Packages/com.evgh.mixed-ui/Samples~/ObjectConfiguration/README.md)
- [Original implementation plan](PLANS.md)
- [Package changelog](Packages/com.evgh.mixed-ui/CHANGELOG.md)
