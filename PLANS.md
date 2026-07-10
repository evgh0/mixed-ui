You are working in an existing Unity Mixed Reality project. Implement the initial architecture for a reusable, uniformly styled, context-aware runtime UI system.

The UI system must support opening menus and controls for a specific underlying scene object. For example, a configuration menu, visibility toggle, scale slider, or delete button must operate on the object for which the UI was opened.

## Main architectural goals

Build a UI system with these clear responsibilities:

1. Themes define visual and interaction styling.
2. Prefabs define visuals and local component structure.
3. UI elements define reusable control behavior.
4. Composite UI components own the context of the object they operate on.
5. Builders provide an ergonomic API for creating UI compositions at runtime.
6. Commands represent consequential actions such as delete or reset.
7. Spatial placement is separate from the business object context.
8. Mixed Reality framework dependencies are isolated behind adapter interfaces.

Do not create one large `UIManager` class.

Do not use `Resources.Load`, string-based prefab paths, `FindObjectOfType`, global selected-object state, or external `transform.Find` hierarchy lookups.

Prefer composition over deep inheritance.

## Assumptions

* Use modern Unity-compatible C#.
* Use TextMeshPro for text.
* Keep the core system independent of MRTK and XR Interaction Toolkit.
* Interaction integrations should be represented by interfaces or small adapter components.
* Use serialized prefab references through ScriptableObject catalogs.
* Do not introduce a third-party dependency injection framework.
* All public APIs should include XML documentation where useful.
* Use nullable reference handling appropriate for the Unity project’s current C# configuration.
* Do not modify unrelated project files.

First inspect the project to determine:

* Unity version
* Existing namespace conventions
* Existing assembly definition layout
* Whether MRTK, XR Interaction Toolkit, or another interaction system is installed
* Existing UI prefabs, themes, or utility classes
* Current testing setup

Adapt namespaces and assembly definitions to the existing project conventions.

## Proposed file structure

Create or adapt the following structure:

```text
Assets/
└── Project/
    └── UI/
        ├── Runtime/
        │   ├── Core/
        │   │   ├── IUIElement.cs
        │   │   ├── UIElement.cs
        │   │   ├── ContextualUIElement.cs
        │   │   ├── UIContext.cs
        │   │   ├── UICreationContext.cs
        │   │   └── UISpatialContext.cs
        │   │
        │   ├── Styling/
        │   │   ├── UITheme.cs
        │   │   ├── UIColorTokens.cs
        │   │   ├── UISpacingTokens.cs
        │   │   ├── UITypographyTokens.cs
        │   │   ├── UIGeometryTokens.cs
        │   │   └── UIInteractionTokens.cs
        │   │
        │   ├── Elements/
        │   │   ├── UILabel.cs
        │   │   ├── UIButton.cs
        │   │   ├── UIToggle.cs
        │   │   └── UISlider.cs
        │   │
        │   ├── Layout/
        │   │   ├── UIStack.cs
        │   │   └── UICard.cs
        │   │
        │   ├── Commands/
        │   │   ├── IUICommand.cs
        │   │   └── UICommand.cs
        │   │
        │   ├── Context/
        │   │   ├── IUITargetLifetime.cs
        │   │   ├── IConfigurableObject.cs
        │   │   └── ObjectConfigurationMenu.cs
        │   │
        │   ├── Factory/
        │   │   ├── UIPrefabCatalog.cs
        │   │   ├── UIFactory.cs
        │   │   └── IUIObjectProvider.cs
        │   │
        │   ├── Builders/
        │   │   ├── UI.cs
        │   │   ├── ContextualUIBuilder.cs
        │   │   └── ContextualPanelBuilder.cs
        │   │
        │   └── Interaction/
        │       ├── IPressSource.cs
        │       ├── IHoverSource.cs
        │       └── UIVisualStateController.cs
        │
        ├── Editor/
        ├── Prefabs/
        │   ├── Elements/
        │   └── Composites/
        ├── Themes/
        ├── Samples/
        └── Tests/
            ├── EditMode/
            └── PlayMode/
```

