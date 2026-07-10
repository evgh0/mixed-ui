using TMPro;
using UnityEngine;

namespace Evgh.MixedUI
{
    [DisallowMultipleComponent]
    public sealed class MixedUITextStyle : MixedUIStyleBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private MixedUITextRole role = MixedUITextRole.Body;
        [SerializeField] private bool secondaryColor;

        public TMP_Text Text => text;
        public MixedUITextRole Role => role;

        public override void ApplyTheme(MixedUITheme theme)
        {
            Required(theme, this, nameof(theme));
            Required(text, this, nameof(text));
            if (theme.typography.defaultFont != null) text.font = theme.typography.defaultFont;
            text.fontSize = theme.FontSizeFor(role);
            text.color = secondaryColor ? theme.colors.textSecondary : theme.colors.textPrimary;
        }

        private void Reset() => text = GetComponent<TMP_Text>();
    }
}

