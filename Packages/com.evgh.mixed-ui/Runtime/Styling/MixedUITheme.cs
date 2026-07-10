using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI
{
    public enum MixedUIVariant
    {
        Primary,
        Secondary,
        Subtle,
        Destructive,
        Success,
    }

    public enum MixedUITextRole
    {
        Body,
        Heading,
        Caption,
    }

    [Serializable]
    public sealed class MixedUIColorPalette
    {
        public Color surface = new(0.055f, 0.071f, 0.102f, 0.96f);
        public Color elevatedSurface = new(0.09f, 0.11f, 0.15f, 0.98f);
        public Color primary = new(0.08f, 0.48f, 0.95f, 1f);
        public Color secondary = new(0.22f, 0.26f, 0.33f, 1f);
        public Color subtle = new(0.12f, 0.15f, 0.2f, 1f);
        public Color destructive = new(0.82f, 0.14f, 0.18f, 1f);
        public Color success = new(0.08f, 0.62f, 0.32f, 1f);
        public Color textPrimary = Color.white;
        public Color textSecondary = new(0.72f, 0.77f, 0.84f, 1f);
        public Color disabled = new(0.28f, 0.3f, 0.34f, 0.72f);
        public Color border = new(1f, 1f, 1f, 0.12f);

        public Color ForVariant(MixedUIVariant variant) => variant switch
        {
            MixedUIVariant.Primary => primary,
            MixedUIVariant.Secondary => secondary,
            MixedUIVariant.Subtle => subtle,
            MixedUIVariant.Destructive => destructive,
            MixedUIVariant.Success => success,
            _ => secondary,
        };
    }

    [Serializable]
    public sealed class MixedUITypography
    {
        public TMP_FontAsset defaultFont;
        [Min(1f)] public float bodySize = 24f;
        [Min(1f)] public float headingSize = 34f;
        [Min(1f)] public float captionSize = 18f;
    }

    [Serializable]
    public sealed class MixedUISpacing
    {
        [Min(0f)] public float small = 8f;
        [Min(0f)] public float medium = 16f;
        [Min(0f)] public float large = 24f;
    }

    [Serializable]
    public sealed class MixedUIMetrics
    {
        [Min(1f)] public float canvasUnitsPerMeter = 1000f;
        public Vector2 controlSize = new(320f, 56f);
        [Min(0f)] public float selectableFadeDuration = 0.08f;
        [Range(0f, 1f)] public float highlightedBlend = 0.12f;
        [Range(0f, 1f)] public float pressedBlend = 0.2f;
        [Range(0f, 1f)] public float selectedBlend = 0.16f;
    }

    [Serializable]
    public sealed class MixedUISprites
    {
        public Sprite roundedSurface;
        public Sprite roundedControl;
    }

    [CreateAssetMenu(fileName = "MixedUITheme", menuName = "Mixed-UI/Theme")]
    public sealed class MixedUITheme : ScriptableObject
    {
        public MixedUIColorPalette colors = new();
        public MixedUITypography typography = new();
        public MixedUISpacing spacing = new();
        public MixedUIMetrics metrics = new();
        public MixedUISprites sprites = new();

        public ColorBlock CreateColorBlock(MixedUIVariant variant)
        {
            var normal = colors.ForVariant(variant);
            return new ColorBlock
            {
                normalColor = normal,
                highlightedColor = Color.Lerp(normal, Color.white, metrics.highlightedBlend),
                pressedColor = Color.Lerp(normal, Color.black, metrics.pressedBlend),
                selectedColor = Color.Lerp(normal, Color.white, metrics.selectedBlend),
                disabledColor = colors.disabled,
                colorMultiplier = 1f,
                fadeDuration = metrics.selectableFadeDuration,
            };
        }

        public float FontSizeFor(MixedUITextRole role) => role switch
        {
            MixedUITextRole.Heading => typography.headingSize,
            MixedUITextRole.Caption => typography.captionSize,
            _ => typography.bodySize,
        };

        private void OnValidate()
        {
            metrics.canvasUnitsPerMeter = Mathf.Max(1f, metrics.canvasUnitsPerMeter);
            metrics.selectableFadeDuration = Mathf.Max(0f, metrics.selectableFadeDuration);
        }
    }
}

