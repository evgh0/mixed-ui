using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    public sealed class UISlider : UIElement
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Graphic fill;

        public event Action<float> ValueChanged;
        public float Value => slider != null ? slider.value : 0f;

        public string Text
        {
            get => label != null ? label.text : string.Empty;
            set { EnsureConfigured(); label.text = value ?? string.Empty; }
        }

        public void SetRange(float minimum, float maximum)
        {
            EnsureConfigured();
            if (minimum >= maximum) throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum must exceed minimum.");
            slider.minValue = minimum;
            slider.maxValue = maximum;
        }

        public void SetValueWithoutNotify(float value)
        {
            EnsureConfigured();
            slider.SetValueWithoutNotify(value);
        }

        public override void SetInteractable(bool interactable)
        {
            EnsureConfigured();
            slider.interactable = interactable;
        }

        protected override void OnThemeApplied(UITheme theme)
        {
            EnsureConfigured();
            if (theme.typography.defaultFont != null) label.font = theme.typography.defaultFont;
            label.fontSize = theme.typography.bodySize;
            label.color = theme.colors.primaryText;
            if (fill != null) fill.color = theme.colors.primary;
        }

        private void OnEnable() { if (slider != null) slider.onValueChanged.AddListener(HandleValueChanged); }
        private void OnDisable() { if (slider != null) slider.onValueChanged.RemoveListener(HandleValueChanged); }
        private void HandleValueChanged(float value) => ValueChanged?.Invoke(value);

        private void EnsureConfigured()
        {
            if (slider == null || label == null)
                throw new InvalidOperationException($"{nameof(UISlider)} on '{name}' requires Slider and TMP_Text references.");
        }

        private void Reset()
        {
            slider = GetComponent<Slider>();
            label = GetComponentInChildren<TMP_Text>(true);
            fill = slider != null ? slider.fillRect?.GetComponent<Graphic>() : null;
        }
    }
}

