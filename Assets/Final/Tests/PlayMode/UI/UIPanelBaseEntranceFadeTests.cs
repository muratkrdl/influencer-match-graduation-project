using System.Collections;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.UI;
using Final.Tests.PlayMode.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Final.Tests.PlayMode.UI
{
    /// <summary>
    /// PlayMode tests for <see cref="UIPanelBase"/>'s entrance fade. PlayMode is required
    /// because the fade is DOTween-driven and needs real frame-time to tick.
    /// </summary>
    public sealed class UIPanelBaseEntranceFadeTests
    {
        private const float k_PostFadeBufferSeconds = 0.1f;

        private GameObject m_Object;
        private UISharedConfig m_SharedConfig;

        [TearDown]
        public void TearDown()
        {
            if (m_Object != null)
            {
                Object.Destroy(m_Object);
                m_Object = null;
            }
            if (m_SharedConfig != null)
            {
                Object.DestroyImmediate(m_SharedConfig);
                m_SharedConfig = null;
            }
        }

        [UnityTest]
        public IEnumerator Show_FadesCanvasGroupAlphaToOne()
        {
            (TestPanel panel, CanvasGroup canvasGroup) = BuildPanel();

            ((IUIPanel)panel).Show();

            yield return new WaitForSecondsRealtime(m_SharedConfig.PanelFadeInDuration + k_PostFadeBufferSeconds);

            Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
            Assert.IsTrue(m_Object.activeSelf);
        }

        [UnityTest]
        public IEnumerator Hide_DeactivatesGameObject()
        {
            (TestPanel panel, _) = BuildPanel();
            ((IUIPanel)panel).Show();
            yield return null;
            Assume.That(m_Object.activeSelf, Is.True);

            ((IUIPanel)panel).Hide();

            Assert.IsFalse(m_Object.activeSelf);
            yield break;
        }

        [UnityTest]
        public IEnumerator Show_TwiceInSuccession_KillsPreviousAndCompletesToOne()
        {
            (TestPanel panel, CanvasGroup canvasGroup) = BuildPanel();

            ((IUIPanel)panel).Show();
            yield return null;
            // Re-trigger mid-fade; PlayEntranceFade DOKill's the in-flight tween + restarts.
            ((IUIPanel)panel).Show();

            yield return new WaitForSecondsRealtime(m_SharedConfig.PanelFadeInDuration + k_PostFadeBufferSeconds);

            Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
        }

        private (TestPanel, CanvasGroup) BuildPanel()
        {
            m_Object = new GameObject(nameof(UIPanelBaseEntranceFadeTests));
            m_Object.SetActive(false);

            CanvasGroup canvasGroup = m_Object.AddComponent<CanvasGroup>();
            TestPanel panel = m_Object.AddComponent<TestPanel>();
            m_SharedConfig = ScriptableObject.CreateInstance<UISharedConfig>();

            ReflectionHelpers.SetPrivateField(panel, "m_CanvasGroup", canvasGroup);
            ReflectionHelpers.SetPrivateField(panel, "m_SharedConfig", m_SharedConfig);

            m_Object.SetActive(true);
            return (panel, canvasGroup);
        }

        private sealed class TestPanel : UIPanelBase
        {
        }
    }
}
