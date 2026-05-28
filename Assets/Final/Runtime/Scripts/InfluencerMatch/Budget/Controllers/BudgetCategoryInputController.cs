using System;
using Final.InfluencerMatch.Common;
using Final.Systems.EventBus.Pipes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Budget
{
    /// <summary>
    /// Drives the budget and category input panel: wires the view's events to the pure <see cref="BudgetSelectionModel"/>, mirrors validation into the view, and surfaces commit/toggle interactions as signals on the pipe.
    /// </summary>
    public class BudgetCategoryInputController : IInitializable, IDisposable
    {
        private readonly BudgetSelectionModel m_Model = new();

        [Inject] private BudgetCategoryInputView m_View;
        [Inject] private CategoryConfig m_CategoryConfig;
        [Inject] private AppState m_AppState;
        [Inject] private MainPipe m_Pipe;

        void IInitializable.Initialize()
        {
            m_View.BudgetChanged += HandleBudgetChanged;
            m_View.CategoryToggled += HandleCategoryToggled;
            m_View.ContinueClicked += HandleContinueClicked;

            m_Model.LoadFrom(m_AppState.Budget, m_AppState.SelectedCategories);

            m_View.DisplayCategories(m_CategoryConfig.Categories);
            m_View.DisplayBudget(m_Model.Budget);
            m_View.RestoreCategorySelections(m_Model.SelectedCategories);
            m_View.ShowError(null);
            ReevaluateContinue();
        }

        void IDisposable.Dispose()
        {
            m_View.BudgetChanged -= HandleBudgetChanged;
            m_View.CategoryToggled -= HandleCategoryToggled;
            m_View.ContinueClicked -= HandleContinueClicked;
        }

        private void HandleBudgetChanged(decimal value)
        {
            m_Model.SetBudget(value);
            ReevaluateContinue();
        }

        private void HandleCategoryToggled(CategoryId id, bool selected)
        {
            m_Model.SetCategory(id, selected);
            ReevaluateContinue();
        }

        private void HandleContinueClicked()
        {
            if (!m_Model.HasPositiveBudget)
            {
                m_View.ShowError("Budget must be greater than zero.");
                return;
            }

            if (!m_Model.HasSelectedCategories)
            {
                m_View.ShowError("Select at least one category.");
                return;
            }

            m_View.SetContinueEnabled(false);
            m_Model.CommitTo(m_AppState);
            m_Pipe.Raise(new BudgetCommittedMessage());
        }

        private void ReevaluateContinue()
        {
            bool isValid = m_Model.IsValid;
            m_View.SetContinueEnabled(isValid);
            if (isValid)
            {
                m_View.ShowError(null);
            }
        }
    }
}
