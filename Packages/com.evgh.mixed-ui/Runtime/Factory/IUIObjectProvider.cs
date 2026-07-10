using UnityEngine;

namespace Evgh.MixedUI
{
    public interface IUIObjectProvider
    {
        T Get<T>(T prefab, Transform parent) where T : Component;
        void Release<T>(T instance) where T : Component;
    }

    public sealed class InstantiateUIObjectProvider : IUIObjectProvider
    {
        public T Get<T>(T prefab, Transform parent) where T : Component =>
            Object.Instantiate(prefab, parent, false);

        public void Release<T>(T instance) where T : Component
        {
            if (instance == null) return;
            if (Application.isPlaying) Object.Destroy(instance.gameObject);
            else Object.DestroyImmediate(instance.gameObject);
        }
    }
}

