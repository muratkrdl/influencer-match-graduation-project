using Final.InfluencerMatch.Recommendation;
using NUnit.Framework;
using UnityEditor;

namespace Final.Tests.Common
{
    /// <summary>
    /// Integrity checks for the shipped <see cref="MatchingConfig"/> asset(s).
    /// </summary>
    [TestFixture]
    public sealed class MatchingConfigTests
    {
        private const float k_WeightSumTolerance = 0.001f;

        // MatchingService computes the match score as a plain weighted sum
        // (categoryWeight * category + followersWeight * followers + engagementWeight * engagement)
        // without dividing by the weight total, so the score stays in [0, 1] only when the weights
        // sum to 1.0. A bad sum does not change ranking (that depends on the ratios), but it
        // miscalibrates the displayed compatibility percent.
        [Test]
        public void ShippedConfigs_ScoreWeights_SumToOne()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(MatchingConfig));
            Assert.IsNotEmpty(guids, "No MatchingConfig asset found in the project.");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MatchingConfig config = AssetDatabase.LoadAssetAtPath<MatchingConfig>(path);
                Assert.IsNotNull(config, "Failed to load MatchingConfig at " + path);

                float weightSum = config.CategoryWeight + config.FollowersWeight + config.EngagementWeight;
                Assert.AreEqual(
                    1.0f,
                    weightSum,
                    k_WeightSumTolerance,
                    $"MatchingConfig '{config.name}' score weights (CategoryWeight+FollowersWeight+EngagementWeight) must sum to 1.0 " +
                    $"for a normalized match score, but sum to {weightSum}. ({path})");
            }
        }
    }
}
