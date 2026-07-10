using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    /// <summary>Creates typed, themed UI from an explicit prefab catalog.</summary>
    public sealed class UIFactory
    {
        private readonly UIPrefabCatalog _catalog;
        private readonly UITheme _theme;
        private readonly IUIObjectProvider _provider;
        private readonly IWorldSpaceCanvasAdapter _canvasAdapter;

        public UIFactory(
            UIPrefabCatalog catalog,
            UITheme theme,
            IUIObjectProvider provider = null,
            IWorldSpaceCanvasAdapter canvasAdapter = null)
        {
            _catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
            _theme = theme != null ? theme : throw new ArgumentNullException(nameof(theme));
            _provider = provider ?? new InstantiateUIObjectProvider();
            _canvasAdapter = canvasAdapter ?? new UGUIWorldSpaceCanvasAdapter();
        }

        public UILabel CreateLabel(Transform parent) => CreateRequired(_catalog.LabelPrefab, parent, "label");
        public UIButton CreateButton(Transform parent) => CreateRequired(_catalog.ButtonPrefab, parent, "button");
        public UIToggle CreateToggle(Transform parent) => CreateRequired(_catalog.TogglePrefab, parent, "toggle");
        public UISlider CreateSlider(Transform parent) => CreateRequired(_catalog.SliderPrefab, parent, "slider");
        public UIStack CreateStack(Transform parent) => CreateRequired(_catalog.StackPrefab, parent, "stack");
        public UICard CreateCard(Transform parent) => CreateRequired(_catalog.CardPrefab, parent, "card");

        public UIInstanceHandle<ObjectConfigurationMenu> CreateConfigurationMenu(Transform parent, Transform viewer)
        {
            var menu = CreateRequired(_catalog.ConfigurationMenuPrefab, parent, "configuration menu");
            var canvas = menu.GetComponent<Canvas>() ?? menu.GetComponentInChildren<Canvas>(true);
            if (canvas == null)
            {
                Release(menu);
                throw new InvalidOperationException("The configuration-menu prefab requires a Canvas.");
            }
            _canvasAdapter.Configure(canvas, viewer);
            return new UIInstanceHandle<ObjectConfigurationMenu>(this, menu);
        }

        internal void Release<T>(T instance) where T : UIElement => _provider.Release(instance);

        private T CreateRequired<T>(T prefab, Transform parent, string description) where T : UIElement
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (prefab == null)
                throw new InvalidOperationException($"UIPrefabCatalog is missing its required {description} prefab.");
            var instance = _provider.Get(prefab, parent);
            instance.ApplyTheme(_theme);
            return instance;
        }
    }
}

