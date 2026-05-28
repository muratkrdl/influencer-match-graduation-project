using System;
using UnityEngine;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Visual and metadata definition for a single category.
    /// </summary>
    [Serializable]
    public class CategoryDefinition
    {
        [Header("Identity")]
        [SerializeField] private CategoryId m_Id = CategoryId.None;
        [SerializeField] private string m_DisplayName = string.Empty;

        [Header("Visual")]
        [SerializeField] private Sprite m_Icon;
        [SerializeField] private Color m_AccentColor = Color.white;

        public CategoryId Id => m_Id;
        public string DisplayName => m_DisplayName;
        public Sprite Icon => m_Icon;
        public Color AccentColor => m_AccentColor;
    }
}
