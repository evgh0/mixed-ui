using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    public sealed class ContextualUIBuilder<TTarget> where TTarget : class
    {
        internal UIContext Context { get; }
        internal TTarget Target { get; }
        internal Transform Anchor { get; private set; }
        internal Vector3 Offset { get; private set; }
        internal UIOffsetSpace OffsetSpace { get; private set; } = UIOffsetSpace.Local;
        internal bool ShouldFaceViewer { get; private set; }
        internal UIPlacementPolicy PlacementPolicy { get; private set; } = UIPlacementPolicy.PlaceOnce;

        internal ContextualUIBuilder(UIContext context, TTarget target)
        {
            Context = context;
            Target = target;
        }

        public ContextualUIBuilder<TTarget> AnchoredTo(Transform anchor)
        {
            Anchor = anchor != null ? anchor : throw new ArgumentNullException(nameof(anchor));
            return this;
        }

        public ContextualUIBuilder<TTarget> WithLocalOffset(Vector3 offset)
        {
            Offset = offset;
            OffsetSpace = UIOffsetSpace.Local;
            return this;
        }

        public ContextualUIBuilder<TTarget> WithWorldOffset(Vector3 offset)
        {
            Offset = offset;
            OffsetSpace = UIOffsetSpace.World;
            return this;
        }

        public ContextualUIBuilder<TTarget> FaceUser()
        {
            ShouldFaceViewer = true;
            return this;
        }

        public ContextualUIBuilder<TTarget> FollowAnchor()
        {
            PlacementPolicy = UIPlacementPolicy.FollowAnchor;
            return this;
        }
    }

    public static class ConfigurationPanelBuilderExtensions
    {
        public static ContextualPanelBuilder<TTarget> ConfigurationPanel<TTarget>(
            this ContextualUIBuilder<TTarget> builder,
            string title) where TTarget : class, IConfigurableObject =>
            new(builder ?? throw new ArgumentNullException(nameof(builder)), title);
    }
}