Adjust the root path if the existing project uses another convention.

Create assembly definition files where appropriate. Runtime code must not reference editor assemblies.

## Core UI contracts

Implement a minimal non-generic base contract:

```csharp
public interface IUIElement
{
    GameObject GameObject { get; }
    Transform Transform { get; }

    void ApplyTheme(UITheme theme);
    void SetVisible(bool visible);
    void SetInteractable(bool interactable);
}
```

Implement an abstract `UIElement : MonoBehaviour, IUIElement`.

It should:

* Store the currently applied theme.
* Expose `GameObject` and `Transform`.
* Apply a theme through a protected hook.
* Support visibility.
* Support interactability through a virtual method.
* Avoid containing control-specific behavior.
* Avoid global service access.

Implement a typed contextual base:

```csharp
public abstract class ContextualUIElement<TTarget> : UIElement
{
    public TTarget Target { get; }

    public bool IsBound { get; }

    public void Bind(UICreationContext<TTarget> context);

    public void Unbind();
}
```

Requirements:

* A UI instance can only have one active binding at a time.
* Rebinding must unsubscribe from the old target before binding the new target.
* `OnBind` and `OnUnbind` protected lifecycle hooks must be provided.
* `OnDestroy` must safely unbind.
* Avoid leaking event subscriptions.
* Handle Unity destroyed-object semantics when relevant.

## Context types

Implement `UIContext` for shared UI services:

```csharp
public sealed class UIContext
{
    public UITheme Theme { get; }
    public UIFactory Factory { get; }
}
```

Extend it only when an existing project service clearly belongs there.

Implement `UISpatialContext` with:

* Anchor transform
* Local or world-space offset
* Whether the UI should face the user
* Optional follow behavior
* Optional placement policy enum

The target object and anchor must not be assumed to be the same object.

Implement:

```csharp
public sealed class UICreationContext<TTarget>
{
    public UIContext UI { get; }
    public TTarget Target { get; }
    public UISpatialContext Spatial { get; }
}
```

Validate constructor arguments and produce useful exceptions for invalid setup.

## Theme system

Create a `UITheme` ScriptableObject composed of serializable token groups.

Include at least:

* Surface colors
* Primary and secondary colors
* Destructive color
* Primary and secondary text colors
* Disabled color
* Hovered and pressed state values
* Default TextMeshPro font
* Body and heading font sizes
* Small, medium, and large spacing
* Standard control dimensions in meters
* Corner radius
* Pressed scale
* Transition duration

Use semantic style variants:

```csharp
public enum UIStyleVariant
{
    Primary,
    Secondary,
    Subtle,
    Destructive,
    Success
}
```

Consumers should request semantic variants rather than directly assigning arbitrary colors.

## Reusable UI elements

Implement initial versions of:

* `UILabel`
* `UIButton`
* `UIToggle`
* `UISlider`
* `UIStack`
* `UICard`

Each element should:

* Apply theme values.
* Expose only necessary public configuration.
* Avoid business-domain dependencies.
* Use explicit serialized child references.
* Never search its internal hierarchy by name at runtime.
* Clean up event subscriptions.
* Support configuration without firing change callbacks where appropriate.

`UIButton` should provide a normal C# event such as:

```csharp
public event Action Pressed;
```

It may also expose a serialized `UnityEvent` for inspector use, but business logic should be bindable through the C# event.

`UIToggle` and `UISlider` should provide:

```csharp
public event Action<TValue> ValueChanged;
```

and:

```csharp
public void SetValueWithoutNotify(TValue value);
```

`UICard` should expose explicit content slots:

```csharp
public Transform HeaderRoot { get; }
public Transform ContentRoot { get; }
public Transform FooterRoot { get; }
```

External callers must never need to inspect the prefab hierarchy.

## Interaction isolation

Create simple interaction-source contracts such as:

```csharp
public interface IPressSource
{
    event Action Pressed;
}

public interface IHoverSource
{
    event Action HoverEntered;
    event Action HoverExited;
}
```

