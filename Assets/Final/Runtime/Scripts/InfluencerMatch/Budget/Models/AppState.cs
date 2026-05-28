using System.Collections.Generic;
using Final.InfluencerMatch.Common;

namespace Final.InfluencerMatch.Budget
{
    /// <summary>
    /// Mutable session state shared across the UI.
    /// </summary>
    public class AppState
    {
        private readonly List<CategoryId> m_SelectedCategories = new();

        private decimal m_Budget;

        public IReadOnlyList<CategoryId> SelectedCategories => m_SelectedCategories;

        public decimal Budget
        {
            get => m_Budget;
            set => m_Budget = value;
        }

        public void SetSelectedCategories(IEnumerable<CategoryId> categories)
        {
            m_SelectedCategories.Clear();
            foreach (CategoryId id in categories)
            {
                m_SelectedCategories.Add(id);
            }
        }
    }
}
