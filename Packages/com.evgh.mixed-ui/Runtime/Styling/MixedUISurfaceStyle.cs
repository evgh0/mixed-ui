using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    [DisallowMultipleComponent]
    public sealed class MixedUISurfaceStyle : MixedUIStyleBehaviour
    {
        [SerializeField] private Image surface;
        [SerializeField] private bool elevated;

        public Image Surface => surface;

        public override void ApplyTheme(MixedUITheme theme)
        {
            Required(theme, this, nameof(theme));
            Required(surface, this, nameof(surface));
            surface.color = elevated ? theme.colors.elevatedSurface : theme.colors.surface;
            if (theme.sprites.roundedSurface != null)
            {
                surface.sprite = theme.sprites.roundedSurface;
                surface.type = Image.Type.Sliced;
            }
        }

        private void Reset() => surface = GetComponent<Image>();
    }
}

