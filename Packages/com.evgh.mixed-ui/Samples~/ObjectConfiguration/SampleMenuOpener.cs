using Evgh.MixedUI.XRI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Evgh.MixedUI.Sample
{
    /// <summary>Explicitly composes package services and opens the sample menu.</summary>
    public sealed class SampleMenuOpener : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private UIPrefabCatalog prefabCatalog;
        [SerializeField] private Camera viewer;
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private Transform uiRoot;
        [SerializeField] private SampleConfigurableObject target;

        private UIComposer _ui;
        private UIInstanceHandle<ObjectConfigurationMenu> _openMenu;

        private void Awake()
        {
            var canvasAdapter = new XRIWorldSpaceCanvasAdapter(eventSystem);
            var factory = new UIFactory(prefabCatalog, theme, canvasAdapter: canvasAdapter);
            _ui = new UIComposer(new UIContext(theme, factory, viewer.transform));
        }

        [ContextMenu("Open Configuration")]
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

        private void OnDestroy() => _openMenu?.Close();
    }
}

