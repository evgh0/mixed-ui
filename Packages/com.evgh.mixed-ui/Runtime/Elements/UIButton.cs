using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    public sealed class UIButton : UIElement
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;
        [SerializeField] private UIVisualStateController visualState;
        [SerializeField] private UIStyleVariant variant = UIStyleVariant.Secondary;
        [SerializeField] private UnityEvent pressed;

        private IUICommand _command;
        public event Action Pressed;

        public string Text
        {
            get => label != null ? label.text : string.Empty;
            set { EnsureConfigured(); label.text = value ?? string.Empty; }
        }

        public void SetStyleVariant(UIStyleVariant value)
        {
            variant = value;
            if (Theme != null) OnThemeApplied(Theme);
        }

        public void BindCommand(IUICommand command)
        {
            UnbindCommand();
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _command.CanExecuteChanged += RefreshCommandState;
            RefreshCommandState();
        }

        public void UnbindCommand()
        {
            if (_command != null) _command.CanExecuteChanged -= RefreshCommandState;
            _command = null;
        }

        public void RefreshCommandState() => SetInteractable(_command == null || _command.CanExecute);

        /// <summary>Invokes the same action as a uGUI click when the control is interactable.</summary>
        public void Press()
        {
            EnsureConfigured();
            if (button.interactable) HandlePressed();
        }

        public override void SetInteractable(bool interactable)
        {
            EnsureConfigured();
            button.interactable = interactable;
            visualState?.SetInteractable(interactable);
        }

        protected override void OnThemeApplied(UITheme theme)
        {
            EnsureConfigured();
            button.transition = Selectable.Transition.None;
            if (theme.typography.defaultFont != null) label.font = theme.typography.defaultFont;
            label.fontSize = theme.typography.bodySize;
            label.color = theme.colors.primaryText;
            visualState?.Configure(theme, variant);
        }

        private void OnEnable()
        {
            if (button != null) button.onClick.AddListener(HandlePressed);
        }

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(HandlePressed);
        }

        private void OnDestroy()
        {
            UnbindCommand();
            if (button != null) button.onClick.RemoveListener(HandlePressed);
        }

        private void HandlePressed()
        {
            if (_command != null) _command.Execute();
            Pressed?.Invoke();
            pressed?.Invoke();
        }

        private void EnsureConfigured()
        {
            if (button == null || label == null)
                throw new InvalidOperationException($"{nameof(UIButton)} on '{name}' requires Button and TMP_Text references.");
        }

        private void Reset()
        {
            button = GetComponent<Button>();
            label = GetComponentInChildren<TMP_Text>(true);
            visualState = GetComponent<UIVisualStateController>();
        }
    }
}
