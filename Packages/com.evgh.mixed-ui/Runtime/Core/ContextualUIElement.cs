using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    /// <summary>Base for UI instances bound to one target at a time.</summary>
    public abstract class ContextualUIElement<TTarget> : UIElement where TTarget : class
    {
        private TTarget _target;

        public bool IsBound => _target != null && !IsUnityNull(_target);

        public TTarget Target => IsBound
            ? _target
            : throw new InvalidOperationException($"{GetType().Name} is not bound to a valid target.");

        public void Bind(UICreationContext<TTarget> context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (!IsTargetValid(context.Target))
                throw new ArgumentException("The target is null, destroyed, or invalid.", nameof(context));

            Unbind();
            _target = context.Target;
            try
            {
                OnBind(context);
            }
            catch
            {
                Unbind();
                throw;
            }
        }

        public void Unbind()
        {
            if (_target == null) return;
            try { OnUnbind(); }
            finally { _target = null; }
        }

        protected virtual bool IsTargetValid(TTarget target)
        {
            if (target == null || IsUnityNull(target)) return false;
            return !(target is IUITargetLifetime lifetime) || lifetime.IsValid;
        }

        protected abstract void OnBind(UICreationContext<TTarget> context);
        protected abstract void OnUnbind();

        protected virtual void LateUpdate()
        {
            if (_target != null && !IsTargetValid(_target)) OnTargetBecameInvalid();
        }

        protected virtual void OnTargetBecameInvalid()
        {
            SetInteractable(false);
            Unbind();
        }

        protected virtual void OnDestroy() => Unbind();

        private static bool IsUnityNull(TTarget target) =>
            target is UnityEngine.Object unityObject && unityObject == null;
    }
}

