using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    /// <summary>Contextual composite for the sample configurable-object capability.</summary>
    public sealed class ObjectConfigurationMenu : ContextualUIElement<IConfigurableObject>
    {
        [SerializeField] private UICard card;
        [SerializeField] private UILabel titleLabel;
        [SerializeField] private UIToggle visibilityToggle;
        [SerializeField] private UISlider scaleSlider;
        [SerializeField] private UIButton deleteButton;
        [SerializeField] private UIButton closeButton;

        private IUITargetLifetime _lifetime;
        private UICommand<IConfigurableObject> _deleteCommand;
        private bool _showVisibility = true;
        private bool _showScale = true;
        private bool _showDelete = true;
        private float _minimumScale = 0.1f;
        private float _maximumScale = 3f;
        private string _title = "Object Settings";

        public event Action CloseRequested;

        public void Configure(
            string title,
            bool showVisibility,
            bool showScale,
            float minimumScale,
            float maximumScale,
            bool showDelete)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("A panel title is required.", nameof(title));
            if (minimumScale >= maximumScale) throw new ArgumentOutOfRangeException(nameof(maximumScale));
            _title = title;
            _showVisibility = showVisibility;
            _showScale = showScale;
            _showDelete = showDelete;
            _minimumScale = minimumScale;
            _maximumScale = maximumScale;
        }

        public override void SetInteractable(bool interactable) => card?.SetInteractable(interactable);

        protected override void OnThemeApplied(UITheme theme)
        {
            EnsureConfigured();
            card.ApplyTheme(theme);
            titleLabel.ApplyTheme(theme);
            visibilityToggle.ApplyTheme(theme);
            scaleSlider.ApplyTheme(theme);
            deleteButton.ApplyTheme(theme);
            closeButton?.ApplyTheme(theme);
        }

        protected override void OnBind(UICreationContext<IConfigurableObject> context)
        {
            EnsureConfigured();
            titleLabel.Text = string.IsNullOrWhiteSpace(_title)
                ? Target.DisplayName
                : $"{_title}: {Target.DisplayName}";

            visibilityToggle.SetVisible(_showVisibility);
            visibilityToggle.Text = "Visible";
            visibilityToggle.SetValueWithoutNotify(Target.IsVisible);

            scaleSlider.SetVisible(_showScale);
            scaleSlider.Text = "Scale";
            scaleSlider.SetRange(_minimumScale, _maximumScale);
            scaleSlider.SetValueWithoutNotify(Target.Scale);

            deleteButton.SetVisible(_showDelete);
            deleteButton.Text = "Delete";
            deleteButton.SetStyleVariant(UIStyleVariant.Destructive);

            visibilityToggle.ValueChanged += HandleVisibilityChanged;
            scaleSlider.ValueChanged += HandleScaleChanged;
            deleteButton.Pressed += HandleDeletePressed;
            if (closeButton != null) closeButton.Pressed += HandleClosePressed;

            _deleteCommand = new UICommand<IConfigurableObject>(Target, value => value.Delete(), value => value.CanDelete);
            deleteButton.BindCommand(_deleteCommand);

            _lifetime = Target as IUITargetLifetime;
            if (_lifetime != null) _lifetime.Invalidated += HandleTargetInvalidated;
            SetInteractable(true);
        }

        protected override void OnUnbind()
        {
            visibilityToggle.ValueChanged -= HandleVisibilityChanged;
            scaleSlider.ValueChanged -= HandleScaleChanged;
            deleteButton.Pressed -= HandleDeletePressed;
            if (closeButton != null) closeButton.Pressed -= HandleClosePressed;
            if (_lifetime != null) _lifetime.Invalidated -= HandleTargetInvalidated;
            deleteButton.UnbindCommand();
            _deleteCommand = null;
            _lifetime = null;
        }

        protected override void OnTargetBecameInvalid()
        {
            base.OnTargetBecameInvalid();
            CloseRequested?.Invoke();
        }

        private void HandleVisibilityChanged(bool value) { if (IsBound) Target.IsVisible = value; }
        private void HandleScaleChanged(float value) { if (IsBound) Target.Scale = value; }
        private void HandleDeletePressed() => CloseRequested?.Invoke();
        private void HandleClosePressed() => CloseRequested?.Invoke();
        private void HandleTargetInvalidated() => OnTargetBecameInvalid();

        private void EnsureConfigured()
        {
            if (card == null || titleLabel == null || visibilityToggle == null || scaleSlider == null || deleteButton == null)
                throw new InvalidOperationException($"{nameof(ObjectConfigurationMenu)} on '{name}' has missing serialized control references.");
        }
    }
}
