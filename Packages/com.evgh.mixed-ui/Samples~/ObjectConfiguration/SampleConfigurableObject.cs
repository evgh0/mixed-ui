using System;
using UnityEngine;

namespace Evgh.MixedUI.Sample
{
    /// <summary>Minimal scene object demonstrating contextual target capabilities.</summary>
    public sealed class SampleConfigurableObject : MonoBehaviour, IConfigurableObject, IUITargetLifetime
    {
        [SerializeField] private string displayName = "Sample Object";
        [SerializeField] private Renderer[] controlledRenderers;
        [SerializeField] private bool canDelete = true;

        private bool _isValid = true;

        public string DisplayName => displayName;
        public bool CanDelete => canDelete && _isValid;
        public bool IsValid => _isValid && this != null;
        public event Action Invalidated;

        public bool IsVisible
        {
            get => controlledRenderers == null || controlledRenderers.Length == 0 || controlledRenderers[0].enabled;
            set
            {
                if (controlledRenderers == null) return;
                foreach (var controlledRenderer in controlledRenderers)
                    if (controlledRenderer != null) controlledRenderer.enabled = value;
            }
        }

        public float Scale
        {
            get => transform.localScale.x;
            set => transform.localScale = Vector3.one * Mathf.Max(0.001f, value);
        }

        public void Delete()
        {
            if (!CanDelete) return;
            Invalidate();
            Destroy(gameObject);
        }

        private void OnDestroy() => Invalidate();

        private void Invalidate()
        {
            if (!_isValid) return;
            _isValid = false;
            Invalidated?.Invoke();
        }
    }
}