`UIButton` should consume an interaction source rather than directly requiring MRTK or XR Interaction Toolkit classes.

When an MR interaction package is installed, add a small adapter component for it in a clearly isolated namespace or assembly.

Do not spread MRTK or XRI types throughout the UI architecture.

Create a `UIVisualStateController` responsible for visual states:

```csharp
public enum UIVisualState
{
    Normal,
    Hovered,
    Focused,
    Pressed,
    Selected,
    Disabled
}
```

The controller should translate semantic state into themed visual properties.

## Commands

Implement:

```csharp
public interface IUICommand
{
    bool CanExecute { get; }
    void Execute();
}
```

Implement a typed command:

```csharp
public sealed class UICommand<TTarget> : IUICommand
{
    public UICommand(
        TTarget target,
        Action<TTarget> execute,
        Func<TTarget, bool> canExecute = null);
}
```

Requirements:

* `Execute` must do nothing when `CanExecute` is false.
* Validate the execute delegate.
* Support refreshing button interactability.
* Do not put confirmation-dialog behavior inside the base command.
* Leave room for undoable command implementations later.

Add a lightweight way to bind a command to `UIButton`.

## Target capabilities

Create a sample target-facing interface:

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

This is a sample capability interface, not a mandatory base interface for every scene object.

Do not make domain objects depend on UI components.

Create a lifetime contract:

```csharp
public interface IUITargetLifetime
{
    bool IsValid { get; }

    event Action Invalidated;
}
```

Contextual composites should subscribe to `Invalidated` while bound and unsubscribe when unbound.

When the target becomes invalid:

* Disable interaction immediately.
* Unbind safely.
* Close or release the menu.
* Do not retain stale target references.

## Object configuration menu

Implement `ObjectConfigurationMenu : ContextualUIElement<IConfigurableObject>`.

It should contain:

* Title label
* Visibility toggle
* Scale slider
* Delete button
* Optional close button

On binding:

* Set the title from `DisplayName`.
* Initialize the toggle without firing callbacks.
* Initialize the slider without firing callbacks.
* Subscribe to control events.
* Create or bind a delete command.
* Set delete-button interactability from `CanDelete`.
* Subscribe to target invalidation when supported.

On unbinding:

* Unsubscribe from every control event.
* Unsubscribe from target lifetime events.
* Remove command bindings.
* Clear transient state.

After delete is executed, the menu must close or release itself safely even if destruction of the target occurs at the end of the frame.

## Prefab catalog and factory

Create a `UIPrefabCatalog` ScriptableObject containing typed prefab references for the initial elements and composites.

Implement an `IUIObjectProvider` abstraction:

```csharp
public interface IUIObjectProvider
{
    T Get<T>(T prefab, Transform parent) where T : Component;
    void Release<T>(T instance) where T : Component;
}
```

Provide a default implementation using `Instantiate` and `Destroy`.

Design the factory so that the provider can later be replaced with pooling without changing the public creation API.

Implement `UIFactory` that:

* Receives a prefab catalog.
* Receives or resolves a theme through explicit initialization.
* Instantiates typed components.
* Applies the current theme.
* Creates elements under an explicit parent.
* Supports releasing created elements.
* Produces useful errors for missing prefab assignments.
* Does not use static global state.

## Ergonomic contextual builder API

Implement a typed entry point that allows code resembling:

```csharp
var menu = ui
    .For<IConfigurableObject>(selectedObject)
    .AnchoredTo(selectedTransform)
    .WithLocalOffset(new Vector3(0.15f, 0.1f, 0f))
    .FaceUser()
    .ConfigurationPanel("Object Settings")
    .ShowVisibilityControl()
    .ShowScaleControl(0.1f, 3f)
    .ShowDeleteAction()
    .Build(parent);
```

Also design the builder foundations so a future API can support:

