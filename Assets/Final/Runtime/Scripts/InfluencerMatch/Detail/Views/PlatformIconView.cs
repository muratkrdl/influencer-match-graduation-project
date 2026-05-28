using Final.InfluencerMatch.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Final.InfluencerMatch.Detail
{
    /// <summary>
    /// Single platform icon entry in the detail panel's active-platforms row.
    /// </summary>
    public class PlatformIconView : MonoBehaviour
    {
        [SerializeField] private Image m_IconImage;

        public void Bind(PlatformDefinition definition)
        {
            m_IconImage.sprite = definition.Icon;
            m_IconImage.enabled = true;
        }
    }
}
