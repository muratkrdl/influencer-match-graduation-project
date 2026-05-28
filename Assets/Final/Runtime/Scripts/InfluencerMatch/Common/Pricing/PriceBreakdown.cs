namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Immutable summary of a price calculation for a single influencer.
    /// </summary>
    public readonly struct PriceBreakdown
    {
        public readonly int BasePrice;
        public readonly float CategoryMultiplier;
        public readonly float FollowerMultiplier;
        public readonly int FinalPrice;

        public PriceBreakdown(int basePrice, float categoryMultiplier, float followerMultiplier, int finalPrice)
        {
            BasePrice = basePrice < 0 ? 0 : basePrice;
            CategoryMultiplier = categoryMultiplier;
            FollowerMultiplier = followerMultiplier;
            FinalPrice = finalPrice < 0 ? 0 : finalPrice;
        }
    }
}
