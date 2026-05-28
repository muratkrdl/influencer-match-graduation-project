using System.Linq;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using NUnit.Framework;

namespace Final.Tests.Budget
{
    [TestFixture]
    public sealed class AppStateTests
    {
        [Test]
        public void Initial_Budget_IsZero()
        {
            AppState state = new AppState();
            Assert.AreEqual(0m, state.Budget);
        }

        [Test]
        public void Initial_SelectedCategories_IsEmpty()
        {
            AppState state = new AppState();
            Assert.IsNotNull(state.SelectedCategories);
            Assert.AreEqual(0, state.SelectedCategories.Count);
        }

        [Test]
        public void SetSelectedCategories_ReplacesSelection()
        {
            AppState state = new AppState();
            state.SetSelectedCategories(new[] { CategoryId.Sports });
            Assert.AreEqual(1, state.SelectedCategories.Count);
            Assert.AreEqual(CategoryId.Sports, state.SelectedCategories[0]);

            state.SetSelectedCategories(new[] { CategoryId.Education, CategoryId.Technology });
            Assert.AreEqual(2, state.SelectedCategories.Count);
            Assert.IsFalse(state.SelectedCategories.Contains(CategoryId.Sports));
        }
    }
}
