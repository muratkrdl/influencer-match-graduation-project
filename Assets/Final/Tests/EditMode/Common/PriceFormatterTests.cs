using Final.InfluencerMatch.Common;
using NUnit.Framework;

namespace Final.Tests.Common
{
    [TestFixture]
    public sealed class PriceFormatterTests
    {
        [Test]
        public void Format_PrependsDollarAndThousandSeparators()
        {
            Assert.AreEqual("$0", PriceFormatter.Format(0));
            Assert.AreEqual("$1,000", PriceFormatter.Format(1_000));
            Assert.AreEqual("$1,234,567", PriceFormatter.Format(1_234_567));
        }

        [Test]
        public void FormatNumeric_UsesThousandSeparator_WithoutPrefix()
        {
            Assert.AreEqual("0", PriceFormatter.FormatNumeric(0m));
            Assert.AreEqual("1,000", PriceFormatter.FormatNumeric(1_000m));
            Assert.AreEqual("10,000,000", PriceFormatter.FormatNumeric(10_000_000m));
        }

        [Test]
        public void TryParse_NullOrEmpty_ReturnsFalseAndZero()
        {
            Assert.IsFalse(PriceFormatter.TryParse(null, out decimal v1));
            Assert.AreEqual(0m, v1);

            Assert.IsFalse(PriceFormatter.TryParse(string.Empty, out decimal v2));
            Assert.AreEqual(0m, v2);
        }

        [Test]
        public void TryParse_PlainDigits()
        {
            Assert.IsTrue(PriceFormatter.TryParse("10000", out decimal v));
            Assert.AreEqual(10_000m, v);
        }

        [Test]
        public void TryParse_AcceptsCommaThousandSeparator()
        {
            Assert.IsTrue(PriceFormatter.TryParse("10,000", out decimal v));
            Assert.AreEqual(10_000m, v);
        }

        // Regression guard: InvariantCulture used to treat "10.000" as 10.0; users typing dots
        // as thousand separators ended up with a budget of 10. Both separators now yield 10000.
        [Test]
        public void TryParse_AcceptsDotAsThousandSeparator_LocaleTrapGuard()
        {
            Assert.IsTrue(PriceFormatter.TryParse("10.000", out decimal v));
            Assert.AreEqual(10_000m, v);
        }

        [Test]
        public void TryParse_StripsDollarPrefix()
        {
            Assert.IsTrue(PriceFormatter.TryParse("$10,000", out decimal v));
            Assert.AreEqual(10_000m, v);
        }

        [Test]
        public void TryParse_IgnoresWhitespaceAndNonDigitNoise()
        {
            Assert.IsTrue(PriceFormatter.TryParse("  10 000 ", out decimal v1));
            Assert.AreEqual(10_000m, v1);

            // Letters mixed with digits: digits are extracted, letters dropped.
            Assert.IsTrue(PriceFormatter.TryParse("abc123", out decimal v2));
            Assert.AreEqual(123m, v2);
        }

        [Test]
        public void TryParse_AllNonDigits_ReturnsFalse()
        {
            Assert.IsFalse(PriceFormatter.TryParse("abc", out decimal v));
            Assert.AreEqual(0m, v);
        }
    }
}
