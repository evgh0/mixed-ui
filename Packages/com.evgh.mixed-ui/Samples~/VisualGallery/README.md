# Mixed UI Visual Gallery

The sample scene shows the complete neutral-dark visual set on an XRI world-space Canvas.

Before opening the scene:

1. Import TMP Essential Resources from `Window > TextMeshPro > Import TMP Essential Resources`.
2. Import this sample from the Mixed UI package's **Samples** tab.
3. Open `Visual Gallery.unity`.
4. Add or merge the gallery into a scene containing your XR Origin and ray/poke interactors.

The sample owns only the `EventSystem`, `XRUIInputModule`, `CanvasOptimizer`, viewer camera, and styled Canvas. Your application supplies device-specific interactors and input actions.

Every interactive prefab uses standard `Button.onClick`, `Toggle.onValueChanged`, or `Slider.onValueChanged` events. No Mixed UI behavior framework is involved.
