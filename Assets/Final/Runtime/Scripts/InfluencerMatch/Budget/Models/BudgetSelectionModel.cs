using System.Collections.Generic;
using Final.InfluencerMatch.Common;

namespace Final.InfluencerMatch.Budget
{
    /// <summary>
    /// Pure input model for the budget/category panel: holds the pending budget and category selection, exposes validation, and commits the result to <see cref="AppState"/>.
    /// </summary>
    public class BudgetSelectionModel
    {
        private readonly HashSet<CategoryId> m_SelectedCategories = new HashSet<CategoryId>();

        private decimal m_Budget;

        public decimal Budget => m_Budget;
        public IReadOnlyCollection<CategoryId> SelectedCategories => m_SelectedCategories;
        public bool HasPositiveBudget => Budget > 0m;
        public bool HasSelectedCategories => m_SelectedCategories.Count > 0;
        public bool IsValid => HasPositiveBudget && HasSelectedCategories;

        public void SetBudget(decimal value)
        {
            m_Budget = value < 0m ? 0m : value;
        }

        public void SetCategory(CategoryId id, bool selected)
        {
            if (selected)
            {
                m_SelectedCategories.Add(id);
            }
            else
            {
                m_SelectedCategories.Remove(id);
            }
        }

        public void LoadFrom(decimal budget, IEnumerable<CategoryId> categories)
        {
            m_Budget = budget < 0m ? 0m : budget;
            m_SelectedCategories.Clear();
            foreach (CategoryId id in categories)
            {
                m_SelectedCategories.Add(id);
            }
        }

        public void CommitTo(AppState appState)
        {
            appState.Budget = Budget;
            appState.SetSelectedCategories(m_SelectedCategories);
        }
    }
}
