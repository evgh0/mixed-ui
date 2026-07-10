using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    public sealed class UIToggle : UIElement
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Graphic background;

        public event Action<bool> ValueChanged;
        public bool Value => toggle != null && toggle.isOn;

        public string Text
        {
            get => label != null ? label.text : string.Empty;
            set { EnsureConfigured(); label.text = value ?? string.Empty; }
        }

        public void SetValueWithoutNotify(bool value)
        {
            EnsureConfigured();
            toggle.SetIsOnWithoutNotify(value);
        }

        public override void SetInteractable(bool interactable)
        {
            EnsureConfigured();
            toggle.interactable = interactable;
        }

        protected override void OnThemeApplied(UITheme theme)
        {
            EnsureConfigured();
            toggle.transition = Selectable.Transition.ColorTint;
            if (theme.typography.defaultFont != null) label.font = theme.typography.defaultFont;
            label.fontSize = theme.typography.bodySize;
            label.color = theme.colors.primaryText;
            if (background != null) background.color = theme.colors.secondary;
            if (toggle.graphic != null) toggle.graphic.color = theme.colors.primary;
        }

        private void OnEnable() { if (toggle != null) toggle.onValueChanged.AddListener(HandleValueChanged); }
        private void OnDisable() { if (toggle != null) toggle.onValueChanged.RemoveListener(HandleValueChanged); }
        private void HandleValueChanged(bool value) => ValueChanged?.Invoke(value);

        private void EnsureConfigured()
        {
            if (toggle == null || label == null)
                throw new InvalidOperationException($"{nameof(UIToggle)} on '{name}' requires Toggle and TMP_Text references.");
        }

        private void Reset()
        {
            toggle = GetComponent<Toggle>();
            label = GetComponentInChildren<TMP_Text>(true);
            background = toggle != null ? toggle.targetGraphic : null;
        }
    }
}

