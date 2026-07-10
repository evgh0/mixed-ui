using System;
using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    public sealed class UICard : UIElement
    {
        [SerializeField] private Image surface;
        [SerializeField] private RectTransform headerRoot;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectTransform footerRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        public Transform HeaderRoot => headerRoot;
        public Transform ContentRoot => contentRoot;
        public Transform FooterRoot => footerRoot;

        public override void SetInteractable(bool interactable)
        {
            EnsureConfigured();
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }

        protected override void OnThemeApplied(UITheme theme)
        {
            EnsureConfigured();
            surface.color = theme.colors.surface;
            if (theme.geometry.roundedSurfaceSprite != null)
            {
                surface.sprite = theme.geometry.roundedSurfaceSprite;
                surface.type = Image.Type.Sliced;
            }
        }

        private void EnsureConfigured()
        {
            if (surface == null || headerRoot == null || contentRoot == null || footerRoot == null || canvasGroup == null)
                throw new InvalidOperationException($"{nameof(UICard)} on '{name}' has missing serialized references.");
        }
    }
}
