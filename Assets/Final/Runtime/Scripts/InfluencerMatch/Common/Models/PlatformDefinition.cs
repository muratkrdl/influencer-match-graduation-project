using System;
using UnityEngine;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Serializable record pairing a PlatformId with its display name and brand-color icon sprite.
    /// </summary>
    [Serializable]
    public class PlatformDefinition
    {
        [SerializeField] private PlatformId m_Id = PlatformId.None;
        [SerializeField] private string m_DisplayName;
        [SerializeField] private Sprite m_Icon;

        public PlatformId Id => m_Id;
        public string DisplayName => m_DisplayName;
        public Sprite Icon => m_Icon;
    }
}
