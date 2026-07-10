using System;
using UnityEngine;

namespace Evgh.MixedUI
{
    /// <summary>Applies a spatial context once or follows its anchor in LateUpdate.</summary>
    public sealed class UISpatialFollower : MonoBehaviour
    {
        private UISpatialContext _context;
        private Transform _viewer;

        public void Initialize(UISpatialContext context, Transform viewer)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _viewer = viewer != null ? viewer : throw new ArgumentNullException(nameof(viewer));
            ApplyPlacement();
            enabled = context.PlacementPolicy == UIPlacementPolicy.FollowAnchor;
        }

        private void LateUpdate()
        {
            if (_context?.Anchor == null)
            {
                enabled = false;
                return;
            }
            ApplyPlacement();
        }

        private void ApplyPlacement()
        {
            var anchor = _context.Anchor;
            transform.position = _context.OffsetSpace == UIOffsetSpace.Local
                ? anchor.TransformPoint(_context.Offset)
                : anchor.position + _context.Offset;

            if (!_context.FaceViewer) transform.rotation = anchor.rotation;
            else
            {
                var awayFromViewer = transform.position - _viewer.position;
                awayFromViewer.y = 0f;
                if (awayFromViewer.sqrMagnitude > 0.000001f)
                    transform.rotation = Quaternion.LookRotation(awayFromViewer.normalized, Vector3.up);
            }
        }
    }
}

