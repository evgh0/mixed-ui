using System;
using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    public enum UIStackDirection { Vertical, Horizontal }

    public sealed class UIStack : UIElement
    {
        [SerializeField] private HorizontalOrVerticalLayoutGroup layout;
        [SerializeField] private UIStackDirection direction = UIStackDirection.Vertical;

        public Transform ContentRoot => transform;

        protected override void OnThemeApplied(UITheme theme)
        {
            if (layout == null)
                throw new InvalidOperationException($"{nameof(UIStack)} on '{name}' requires a layout group.");
            layout.spacing = theme.spacing.medium;
        }

        private void OnValidate()
        {
            layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layout == null) return;
            var isVertical = layout is VerticalLayoutGroup;
            if ((direction == UIStackDirection.Vertical) != isVertical)
                Debug.LogWarning($"{name}: Stack direction does not match its layout-group type.", this);
        }
    }
}

