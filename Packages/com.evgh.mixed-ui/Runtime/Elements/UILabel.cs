using System;
using TMPro;
using UnityEngine;

namespace Evgh.MixedUI
{
    public sealed class UILabel : UIElement
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private UILabelStyle style = UILabelStyle.Body;
        [SerializeField] private bool secondaryColor;

        public string Text
        {
            get => textComponent != null ? textComponent.text : string.Empty;
            set
            {
                EnsureConfigured();
                textComponent.text = value ?? string.Empty;
            }
        }

        public void SetStyle(UILabelStyle value)
        {
            style = value;
            if (Theme != null) OnThemeApplied(Theme);
        }

        protected override void OnThemeApplied(UITheme theme)
        {
            EnsureConfigured();
            if (theme.typography.defaultFont != null) textComponent.font = theme.typography.defaultFont;
            textComponent.fontSize = style == UILabelStyle.Heading
                ? theme.typography.headingSize
                : theme.typography.bodySize;
            textComponent.color = secondaryColor ? theme.colors.secondaryText : theme.colors.primaryText;
        }

        private void EnsureConfigured()
        {
            if (textComponent == null)
                throw new InvalidOperationException($"{nameof(UILabel)} on '{name}' requires a TMP_Text reference.");
        }

        private void Reset() => textComponent = GetComponent<TMP_Text>();
        private void OnValidate() { if (textComponent == null) textComponent = GetComponent<TMP_Text>(); }
    }
}

