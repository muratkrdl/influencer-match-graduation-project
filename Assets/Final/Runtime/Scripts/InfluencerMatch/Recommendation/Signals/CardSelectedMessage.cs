using Final.InfluencerMatch.Common;
using Final.Systems.Signals;

namespace Final.InfluencerMatch.Recommendation
{
    public readonly struct CardSelectedMessage : ISignal
    {
        public readonly SerializableGuid InfluencerId;

        public CardSelectedMessage(SerializableGuid influencerId)
        {
            InfluencerId = influencerId;
        }
    }
}
