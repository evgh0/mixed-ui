# Object Configuration Sample

1. Import **TMP Essential Resources** from `Window > TextMeshPro > Import TMP Essential Resources`.
2. Add an `EventSystem` with exactly one `XRUIInputModule`.
3. Create a camera/XR rig, an empty UI root, and a primitive with `SampleConfigurableObject`.
4. Add `SampleMenuOpener` and assign the package's `DefaultUITheme` and `DefaultUIPrefabCatalog`, plus the camera, EventSystem, UI root, and target.
5. Invoke `OpenConfiguration` from a scene interaction or its component context menu.

XRI tracked-ray interaction requires a ray interactor with UI interaction enabled. Near poke is intentionally outside the initial package scope.

