using UnityEngine;

namespace Evgh.MixedUI
{
    /// <summary>Common behavior exposed by every reusable UI element.</summary>
    public interface IUIElement
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
        RectTransform RectTransform { get; }
        void ApplyTheme(UITheme theme);
        void SetVisible(bool visible);
        void SetInteractable(bool interactable);
    }
}

