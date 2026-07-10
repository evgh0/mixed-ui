using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    /// <summary>Base implementation for themed uGUI components.</summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIElement : MonoBehaviour, IUIElement
    {
        private UITheme _theme;

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public RectTransform RectTransform => (RectTransform)transform;
        public UITheme Theme => _theme;

        public void ApplyTheme(UITheme theme)
        {
            _theme = theme != null ? theme : throw new ArgumentNullException(nameof(theme));
            OnThemeApplied(theme);
        }

        public virtual void SetVisible(bool visible) => gameObject.SetActive(visible);
        public virtual void SetInteractable(bool interactable) { }
        protected abstract void OnThemeApplied(UITheme theme);
    }
}

