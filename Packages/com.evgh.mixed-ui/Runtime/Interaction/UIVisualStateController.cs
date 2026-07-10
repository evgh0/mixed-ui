using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    public enum UIVisualState { Normal, Hovered, Focused, Pressed, Selected, Disabled }

    /// <summary>Maps EventSystem state to semantic theme colors and scale.</summary>
    public sealed class UIVisualStateController : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private RectTransform scaleTarget;
        [SerializeField] private UIStyleVariant variant = UIStyleVariant.Secondary;

        private UITheme _theme;
        private bool _interactable = true;
        private bool _hovered;
        private bool _focused;
        private bool _pressed;

        public UIVisualState State { get; private set; } = UIVisualState.Normal;
        public UIStyleVariant Variant => variant;

        public void Configure(UITheme theme, UIStyleVariant styleVariant)
        {
            _theme = theme;
            variant = styleVariant;
            Refresh();
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            if (!interactable) _pressed = false;
            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData) { _hovered = true; Refresh(); }
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; _pressed = false; Refresh(); }
        public void OnPointerDown(PointerEventData eventData) { if (_interactable) _pressed = true; Refresh(); }
        public void OnPointerUp(PointerEventData eventData) { _pressed = false; Refresh(); }
        public void OnSelect(BaseEventData eventData) { _focused = true; Refresh(); }
        public void OnDeselect(BaseEventData eventData) { _focused = false; _pressed = false; Refresh(); }

        private void Refresh()
        {
            if (_theme == null) return;
            State = !_interactable ? UIVisualState.Disabled
                : _pressed ? UIVisualState.Pressed
                : _hovered ? UIVisualState.Hovered
                : _focused ? UIVisualState.Focused
                : UIVisualState.Normal;

            if (targetGraphic != null)
            {
                var baseColor = _theme.colors.ForVariant(variant);
                targetGraphic.color = State switch
                {
                    UIVisualState.Disabled => _theme.colors.disabled,
                    UIVisualState.Hovered => Color.Lerp(baseColor, _theme.colors.hovered, _theme.colors.hovered.a),
                    UIVisualState.Pressed => Color.Lerp(baseColor, _theme.colors.pressed, _theme.colors.pressed.a),
                    UIVisualState.Focused => Color.Lerp(baseColor, Color.white, 0.1f),
                    _ => baseColor
                };
            }

            if (scaleTarget != null)
            {
                var scale = State == UIVisualState.Pressed ? _theme.interaction.pressedScale : 1f;
                scaleTarget.localScale = Vector3.one * scale;
            }
        }

        private void Reset()
        {
            targetGraphic = GetComponent<Graphic>();
            scaleTarget = transform as RectTransform;
        }
    }
}

