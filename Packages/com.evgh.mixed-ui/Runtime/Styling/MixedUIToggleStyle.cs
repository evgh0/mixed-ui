using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    [DisallowMultipleComponent]
    public sealed class MixedUIToggleStyle : MixedUIStyleBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image background;
        [SerializeField] private Graphic checkmark;
        [SerializeField] private TMP_Text label;

        public Toggle Toggle => toggle;

        public override void ApplyTheme(MixedUITheme theme)
        {
            Required(theme, this, nameof(theme));
            Required(toggle, this, nameof(toggle));
            Required(background, this, nameof(background));
            Required(checkmark, this, nameof(checkmark));
            Required(label, this, nameof(label));

            toggle.targetGraphic = background;
            toggle.graphic = checkmark;
            toggle.transition = Selectable.Transition.ColorTint;
            toggle.colors = theme.CreateColorBlock(MixedUIVariant.Subtle);
            background.color = Color.white;
            checkmark.color = theme.colors.primary;
            if (theme.sprites.roundedControl != null)
            {
                background.sprite = theme.sprites.roundedControl;
                background.type = Image.Type.Sliced;
            }
            if (theme.typography.defaultFont != null) label.font = theme.typography.defaultFont;
            label.fontSize = theme.FontSizeFor(MixedUITextRole.Body);
            label.color = theme.colors.textPrimary;
        }

        private void Reset()
        {
            toggle = GetComponent<Toggle>();
            background = toggle != null ? toggle.targetGraphic as Image : null;
            checkmark = toggle != null ? toggle.graphic : null;
            label = GetComponentInChildren<TMP_Text>(true);
        }
    }
}

