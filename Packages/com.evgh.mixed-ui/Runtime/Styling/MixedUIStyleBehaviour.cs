using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    /// <summary>Visual-only base for components that apply a Mixed UI theme to built-in UI components.</summary>
    public abstract class MixedUIStyleBehaviour : MonoBehaviour
    {
        public abstract void ApplyTheme(MixedUITheme theme);

        protected virtual void OnEnable()
        {
            var scope = GetComponentInParent<MixedUIThemeScope>();
            if (scope != null && scope.Theme != null)
                ApplyTheme(scope.Theme);
        }

        protected static T Required<T>(T value, Component owner, string fieldName) where T : UnityEngine.Object =>
            value != null
                ? value
                : throw new InvalidOperationException($"{owner.GetType().Name} on '{owner.name}' requires '{fieldName}'.");
    }
}

