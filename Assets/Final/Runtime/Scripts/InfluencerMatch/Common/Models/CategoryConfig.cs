using System;
using System.Collections.Generic;
using Final.Systems.ConfigManagement;
using UnityEngine;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Project-wide configuration of all category metadata (display names, icons, colors).
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/Category Config", fileName = "CategoryConfig", order = 80)]
    public class CategoryConfig : ScriptableObject, IVisibleConfig
    {
        [SerializeField] private List<CategoryDefinition> m_Categories = new List<CategoryDefinition>();

        [NonSerialized] private Dictionary<CategoryId, CategoryDefinition> m_DefinitionLookup;
        [NonSerialized] private bool m_IsInitialized;

        string IVisibleConfig.ConfigName => "Category Config";
        string IVisibleConfig.Category => "Catalog";

        public IReadOnlyList<CategoryDefinition> Categories => m_Categories;

        public bool TryGetDefinition(CategoryId id, out CategoryDefinition result)
        {
            EnsureLookup();
            return m_DefinitionLookup.TryGetValue(id, out result);
        }

        private void EnsureLookup()
        {
            if (m_IsInitialized)
            {
                return;
            }

            m_DefinitionLookup = new Dictionary<CategoryId, CategoryDefinition>(m_Categories.Count);
            foreach (CategoryDefinition def in m_Categories)
            {
                m_DefinitionLookup.TryAdd(def.Id, def);
            }

            m_IsInitialized = true;
        }
    }
}
