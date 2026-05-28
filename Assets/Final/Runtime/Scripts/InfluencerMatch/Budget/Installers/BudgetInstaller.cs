using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Budget
{
    [Serializable]
    public class BudgetInstaller : IInstaller
    {
        [Header("Views")]
        [SerializeField] private BudgetCategoryInputView m_BudgetCategoryInputView;

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<BudgetCategoryInputController>().AsSelf();
            builder.RegisterInstance(m_BudgetCategoryInputView).AsImplementedInterfaces().AsSelf();
        }
    }
}
