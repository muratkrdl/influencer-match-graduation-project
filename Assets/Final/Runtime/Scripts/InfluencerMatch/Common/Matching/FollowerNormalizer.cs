using System;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Logarithmic normalization of follower counts to a [0, 1] scalar.
    /// </summary>
    public static class FollowerNormalizer
    {
        private const float k_Min01 = 0f;
        private const float k_Max01 = 1f;

        public static float Normalize(int followers, int min, int max)
        {
            if (followers <= 0)
            {
                return k_Min01;
            }

            if (min >= max)
            {
                return k_Min01;
            }

            if (followers <= min)
            {
                return k_Min01;
            }

            if (followers >= max)
            {
                return k_Max01;
            }

            double logMin = Math.Log10(min + 1.0);
            double logMax = Math.Log10(max + 1.0);
            double denominator = logMax - logMin;

            if (denominator <= 0.0)
            {
                return k_Min01;
            }

            double numerator = Math.Log10(followers + 1.0) - logMin;
            double ratio = numerator / denominator;

            if (ratio < k_Min01)
            {
                return k_Min01;
            }

            if (ratio > k_Max01)
            {
                return k_Max01;
            }

            return (float)ratio;
        }
    }
}
