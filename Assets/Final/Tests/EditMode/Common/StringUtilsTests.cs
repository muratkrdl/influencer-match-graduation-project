using Final.InfluencerMatch.Common;
using NUnit.Framework;

namespace Final.Tests.Common
{
    [TestFixture]
    public sealed class StringUtilsTests
    {
        [Test]
        public void GetNumberString_FormatsAsInvariantInteger()
        {
            Assert.AreEqual("0", StringUtils.GetNumberString(0));
            Assert.AreEqual("1234", StringUtils.GetNumberString(1234));
            Assert.AreEqual("-1", StringUtils.GetNumberString(-1));
        }

        [Test]
        public void GetNumberString_CachesIdenticalCalls()
        {
            string a = StringUtils.GetNumberString(424242);
            string b = StringUtils.GetNumberString(424242);
            Assert.AreSame(a, b);
        }

        [Test]
        public void FormatFollowers_BelowThousand_PlainDigits()
        {
            Assert.AreEqual("0", StringUtils.FormatFollowers(0));
            Assert.AreEqual("999", StringUtils.FormatFollowers(999));
        }

        [Test]
        public void FormatFollowers_Negative_ReturnsZero()
        {
            Assert.AreEqual("0", StringUtils.FormatFollowers(-1));
            Assert.AreEqual("0", StringUtils.FormatFollowers(-1_000_000));
        }

        [Test]
        public void FormatFollowers_Thousands_UsesKSuffix_OneDecimalMax()
        {
            Assert.AreEqual("1K", StringUtils.FormatFollowers(1_000));
            Assert.AreEqual("9.5K", StringUtils.FormatFollowers(9_500));
            Assert.AreEqual("12.3K", StringUtils.FormatFollowers(12_345));
            Assert.AreEqual("999.9K", StringUtils.FormatFollowers(999_900));
        }

        [Test]
        public void FormatFollowers_Millions_UsesMSuffix_OneDecimalMax()
        {
            Assert.AreEqual("1M", StringUtils.FormatFollowers(1_000_000));
            Assert.AreEqual("1.5M", StringUtils.FormatFollowers(1_500_000));
            Assert.AreEqual("12.3M", StringUtils.FormatFollowers(12_345_678));
        }
    }
}
