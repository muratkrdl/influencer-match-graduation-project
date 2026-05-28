using Final.InfluencerMatch.Common;
using NUnit.Framework;

namespace Final.Tests.Common
{
    [TestFixture]
    public sealed class PriceBreakdownTests
    {
        [Test]
        public void Ctor_PreservesValidValues()
        {
            PriceBreakdown b = new PriceBreakdown(1_000, 1.5f, 2.0f, 3_000);

            Assert.AreEqual(1_000, b.BasePrice);
            Assert.AreEqual(1.5f, b.CategoryMultiplier);
            Assert.AreEqual(2.0f, b.FollowerMultiplier);
            Assert.AreEqual(3_000, b.FinalPrice);
        }

        [Test]
        public void Ctor_NegativeBasePrice_ClampedToZero()
        {
            PriceBreakdown b = new PriceBreakdown(-100, 1.0f, 1.0f, 0);
            Assert.AreEqual(0, b.BasePrice);
        }

        [Test]
        public void Ctor_NegativeFinalPrice_ClampedToZero()
        {
            PriceBreakdown b = new PriceBreakdown(1_000, 1.0f, 1.0f, -100);
            Assert.AreEqual(0, b.FinalPrice);
        }

        [Test]
        public void Ctor_DoesNotClampMultipliers()
        {
            // Multipliers stay as caller passed them; PricingService bounds inputs upstream.
            PriceBreakdown b = new PriceBreakdown(1_000, -1.0f, 0.0f, 1_000);
            Assert.AreEqual(-1.0f, b.CategoryMultiplier);
            Assert.AreEqual(0.0f, b.FollowerMultiplier);
        }
    }
}
