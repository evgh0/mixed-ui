# Mixed UI

Mixed UI `0.2.0` is a themed visual component kit for XRI world-space uGUI.

## Setup

1. Use Unity 6000.0 or newer with XR Interaction Toolkit 3.0.9.
2. Import TMP Essential Resources.
3. Create an XRI UI EventSystem with `GameObject > XR > UI EventSystem`.
4. Drag `Runtime/Assets/Prefabs/Canvases/World Space Canvas.prefab` into the scene.
5. Assign the XR head camera as its event camera.
6. Add control and layout prefabs beneath the Canvas.

Controls use the standard uGUI APIs: `Button.onClick`, `Toggle.onValueChanged`, `Slider.onValueChanged`, and `Selectable.interactable`.

## Styling

Assign a `MixedUITheme` to `MixedUIThemeScope`. Call `ApplyTheme()` or set the `Theme` property to refresh all child style bindings.

The package includes semantic button variants, TMP text roles, toggle and slider styles, surfaces, cards, stacks, a modal, a scroll panel, and XRI world/head-follow Canvas prefabs.

## Sample

Import **Visual Gallery** from the package's Samples tab. The scene includes all visual variants plus `XRUIInputModule` and `CanvasOptimizer`; the consuming project supplies its XR Origin and interactors.

## Migration

Version 0.2 removes the 0.1 framework APIs. Use standard uGUI components with `MixedUI*Style` bindings, direct prefab references, application-owned view/controller logic, and XRI `LazyFollow`.

See the [repository README](https://github.com/evgh0/mixed-ui#readme) for installation, complete usage, prefab reference, theme API, migration details, testing, and troubleshooting.
