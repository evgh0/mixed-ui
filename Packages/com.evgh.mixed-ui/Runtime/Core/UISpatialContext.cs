using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    public enum UIPlacementPolicy { PlaceOnce, FollowAnchor }
    public enum UIOffsetSpace { Local, World }

    /// <summary>Placement data kept separate from the bound business target.</summary>
    public sealed class UISpatialContext
    {
        public Transform Anchor { get; }
        public Vector3 Offset { get; }
        public UIOffsetSpace OffsetSpace { get; }
        public bool FaceViewer { get; }
        public UIPlacementPolicy PlacementPolicy { get; }

        public UISpatialContext(
            Transform anchor,
            Vector3 offset,
            UIOffsetSpace offsetSpace = UIOffsetSpace.Local,
            bool faceViewer = false,
            UIPlacementPolicy placementPolicy = UIPlacementPolicy.PlaceOnce)
        {
            Anchor = anchor != null ? anchor : throw new ArgumentNullException(nameof(anchor));
            Offset = offset;
            OffsetSpace = offsetSpace;
            FaceViewer = faceViewer;
            PlacementPolicy = placementPolicy;
        }
    }
}

