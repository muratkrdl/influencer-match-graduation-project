using UnityEngine;

namespace Final.InfluencerMatch.UI
{
    /// <summary>
    /// Anchors this RectTransform to the device safe area so child panels avoid notches and cutouts.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaContainer : MonoBehaviour
    {
        private RectTransform m_Rect;
        private Rect m_LastSafeArea = Rect.zero;

        private void Awake()
        {
            m_Rect = (RectTransform)transform;
        }

        private void OnEnable()
        {
            ApplySafeArea();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (m_Rect != null)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea == m_LastSafeArea)
            {
                return;
            }

            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                return;
            }

            m_LastSafeArea = safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= screenWidth;
            anchorMin.y /= screenHeight;
            anchorMax.x /= screenWidth;
            anchorMax.y /= screenHeight;

            m_Rect.anchorMin = anchorMin;
            m_Rect.anchorMax = anchorMax;
            m_Rect.offsetMin = Vector2.zero;
            m_Rect.offsetMax = Vector2.zero;
        }
    }
}
