using System;

namespace Evgh.MixedUI
{
    /// <summary>Optional invalidation notification implemented by contextual targets.</summary>
    public interface IUITargetLifetime
    {
        bool IsValid { get; }
        event Action Invalidated;
    }
}

