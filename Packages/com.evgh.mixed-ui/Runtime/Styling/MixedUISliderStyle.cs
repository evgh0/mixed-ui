using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    [DisallowMultipleComponent]
    public sealed class MixedUISliderStyle : MixedUIStyleBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Image track;
        [SerializeField] private Image fill;
        [SerializeField] private Image handle;
        [SerializeField] private TMP_Text label;

        public Slider Slider => slider;

        public override void ApplyTheme(MixedUITheme theme)
        {
            Required(theme, this, nameof(theme));
            Required(slider, this, nameof(slider));
            Required(track, this, nameof(track));
            Required(fill, this, nameof(fill));
            Required(handle, this, nameof(handle));
            Required(label, this, nameof(label));

            slider.targetGraphic = handle;
            slider.transition = Selectable.Transition.ColorTint;
            slider.colors = theme.CreateColorBlock(MixedUIVariant.Primary);
            track.color = theme.colors.subtle;
            fill.color = theme.colors.primary;
            handle.color = Color.white;
            if (theme.sprites.roundedControl != null)
            {
                track.sprite = theme.sprites.roundedControl;
                track.type = Image.Type.Sliced;
                fill.sprite = theme.sprites.roundedControl;
                fill.type = Image.Type.Sliced;
                handle.sprite = theme.sprites.roundedControl;
                handle.type = Image.Type.Sliced;
            }
            if (theme.typography.defaultFont != null) label.font = theme.typography.defaultFont;
            label.fontSize = theme.FontSizeFor(MixedUITextRole.Body);
            label.color = theme.colors.textPrimary;
        }

        private void Reset()
        {
            slider = GetComponent<Slider>();
            handle = slider != null ? slider.targetGraphic as Image : null;
            fill = slider != null ? slider.fillRect?.GetComponent<Image>() : null;
            label = GetComponentInChildren<TMP_Text>(true);
        }
    }
}

