# Mixed Ui

`com.evgh.mixed-ui` is a Unity 6 package for reusable, themed, context-aware mixed-reality UI built on the built-in world-space Canvas system.

## Architecture

- `UITheme` owns semantic design tokens; prefabs own hierarchy and local visuals.
- `UIElement` components wrap standard uGUI/TMP controls.
- `ContextualUIElement<TTarget>` owns one target binding and its subscription lifetime.
- `UIFactory` creates themed instances from a serialized `UIPrefabCatalog` through a replaceable object provider.
- `UIComposer` and its typed, single-use builders capture configuration and bind a completed composite once.
- `UIInstanceHandle<T>` owns release, including close, delete, and target invalidation paths.
- `UISpatialContext` keeps anchor/viewer placement separate from the business target.
- `XRIWorldSpaceCanvasAdapter` is the only runtime assembly that references XR Interaction Toolkit.

No runtime code uses `Resources.Load`, hierarchy-name lookup, scene-wide object search, global selection, or string prefab paths.

## Installation and setup

The repository includes a minimal Unity 6 host project and this embedded package. For another project, copy `Packages/com.evgh.mixed-ui` into its `Packages` directory or reference the package from Git.

1. Use Unity 6000.0 or newer and install XRI 3.0.9 or a compatible later 3.x release.
2. Import **TMP Essential Resources** once in the consuming project. The package deliberately does not install a competing global `TMP Settings` resource.
3. Assign `Runtime/Assets/Themes/DefaultUITheme.asset` and `Runtime/Assets/DefaultUIPrefabCatalog.asset`, or create project-specific variants.
4. For XRI, provide a consumer-owned `EventSystem` containing exactly one `XRUIInputModule`. The adapter validates rather than silently rewriting scene input.
5. Ensure the viewer transform has a `Camera`; XRI Canvas instances receive it as their event camera.

Placeholder assets can be regenerated from `Tools > Mixed-UI > Regenerate Default Assets`.

## Opening contextual UI

```csharp
var adapter = new XRIWorldSpaceCanvasAdapter(eventSystem);
var factory = new UIFactory(prefabCatalog, theme, canvasAdapter: adapter);
var ui = new UIComposer(new UIContext(theme, factory, viewer.transform));

var handle = ui
    .For<IConfigurableObject>(target)
    .AnchoredTo(target.transform)
    .WithLocalOffset(new Vector3(0.15f, 0.1f, 0f))
    .FaceUser()
    .ConfigurationPanel("Object Settings")
    .ShowVisibilityControl()
    .ShowScaleControl(0.1f, 3f)
    .ShowDeleteAction()
    .Build(uiRoot);
```

Keep the returned handle only when the caller also needs to close the instance. Target invalidation and the menu's close/delete actions release it automatically.

## Prefabs and themes

All control references are serialized explicitly. `UICard` exposes header, content, and footer slots, so callers never inspect prefab hierarchy. Geometry and typography use logical Canvas units; the supplied composite uses 1,000 units per meter and a root scale of `0.001`.

Rounded surfaces use a nine-sliced sprite because standard uGUI has no numeric corner-radius property. Semantic variants select theme colors instead of accepting arbitrary per-call colors.

## Extending the package

To add an element:

1. Derive a focused component from `UIElement`.
2. Serialize the required uGUI/TMP references and validate them.
3. Apply tokens in `OnThemeApplied`, with no domain behavior.
4. Add a typed catalog entry and factory creation method.
5. Add tests for theme application, events, and no-notify behavior where relevant.

To add another interaction framework, implement `IWorldSpaceCanvasAdapter` in a separate assembly. Configure its raycaster/input boundary there; do not reference framework types from controls, builders, commands, or contexts.

## Target invalidation

Targets may implement `IUITargetLifetime`. A contextual composite subscribes while bound, disables interaction immediately on invalidation, unbinds all callbacks, and requests its handle to release the instance. Destroyed `UnityEngine.Object` targets are also detected through Unity's special null semantics.

## Current limitations

- Initial MR input support covers XRI tracked rays, not direct near poke.
- Controls initialize from the target and write user changes back; unrelated external target changes are not observed live.
- The provider boundary supports future pooling, but the supplied provider uses `Instantiate` and `Destroy`.
- Generic declarative property binding, undoable commands, confirmation dialogs, and MRTK adapters are not included in v0.1.
