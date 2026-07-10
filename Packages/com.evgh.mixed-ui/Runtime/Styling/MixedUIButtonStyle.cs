using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    [DisallowMultipleComponent]
    public sealed class MixedUIButtonStyle : MixedUIStyleBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text label;
        [SerializeField] private MixedUIVariant variant = MixedUIVariant.Secondary;

        public Button Button => button;
        public TMP_Text Label => label;
        public MixedUIVariant Variant => variant;

        public override void ApplyTheme(MixedUITheme theme)
        {
            Required(theme, this, nameof(theme));
            Required(button, this, nameof(button));
            Required(background, this, nameof(background));
            Required(label, this, nameof(label));

            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = theme.CreateColorBlock(variant);
            background.color = Color.white;
            ApplySprite(background, theme.sprites.roundedControl ?? theme.sprites.roundedSurface);
            ApplyText(label, theme, MixedUITextRole.Body, false);
        }

        public void SetVariant(MixedUIVariant value)
        {
            variant = value;
            var scope = GetComponentInParent<MixedUIThemeScope>();
            if (scope != null && scope.Theme != null) ApplyTheme(scope.Theme);
        }

        private static void ApplySprite(Image image, Sprite sprite)
        {
            if (sprite == null) return;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
        }

        private static void ApplyText(TMP_Text text, MixedUITheme theme, MixedUITextRole role, bool secondary)
        {
            if (theme.typography.defaultFont != null) text.font = theme.typography.defaultFont;
            text.fontSize = theme.FontSizeFor(role);
            text.color = secondary ? theme.colors.textSecondary : theme.colors.textPrimary;
        }

        private void Reset()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            label = GetComponentInChildren<TMP_Text>(true);
        }
    }
}