```csharp
ui.For(target)
    .Panel("Object Settings")
    .Toggle(
        "Visible",
        target => target.IsVisible,
        (target, value) => target.IsVisible = value)
    .Slider(
        "Scale",
        target => target.Scale,
        (target, value) => target.Scale = value,
        0.1f,
        3f)
    .Action(
        "Delete",
        target => target.Delete(),
        target => target.CanDelete,
        UIStyleVariant.Destructive)
    .Build(parent);
```

For the initial implementation, prioritize the concrete `ObjectConfigurationMenu` builder over a fully generic declarative binding engine.

Builders must:

* Preserve the target’s compile-time type.
* Require a target before constructing contextual UI.
* Store configuration only.
* Avoid containing scene or domain logic.
* Build through `UIFactory`.
* Bind the completed composite once.
* Validate required values before instantiation.
* Remain thin wrappers rather than becoming alternate UI components.

## Public API expectations

The normal call site should not need to:

* Manually assign themes
* Manually subscribe every child control
* Search prefab hierarchies
* Pass the same target to every child element
* Know which MR interaction framework is in use
* Use global selection state
* Manually destroy a menu after target invalidation

The target should be supplied once at the top of the contextual builder chain.

## Error handling

Add clear development-time errors for:

* Null target
* Missing theme
* Missing prefab catalog
* Missing required prefab
* Missing required serialized control reference
* Invalid slider ranges
* Attempting to build twice with a single-use builder, if builders are implemented as single-use
* Binding an invalid target

Use `Debug.Assert`, validation methods, exceptions, or editor validation appropriately. Avoid silently ignoring broken configuration.

Implement `OnValidate` where it helps detect prefab setup problems in the editor.

## Testing

Add EditMode tests for at least:

1. Binding a contextual UI element stores the correct target.
2. Rebinding unbinds the previous target.
3. Unbinding removes event subscriptions.
4. `UICommand<TTarget>` executes against the correct target.
5. A command does not execute when `CanExecute` is false.
6. The delete button uses the currently bound target.
7. Target invalidation causes the configuration menu to unbind.
8. Builder configuration preserves the target type and target instance.
9. Factory-created elements receive the current theme.
10. A missing required prefab produces a useful failure.

Add PlayMode tests only where Unity lifecycle or delayed destruction behavior makes them necessary.

Do not make tests depend on hand tracking hardware.

## Sample usage

Create a small sample MonoBehaviour implementing `IConfigurableObject` and `IUITargetLifetime`.

Create a sample script that opens the configuration menu for that object.

The sample should demonstrate:

```csharp
private void OpenConfiguration()
{
    ui
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
```

Do not create polished production art. Use clearly documented placeholder prefabs or provide prefab-setup instructions where prefab assets cannot be generated reliably through code.

## Implementation approach

Work in small, reviewable stages:

1. Inspect the existing project and report relevant conventions.
2. Create core contracts and context types.
3. Add theme tokens and theme application.
4. Add the basic elements and layout components.
5. Add commands and target lifetime handling.
6. Add the prefab catalog and factory.
7. Add `ObjectConfigurationMenu`.
8. Add the contextual builder API.
9. Add tests.
10. Add sample usage and setup documentation.

After each stage:

* Compile the affected assemblies.
* Resolve compilation errors before proceeding.
* Keep changes limited to the UI architecture.
* Summarize files added or changed.

## Deliverables

Produce:

* All required runtime C# files
* Appropriate assembly definition files
* EditMode tests
* Necessary PlayMode tests
* Sample target and sample menu opener
* A concise README describing:

  * Architecture
  * Folder structure
  * Prefab setup
  * Theme setup
  * How to create contextual UI
  * How to add a new reusable element
  * How to add an interaction-framework adapter
  * How target invalidation is handled

At completion, provide:

1. A summary of the implemented architecture.
2. A list of created and modified files.
3. Any Unity Editor setup steps that remain.
4. Known limitations.
5. The exact sample code required to open a contextual menu.
6. Test results or an explicit explanation of tests that could not be executed.

Favor a small, coherent, extensible implementation over a large speculative framework.
