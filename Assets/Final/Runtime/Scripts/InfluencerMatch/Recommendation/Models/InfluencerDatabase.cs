using System;
using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.Systems.ConfigManagement;
using UnityEngine;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// Container for all influencer entries (single project-wide instance).
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/Database", fileName = "InfluencerDatabase", order = 90)]
    public class InfluencerDatabase : ScriptableObject, IVisibleConfig
    {
        [SerializeField] private List<InfluencerData> m_Influencers = new List<InfluencerData>();

        string IVisibleConfig.ConfigName => "Influencer Database";
        string IVisibleConfig.Category => "Catalog";

        [NonSerialized] private Dictionary<SerializableGuid, InfluencerData> m_IdLookup;
        [NonSerialized] private bool m_IsInitialized;

        public IReadOnlyList<InfluencerData> Influencers => m_Influencers;

        public bool TryFindById(SerializableGuid id, out InfluencerData result)
        {
            EnsureLookup();
            return m_IdLookup.TryGetValue(id, out result);
        }

        private void EnsureLookup()
        {
            if (m_IsInitialized)
            {
                return;
            }

            m_IdLookup = new Dictionary<SerializableGuid, InfluencerData>(m_Influencers.Count);
            foreach (InfluencerData entry in m_Influencers)
            {
                m_IdLookup.TryAdd(entry.Id, entry);
            }

            m_IsInitialized = true;
        }
    }
}
