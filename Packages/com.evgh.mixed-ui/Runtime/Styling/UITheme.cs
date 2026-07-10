using System;
using TMPro;
using UnityEngine;

namespace Evgh.MixedUI
{
    public enum UIStyleVariant { Primary, Secondary, Subtle, Destructive, Success }
    public enum UILabelStyle { Body, Heading }

    [Serializable]
    public sealed class UIColorTokens
    {
        public Color surface = new(0.08f, 0.1f, 0.14f, 0.96f);
        public Color primary = new(0.1f, 0.55f, 1f, 1f);
        public Color secondary = new(0.24f, 0.28f, 0.34f, 1f);
        public Color subtle = new(0.14f, 0.17f, 0.22f, 1f);
        public Color destructive = new(0.8f, 0.16f, 0.18f, 1f);
        public Color success = new(0.1f, 0.65f, 0.32f, 1f);
        public Color primaryText = Color.white;
        public Color secondaryText = new(0.78f, 0.82f, 0.88f, 1f);
        public Color disabled = new(0.35f, 0.37f, 0.4f, 0.7f);
        public Color hovered = new(1f, 1f, 1f, 0.12f);
        public Color pressed = new(0f, 0f, 0f, 0.18f);

        public Color ForVariant(UIStyleVariant variant) => variant switch
        {
            UIStyleVariant.Primary => primary,
            UIStyleVariant.Secondary => secondary,
            UIStyleVariant.Subtle => subtle,
            UIStyleVariant.Destructive => destructive,
            UIStyleVariant.Success => success,
            _ => secondary
        };
    }

    [Serializable]
    public sealed class UISpacingTokens
    {
        [Min(0)] public float small = 8f;
        [Min(0)] public float medium = 16f;
        [Min(0)] public float large = 24f;
    }

    [Serializable]
    public sealed class UITypographyTokens
    {
        public TMP_FontAsset defaultFont;
        [Min(1)] public float bodySize = 24f;
        [Min(1)] public float headingSize = 32f;
    }

    [Serializable]
    public sealed class UIGeometryTokens
    {
        public Vector2 standardControlSize = new(320f, 56f);
        [Min(1)] public float canvasUnitsPerMeter = 1000f;
        public Sprite roundedSurfaceSprite;
    }

    [Serializable]
    public sealed class UIInteractionTokens
    {
        [Range(0.5f, 1f)] public float pressedScale = 0.97f;
        [Min(0)] public float transitionDuration = 0.08f;
    }

    /// <summary>Semantic visual and interaction tokens shared by all elements.</summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "Evgh/Mixed UI/Theme")]
    public sealed class UITheme : ScriptableObject
    {
        public UIColorTokens colors = new();
        public UISpacingTokens spacing = new();
        public UITypographyTokens typography = new();
        public UIGeometryTokens geometry = new();
        public UIInteractionTokens interaction = new();

        private void OnValidate()
        {
            geometry.canvasUnitsPerMeter = Mathf.Max(1f, geometry.canvasUnitsPerMeter);
            interaction.transitionDuration = Mathf.Max(0f, interaction.transitionDuration);
        }
    }
}
