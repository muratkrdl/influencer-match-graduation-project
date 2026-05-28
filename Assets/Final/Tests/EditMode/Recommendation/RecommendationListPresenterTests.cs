using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.Tests.Helpers;
using NUnit.Framework;

namespace Final.Tests.Recommendation
{
    [TestFixture]
    public sealed class RecommendationListPresenterTests
    {
        private InfluencerData m_Dummy;
        private List<InfluencerData> m_Created;

        [SetUp]
        public void SetUp()
        {
            m_Created = new List<InfluencerData>();
            m_Dummy = TestDataFactory.CreateInfluencer("dummy", "Dummy", 5_000, 0.05f, 5_000);
            m_Created.Add(m_Dummy);
        }

        [TearDown]
        public void TearDown()
        {
            TestDataFactory.DestroyAll(m_Created);
        }

        [Test]
        public void Build_NoSelection_ReturnsNullRankedAndDoesNotRank()
        {
            FakeMatchingService matching = new FakeMatchingService(ScoredList(3));
            RecommendationListPresenter presenter = NewPresenter(matching);

            RecommendationListViewModel viewModel = presenter.Build(
                new List<CategoryId>(),
                10_000m,
                m_Created);

            Assert.IsNull(viewModel.Ranked);
            Assert.AreEqual("No categories selected.", viewModel.Subtitle);
            Assert.AreEqual(0, viewModel.ResultCount);
            Assert.IsFalse(matching.RankCalled, "Rank must be short-circuited when nothing is selected.");
        }

        [Test]
        public void Build_NoMatches_ReturnsEmptyStateSubtitle()
        {
            FakeMatchingService matching = new FakeMatchingService(ScoredList(0));
            RecommendationListPresenter presenter = NewPresenter(matching);

            RecommendationListViewModel viewModel = presenter.Build(SingleCategory(), 10_000m, m_Created);

            Assert.IsNotNull(viewModel.Ranked);
            Assert.AreEqual(0, viewModel.ResultCount);
            Assert.AreEqual("No influencers found", viewModel.Subtitle);
        }

        [Test]
        public void Build_SingleMatch_ReturnsSingularSubtitle()
        {
            FakeMatchingService matching = new FakeMatchingService(ScoredList(1));
            RecommendationListPresenter presenter = NewPresenter(matching);

            RecommendationListViewModel viewModel = presenter.Build(SingleCategory(), 10_000m, m_Created);

            Assert.AreEqual(1, viewModel.ResultCount);
            Assert.AreEqual("1 influencer matches your criteria", viewModel.Subtitle);
        }

        [Test]
        public void Build_MultipleMatches_ReturnsPluralSubtitle()
        {
            FakeMatchingService matching = new FakeMatchingService(ScoredList(3));
            RecommendationListPresenter presenter = NewPresenter(matching);

            RecommendationListViewModel viewModel = presenter.Build(SingleCategory(), 10_000m, m_Created);

            Assert.AreEqual(3, viewModel.ResultCount);
            Assert.AreEqual("3 influencers match your criteria", viewModel.Subtitle);
        }

        [Test]
        public void Build_ForwardsSelectionBudgetAndPoolToMatchingService()
        {
            FakeMatchingService matching = new FakeMatchingService(ScoredList(1));
            RecommendationListPresenter presenter = NewPresenter(matching);
            List<CategoryId> selected = SingleCategory();

            presenter.Build(selected, 7_500m, m_Created);

            Assert.AreSame(selected, matching.LastCategories);
            Assert.AreEqual(7_500m, matching.LastBudget);
            Assert.AreSame(m_Created, matching.LastPool);
        }

        // Config and pricing are forwarded untouched to the matching service, so the
        // presenter is exercised end to end with nulls and a fake that ignores them.
        private static RecommendationListPresenter NewPresenter(IMatchingService matching)
        {
            return new RecommendationListPresenter(matching, null, null);
        }

        private static List<CategoryId> SingleCategory()
        {
            return new List<CategoryId> { CategoryId.Education };
        }

        private IReadOnlyList<ScoredInfluencer> ScoredList(int count)
        {
            List<ScoredInfluencer> list = new List<ScoredInfluencer>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(new ScoredInfluencer(m_Dummy, 0.5f, false));
            }
            return list;
        }

        private sealed class FakeMatchingService : IMatchingService
        {
            private readonly IReadOnlyList<ScoredInfluencer> m_Result;

            public FakeMatchingService(IReadOnlyList<ScoredInfluencer> result)
            {
                m_Result = result;
            }

            public bool RankCalled { get; private set; }
            public IReadOnlyList<CategoryId> LastCategories { get; private set; }
            public decimal LastBudget { get; private set; }
            public IReadOnlyList<InfluencerData> LastPool { get; private set; }

            public IReadOnlyList<ScoredInfluencer> Rank(
                IReadOnlyList<CategoryId> selectedCategories,
                decimal budget,
                IReadOnlyList<InfluencerData> pool,
                MatchingConfig config,
                IPricingService pricing)
            {
                RankCalled = true;
                LastCategories = selectedCategories;
                LastBudget = budget;
                LastPool = pool;
                return m_Result;
            }
        }
    }
}
