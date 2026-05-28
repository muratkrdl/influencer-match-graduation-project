using Final.Systems.Signals;

namespace Final.InfluencerMatch.EmptyState
{
    /// <summary>
    /// Raised to surface the empty-state panel when matching yields zero recommendations.
    /// </summary>
    public readonly struct EmptyStateRequestedMessage : ISignal
    {
    }
}
