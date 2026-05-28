using System.Linq;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using NUnit.Framework;

namespace Final.Tests.Budget
{
    [TestFixture]
    public sealed class BudgetSelectionModelTests
    {
        private BudgetSelectionModel m_Model;

        [SetUp]
        public void SetUp()
        {
            m_Model = new BudgetSelectionModel();
        }

        [Test]
        public void Initial_IsEmptyAndInvalid()
        {
            Assert.AreEqual(0m, m_Model.Budget);
            Assert.AreEqual(0, m_Model.SelectedCategories.Count);
            Assert.IsFalse(m_Model.HasPositiveBudget);
            Assert.IsFalse(m_Model.HasSelectedCategories);
            Assert.IsFalse(m_Model.IsValid);
        }

        [Test]
        public void SetBudget_ClampsNegativeToZero()
        {
            m_Model.SetBudget(-50m);
            Assert.AreEqual(0m, m_Model.Budget);
            Assert.IsFalse(m_Model.HasPositiveBudget);
        }

        [Test]
        public void SetBudget_PositiveValue_IsStored()
        {
            m_Model.SetBudget(1_000m);
            Assert.AreEqual(1_000m, m_Model.Budget);
            Assert.IsTrue(m_Model.HasPositiveBudget);
        }

        [Test]
        public void SetCategory_AddsAndRemoves()
        {
            m_Model.SetCategory(CategoryId.Education, true);
            Assert.IsTrue(m_Model.SelectedCategories.Contains(CategoryId.Education));

            m_Model.SetCategory(CategoryId.Education, false);
            Assert.IsFalse(m_Model.SelectedCategories.Contains(CategoryId.Education));
        }

        [Test]
        public void SetCategory_IsIdempotent()
        {
            m_Model.SetCategory(CategoryId.Sports, true);
            m_Model.SetCategory(CategoryId.Sports, true);
            Assert.AreEqual(1, m_Model.SelectedCategories.Count);
        }

        [Test]
        public void IsValid_RequiresBothBudgetAndCategory()
        {
            m_Model.SetBudget(500m);
            Assert.IsFalse(m_Model.IsValid, "budget only is not enough");

            m_Model.SetCategory(CategoryId.Technology, true);
            Assert.IsTrue(m_Model.IsValid);

            m_Model.SetBudget(0m);
            Assert.IsFalse(m_Model.IsValid, "category only is not enough");
        }

        [Test]
        public void LoadFrom_ReplacesStateAndClampsBudget()
        {
            m_Model.SetCategory(CategoryId.Sports, true);

            m_Model.LoadFrom(-10m, new[] { CategoryId.Education, CategoryId.Technology });

            Assert.AreEqual(0m, m_Model.Budget, "negative budget clamped");
            Assert.AreEqual(2, m_Model.SelectedCategories.Count);
            Assert.IsTrue(m_Model.SelectedCategories.Contains(CategoryId.Education));
            Assert.IsTrue(m_Model.SelectedCategories.Contains(CategoryId.Technology));
            Assert.IsFalse(m_Model.SelectedCategories.Contains(CategoryId.Sports), "prior selection cleared");
        }

        [Test]
        public void CommitTo_WritesBudgetAndCategoriesIntoAppState()
        {
            m_Model.SetBudget(2_500m);
            m_Model.SetCategory(CategoryId.Education, true);
            m_Model.SetCategory(CategoryId.Sports, true);

            AppState state = new AppState();
            m_Model.CommitTo(state);

            Assert.AreEqual(2_500m, state.Budget);
            Assert.AreEqual(2, state.SelectedCategories.Count);
            Assert.IsTrue(state.SelectedCategories.Contains(CategoryId.Education));
            Assert.IsTrue(state.SelectedCategories.Contains(CategoryId.Sports));
        }
    }
}
