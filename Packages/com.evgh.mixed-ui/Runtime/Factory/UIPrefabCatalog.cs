using UnityEngine;

namespace Evgh.MixedUI
{
    [CreateAssetMenu(fileName = "UIPrefabCatalog", menuName = "Mixed-UI/Prefab Catalog")]
    public sealed class UIPrefabCatalog : ScriptableObject
    {
        [SerializeField] private UILabel labelPrefab;
        [SerializeField] private UIButton buttonPrefab;
        [SerializeField] private UIToggle togglePrefab;
        [SerializeField] private UISlider sliderPrefab;
        [SerializeField] private UIStack stackPrefab;
        [SerializeField] private UICard cardPrefab;
        [SerializeField] private ObjectConfigurationMenu configurationMenuPrefab;

        public UILabel LabelPrefab => labelPrefab;
        public UIButton ButtonPrefab => buttonPrefab;
        public UIToggle TogglePrefab => togglePrefab;
        public UISlider SliderPrefab => sliderPrefab;
        public UIStack StackPrefab => stackPrefab;
        public UICard CardPrefab => cardPrefab;
        public ObjectConfigurationMenu ConfigurationMenuPrefab => configurationMenuPrefab;
    }
}
