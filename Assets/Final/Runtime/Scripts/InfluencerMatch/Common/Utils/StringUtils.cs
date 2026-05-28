using System.Collections.Generic;
using System.Globalization;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Hot-path string conversion helpers with caching for repeated integer-to-string conversions.
    /// </summary>
    public static class StringUtils
    {
        private const int k_FollowersThousandThreshold = 1000;
        private const int k_FollowersMillionThreshold = 1_000_000;

        private static readonly Dictionary<int, string> s_NumberCache = new Dictionary<int, string>();

        public static string GetNumberString(int value)
        {
            if (s_NumberCache.TryGetValue(value, out string cached))
            {
                return cached;
            }
            string s = value.ToString(CultureInfo.InvariantCulture);
            s_NumberCache[value] = s;
            return s;
        }

        public static string FormatFollowers(int count)
        {
            if (count < 0)
            {
                return "0";
            }

            if (count < k_FollowersThousandThreshold)
            {
                return GetNumberString(count);
            }

            if (count < k_FollowersMillionThreshold)
            {
                double thousands = count / 1000.0;
                return thousands.ToString("0.#", CultureInfo.InvariantCulture) + "K";
            }

            double millions = count / 1_000_000.0;
            return millions.ToString("0.#", CultureInfo.InvariantCulture) + "M";
        }
    }
}
