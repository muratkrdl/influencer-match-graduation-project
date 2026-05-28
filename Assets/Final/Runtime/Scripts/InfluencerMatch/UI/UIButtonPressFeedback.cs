using DG.Tweening;
using Final.InfluencerMatch.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Final.InfluencerMatch.UI
{
    /// <summary>
    /// Press-down scale feedback for UI Buttons; attach in the Editor next to the Button component, skips presses on non-interactable buttons.
    /// </summary>
    public class UIButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private Button m_Button;

        [Inject] private UISharedConfig m_Config;

        private Tween m_Tween;

        private void OnDestroy()
        {
            m_Tween?.Kill();
            m_Tween = null;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!m_Button.interactable)
            {
                return;
            }
            AnimateTo(m_Config.ButtonPressedScale, m_Config.ButtonPressDuration);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            AnimateTo(1f, m_Config.ButtonReleaseDuration);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            AnimateTo(1f, m_Config.ButtonReleaseDuration);
        }

        private void AnimateTo(float scale, float duration)
        {
            m_Tween?.Kill();
            m_Tween = transform.DOScale(scale, duration).SetEase(m_Config.ButtonPressEase).SetLink(gameObject);
        }
    }
}
