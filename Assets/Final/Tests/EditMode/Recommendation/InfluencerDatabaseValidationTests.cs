using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using NUnit.Framework;
using UnityEditor;

namespace Final.Tests.Recommendation
{
    /// <summary>
    /// Loads the production InfluencerDatabase asset and asserts data-integrity invariants.
    /// </summary>
    [TestFixture]
    public sealed class InfluencerDatabaseValidationTests
    {
        private const string k_DatabasePath = "Assets/Final/Runtime/Data/InfluencerDatabase.asset";
        private const int k_ExpectedTotalCount = 100;

        private InfluencerDatabase m_Database;

        [SetUp]
        public void SetUp()
        {
            m_Database = AssetDatabase.LoadAssetAtPath<InfluencerDatabase>(k_DatabasePath);
            Assert.IsNotNull(m_Database, "InfluencerDatabase asset not found at " + k_DatabasePath);
        }

        [Test]
        public void Database_HasExpectedEntryCount()
        {
            Assert.AreEqual(k_ExpectedTotalCount, m_Database.Influencers.Count);
        }

        [Test]
        public void Database_HasNoNullEntries()
        {
            for (int i = 0; i < m_Database.Influencers.Count; i++)
            {
                Assert.IsNotNull(m_Database.Influencers[i], "Influencer at index " + i + " is null.");
            }
        }

        [Test]
        public void Database_AllEntriesHaveNonEmptyId()
        {
            for (int i = 0; i < m_Database.Influencers.Count; i++)
            {
                InfluencerData entry = m_Database.Influencers[i];
                Assert.IsFalse(entry.Id.IsEmpty, "'" + entry.name + "' (index " + i + ") has empty Id.");
            }
        }

        [Test]
        public void Database_AllIdsAreUnique()
        {
            HashSet<SerializableGuid> seen = new HashSet<SerializableGuid>();
            for (int i = 0; i < m_Database.Influencers.Count; i++)
            {
                InfluencerData entry = m_Database.Influencers[i];
                Assert.IsTrue(seen.Add(entry.Id), "Duplicate Id on '" + entry.name + "' (index " + i + ").");
            }
        }

        [Test]
        public void Database_AllEntriesHaveAvatar()
        {
            for (int i = 0; i < m_Database.Influencers.Count; i++)
            {
                InfluencerData entry = m_Database.Influencers[i];
                Assert.IsNotNull(entry.Avatar, "'" + entry.name + "' has null Avatar.");
            }
        }

        [Test]
        public void Database_AllEntriesHaveWellFormedEmail()
        {
            for (int i = 0; i < m_Database.Influencers.Count; i++)
            {
                InfluencerData entry = m_Database.Influencers[i];
                Assert.IsFalse(string.IsNullOrEmpty(entry.Email), "'" + entry.name + "' has empty Email.");
                Assert.IsTrue(entry.Email.Contains("@"), "'" + entry.name + "' Email '" + entry.Email + "' missing '@'.");
                Assert.IsTrue(entry.Email.Contains("."), "'" + entry.name + "' Email '" + entry.Email + "' missing '.'.");
            }
        }

        [Test]
        public void Database_AllEntriesHaveAtLeastOneCategoryScore()
        {
            for (int i = 0; i < m_Database.Influencers.Count; i++)
            {
                InfluencerData entry = m_Database.Influencers[i];
                Assert.Greater(entry.CategoryScores.Count, 0, "'" + entry.name + "' has no CategoryScores.");
            }
        }

        [Test]
        public void Database_AllEntriesHaveNonNegativeFollowersAndBasePrice()
        {
            for (int i = 0; i < m_Database.Influencers.Count; i++)
            {
                InfluencerData entry = m_Database.Influencers[i];
                Assert.GreaterOrEqual(entry.Followers, 0, "'" + entry.name + "' has negative Followers.");
                Assert.GreaterOrEqual(entry.BasePrice, 0, "'" + entry.name + "' has negative BasePrice.");
            }
        }

        [Test]
        public void Database_AllEntriesHaveEngagementRateInUnitRange()
        {
            for (int i = 0; i < m_Database.Influencers.Count; i++)
            {
                InfluencerData entry = m_Database.Influencers[i];
                Assert.GreaterOrEqual(entry.EngagementRate, 0f, "'" + entry.name + "' EngagementRate < 0.");
                Assert.LessOrEqual(entry.EngagementRate, 1f, "'" + entry.name + "' EngagementRate > 1.");
            }
        }
    }
}
