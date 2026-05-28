using System;
using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Detail;
using Final.InfluencerMatch.Recommendation;
using Final.Tests.Helpers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Final.Tests.Detail
{
    [TestFixture]
    public sealed class InfluencerDetailPresenterTests
    {
        private MatchingService m_MatchingService;
        private PricingService m_PricingService;
        private MatchingConfig m_MatchingConfig;
        private InfluencerDatabase m_Database;
        private PlatformConfig m_PlatformConfig;
        private List<InfluencerData> m_Created;
        private InfluencerDetailPresenter m_Presenter;

        [SetUp]
        public void SetUp()
        {
            m_MatchingService = new MatchingService();
            m_PricingService = new PricingService();
            m_MatchingConfig = TestDataFactory.CreateDefaultMatchingConfig();
            m_Database = ScriptableObject.CreateInstance<InfluencerDatabase>();
            m_PlatformConfig = ScriptableObject.CreateInstance<PlatformConfig>();
            m_Created = new List<InfluencerData>();
            m_Presenter = new InfluencerDetailPresenter(m_MatchingService, m_PricingService, m_Database, m_MatchingConfig, m_PlatformConfig);
        }

        [TearDown]
        public void TearDown()
        {
            TestDataFactory.DestroyAll(m_Created);
            if (m_MatchingConfig != null)
            {
                Object.DestroyImmediate(m_MatchingConfig);
                m_MatchingConfig = null;
            }
            if (m_Database != null)
            {
                Object.DestroyImmediate(m_Database);
                m_Database = null;
            }
            if (m_PlatformConfig != null)
            {
                Object.DestroyImmediate(m_PlatformConfig);
                m_PlatformConfig = null;
            }
        }

        [Test]
        public void Build_EmptyId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => m_Presenter.Build(SerializableGuid.Empty, SingleCategory(), 10_000m));
        }

        [Test]
        public void Build_UnknownId_ThrowsArgumentException()
        {
            AddInfluencer("a");
            AssignToDatabase();
            SerializableGuid unknownId = SerializableGuid.NewGuid();

            Assert.Throws<ArgumentException>(
                () => m_Presenter.Build(unknownId, SingleCategory(), 10_000m));
        }

        [Test]
        public void Build_KnownId_ReturnsViewModelWithInfluencer()
        {
            InfluencerData target = AddInfluencer("a", scores: (CategoryId.Education, 5));
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(target.Id, SingleCategory(), 10_000m);

            Assert.AreSame(target, vm.Influencer);
        }

        [Test]
        public void Build_NoSelectedCategories_CompatibilityIsZero()
        {
            InfluencerData target = AddInfluencer("a", scores: (CategoryId.Education, 5));
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(target.Id, new List<CategoryId>(), 10_000m);

            Assert.AreEqual(0, vm.CompatibilityPercent);
        }

        [Test]
        public void Build_KnownId_FinalPriceIsPositive()
        {
            InfluencerData target = AddInfluencer("a", basePrice: 5_000, scores: (CategoryId.Education, 3));
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(target.Id, SingleCategory(), 10_000m);

            Assert.Greater(vm.FinalPrice, 0);
        }

        [Test]
        public void Build_ZeroBasePrice_FinalPriceIsZero()
        {
            InfluencerData target = AddInfluencer("a", basePrice: 0, scores: (CategoryId.Education, 5));
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(target.Id, SingleCategory(), 10_000m);

            Assert.AreEqual(0, vm.FinalPrice);
        }

        [Test]
        public void Build_EngagementCount_IsFollowersTimesRate()
        {
            InfluencerData target = AddInfluencer("a", followers: 100_000, engagementRate: 0.10f, scores: (CategoryId.Education, 5));
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(target.Id, SingleCategory(), 10_000m);

            Assert.AreEqual(10_000, vm.EngagementCount);
        }

        [Test]
        public void Build_LowFollowerHighRate_BeatsHighFollowerLowRateInEngagement()
        {
            InfluencerData lowFollowerHighRate = AddInfluencer("a", followers: 50_000, engagementRate: 0.50f, scores: (CategoryId.Education, 5));
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(lowFollowerHighRate.Id, SingleCategory(), 10_000m);

            Assert.AreEqual(25_000, vm.EngagementCount, "50k followers × 50% engagement = 25k interactions, beating 100k × 10% = 10k.");
        }

        [Test]
        public void Build_ZeroFollowers_EngagementCountIsZero()
        {
            InfluencerData target = AddInfluencer("a", followers: 0, engagementRate: 0.10f, scores: (CategoryId.Education, 5));
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(target.Id, SingleCategory(), 10_000m);

            Assert.AreEqual(0, vm.EngagementCount);
        }

        [Test]
        public void Build_ZeroRate_EngagementCountIsZero()
        {
            InfluencerData target = AddInfluencer("a", followers: 100_000, engagementRate: 0f, scores: (CategoryId.Education, 5));
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(target.Id, SingleCategory(), 10_000m);

            Assert.AreEqual(0, vm.EngagementCount);
        }

        [Test]
        public void Build_NoActivePlatforms_ResolvedListIsEmpty()
        {
            InfluencerData target = AddInfluencer("a", scores: (CategoryId.Education, 5));
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(target.Id, SingleCategory(), 10_000m);

            Assert.AreEqual(0, vm.ActivePlatforms.Count);
        }

        [Test]
        public void Build_ResolvesActivePlatformsFromConfig()
        {
            InfluencerData target = AddInfluencer("a", scores: (CategoryId.Education, 5));
            AssignPlatformsToInfluencer(target, PlatformId.Instagram, PlatformId.TikTok);
            AssignPlatformDefinitions(PlatformId.Instagram, PlatformId.YouTube, PlatformId.TikTok);
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(target.Id, SingleCategory(), 10_000m);

            Assert.AreEqual(2, vm.ActivePlatforms.Count);
            Assert.AreEqual(PlatformId.Instagram, vm.ActivePlatforms[0].Id);
            Assert.AreEqual(PlatformId.TikTok, vm.ActivePlatforms[1].Id);
        }

        [Test]
        public void Build_UnknownPlatformId_IsSkipped()
        {
            InfluencerData target = AddInfluencer("a", scores: (CategoryId.Education, 5));
            AssignPlatformsToInfluencer(target, PlatformId.Instagram, PlatformId.Twitch);
            AssignPlatformDefinitions(PlatformId.Instagram);
            AssignToDatabase();

            InfluencerDetailViewModel vm = m_Presenter.Build(target.Id, SingleCategory(), 10_000m);

            Assert.AreEqual(1, vm.ActivePlatforms.Count);
            Assert.AreEqual(PlatformId.Instagram, vm.ActivePlatforms[0].Id);
        }

        private static List<CategoryId> SingleCategory() => new List<CategoryId> { CategoryId.Education };

        private InfluencerData AddInfluencer(string id, int followers = 5_000, float engagementRate = 0.05f, int basePrice = 1_000, params (CategoryId, int)[] scores)
        {
            InfluencerData data = TestDataFactory.CreateInfluencer(id, id, followers, engagementRate, basePrice, scores);
            m_Created.Add(data);
            return data;
        }

        private void AssignToDatabase()
        {
            SerializedObject so = new SerializedObject(m_Database);
            SerializedProperty list = so.FindProperty("m_Influencers");
            list.arraySize = m_Created.Count;
            for (int i = 0; i < m_Created.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = m_Created[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignPlatformsToInfluencer(InfluencerData data, params PlatformId[] platforms)
        {
            SerializedObject so = new SerializedObject(data);
            SerializedProperty list = so.FindProperty("m_ActivePlatforms");
            list.arraySize = platforms.Length;
            for (int i = 0; i < platforms.Length; i++)
            {
                list.GetArrayElementAtIndex(i).enumValueIndex = (int)platforms[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignPlatformDefinitions(params PlatformId[] platforms)
        {
            SerializedObject so = new SerializedObject(m_PlatformConfig);
            SerializedProperty list = so.FindProperty("m_Platforms");
            list.arraySize = platforms.Length;
            for (int i = 0; i < platforms.Length; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("m_Id").enumValueIndex = (int)platforms[i];
                element.FindPropertyRelative("m_DisplayName").stringValue = platforms[i].ToString();
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
