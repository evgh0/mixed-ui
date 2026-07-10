using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    /// <summary>Applies one theme to all Mixed UI style bindings beneath this transform.</summary>
    [DisallowMultipleComponent]
    public sealed class MixedUIThemeScope : MonoBehaviour
    {
        [SerializeField] private MixedUITheme theme;

        public MixedUITheme Theme
        {
            get => theme;
            set
            {
                theme = value != null ? value : throw new ArgumentNullException(nameof(value));
                ApplyTheme();
            }
        }

        public void ApplyTheme()
        {
            if (theme == null)
                throw new InvalidOperationException($"{nameof(MixedUIThemeScope)} on '{name}' requires a theme.");

            var styles = GetComponentsInChildren<MixedUIStyleBehaviour>(true);
            foreach (var style in styles)
                style.ApplyTheme(theme);
        }

        private void OnEnable()
        {
            if (theme != null) ApplyTheme();
        }

        private void OnValidate()
        {
            if (theme != null) ApplyTheme();
        }
    }
}

