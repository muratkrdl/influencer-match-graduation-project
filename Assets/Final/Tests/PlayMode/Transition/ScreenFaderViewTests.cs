using System.Collections;
using System.Reflection;
using Final.InfluencerMatch.Transition;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Final.Tests.PlayMode.Transition
{
    /// <summary>
    /// PlayMode tests for <see cref="ScreenFaderView"/>.
    /// </summary>
    public sealed class ScreenFaderViewTests
    {
        private const float k_PostFadeBufferSeconds = 0.1f;

        private GameObject m_Object;
        private ScreenFaderConfig m_Config;

        [TearDown]
        public void TearDown()
        {
            if (m_Object != null)
            {
                Object.Destroy(m_Object);
                m_Object = null;
            }
            if (m_Config != null)
            {
                Object.DestroyImmediate(m_Config);
                m_Config = null;
            }
        }

        [UnityTest]
        public IEnumerator FadeOut_RaisesAlphaToOne_AndInvokesCallback()
        {
            BuildFader();
            CanvasGroup canvasGroup = m_Object.GetComponent<CanvasGroup>();
            IScreenFader fader = m_Object.GetComponent<ScreenFaderView>();

            bool callbackFired = false;
            fader.FadeOut(() => callbackFired = true);

            yield return new WaitForSecondsRealtime(m_Config.FadeOutDuration + k_PostFadeBufferSeconds);

            Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
            Assert.IsTrue(canvasGroup.blocksRaycasts);
            Assert.IsTrue(callbackFired);
        }

        [UnityTest]
        public IEnumerator FadeIn_DropsAlphaToZero_ClearsBlocksRaycasts_AndInvokesCallback()
        {
            BuildFader();
            CanvasGroup canvasGroup = m_Object.GetComponent<CanvasGroup>();
            IScreenFader fader = m_Object.GetComponent<ScreenFaderView>();

            // Bring the overlay fully opaque + blocking first so FadeIn has work to do.
            fader.FadeOut();
            yield return new WaitForSecondsRealtime(m_Config.FadeOutDuration + k_PostFadeBufferSeconds);
            Assume.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
            Assume.That(canvasGroup.blocksRaycasts, Is.True);

            bool callbackFired = false;
            fader.FadeIn(() => callbackFired = true);

            yield return new WaitForSecondsRealtime(m_Config.FadeInDuration + k_PostFadeBufferSeconds);

            Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.001f));
            Assert.IsFalse(canvasGroup.blocksRaycasts);
            Assert.IsTrue(callbackFired);
        }

        [UnityTest]
        public IEnumerator FadeOut_NullCallback_DoesNotThrow()
        {
            BuildFader();
            IScreenFader fader = m_Object.GetComponent<ScreenFaderView>();

            Assert.DoesNotThrow(() => fader.FadeOut(null));

            yield return new WaitForSecondsRealtime(m_Config.FadeOutDuration + k_PostFadeBufferSeconds);
        }

        private void BuildFader()
        {
            // Create inactive so Awake fires AFTER we wire the serialized fields via reflection.
            m_Object = new GameObject(nameof(ScreenFaderViewTests));
            m_Object.SetActive(false);

            CanvasGroup canvasGroup = m_Object.AddComponent<CanvasGroup>();
            ScreenFaderView fader = m_Object.AddComponent<ScreenFaderView>();

            m_Config = ScriptableObject.CreateInstance<ScreenFaderConfig>();

            SetPrivateField(fader, "m_CanvasGroup", canvasGroup);
            SetPrivateField(fader, "m_Config", m_Config);

            m_Object.SetActive(true);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, fieldName + " not found on " + target.GetType().Name + ".");
            field.SetValue(target, value);
        }
    }
}
