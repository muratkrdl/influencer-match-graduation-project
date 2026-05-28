using System;
using System.Collections.Generic;
using Final.Systems.ConfigManagement;
using UnityEngine;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Project-wide registry of social-media platform definitions (display name + brand-color icon per PlatformId).
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/Platform Config", fileName = "PlatformConfig", order = 90)]
    public class PlatformConfig : ScriptableObject, IVisibleConfig
    {
        [SerializeField] private List<PlatformDefinition> m_Platforms = new List<PlatformDefinition>();

        [NonSerialized] private Dictionary<PlatformId, PlatformDefinition> m_DefinitionLookup;
        [NonSerialized] private bool m_IsInitialized;

        string IVisibleConfig.ConfigName => "Platform Config";
        string IVisibleConfig.Category => "Catalog";

        public IReadOnlyList<PlatformDefinition> Platforms => m_Platforms;

        public bool TryGetDefinition(PlatformId id, out PlatformDefinition result)
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

            m_DefinitionLookup = new Dictionary<PlatformId, PlatformDefinition>(m_Platforms.Count);
            foreach (PlatformDefinition def in m_Platforms)
            {
                m_DefinitionLookup.TryAdd(def.Id, def);
            }

            m_IsInitialized = true;
        }
    }
}
