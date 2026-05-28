using System.Collections;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.Tests.PlayMode.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Final.Tests.PlayMode.Recommendation
{
    /// <summary>
    /// PlayMode tests for <see cref="InfluencerCardView"/>'s tap → tween → CardClicked path.
    /// Sets the minimum SerializeField / [Inject] subset needed by Awake (m_ClickButton,
    /// m_NameText, m_EmailText, m_Config) and skips Bind, setting m_InfluencerId directly.
    /// </summary>
    public sealed class InfluencerCardViewTapTests
    {
        private const float k_PostTapBufferSeconds = 0.2f;

        private GameObject m_Object;
        private RecommendationConfig m_Config;
        private Button m_ClickButton;

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
        public IEnumerator Tap_AfterTweenCompletes_RaisesCardClickedWithCapturedId()
        {
            InfluencerCardView card = BuildCard();
            SerializableGuid expectedId = SerializableGuid.NewGuid();
            ReflectionHelpers.SetPrivateField(card, "m_InfluencerId", expectedId);

            SerializableGuid? capturedId = null;
            card.CardClicked += id => capturedId = id;

            m_ClickButton.onClick.Invoke();

            yield return new WaitForSecondsRealtime(m_Config.TapPunchDuration + k_PostTapBufferSeconds);

            Assert.IsTrue(capturedId.HasValue, "CardClicked should have fired after the tap tween completes.");
            Assert.AreEqual(expectedId, capturedId.Value);
        }

        [UnityTest]
        public IEnumerator Tap_DoubleInvokeBeforeTweenCompletes_RaisesCardClickedOnce()
        {
            InfluencerCardView card = BuildCard();
            ReflectionHelpers.SetPrivateField(card, "m_InfluencerId", SerializableGuid.NewGuid());

            int fireCount = 0;
            card.CardClicked += _ => fireCount++;

            m_ClickButton.onClick.Invoke();
            // Second invoke during the in-flight tween is guarded by `m_TapTween.IsActive()`.
            m_ClickButton.onClick.Invoke();

            yield return new WaitForSecondsRealtime(m_Config.TapPunchDuration + k_PostTapBufferSeconds);

            Assert.AreEqual(1, fireCount);
        }

        private InfluencerCardView BuildCard()
        {
            m_Object = new GameObject(nameof(InfluencerCardViewTapTests));
            m_Object.SetActive(false);

            m_ClickButton = NewChildWithComponent<Button>("ClickButton");
            TMP_Text nameText = NewChildWithComponent<TextMeshProUGUI>("Name");
            TMP_Text emailText = NewChildWithComponent<TextMeshProUGUI>("Email");

            InfluencerCardView card = m_Object.AddComponent<InfluencerCardView>();
            m_Config = ScriptableObject.CreateInstance<RecommendationConfig>();

            ReflectionHelpers.SetPrivateField(card, "m_ClickButton", m_ClickButton);
            ReflectionHelpers.SetPrivateField(card, "m_NameText", nameText);
            ReflectionHelpers.SetPrivateField(card, "m_EmailText", emailText);
            ReflectionHelpers.SetPrivateField(card, "m_Config", m_Config);

            m_Object.SetActive(true);
            return card;
        }

        private T NewChildWithComponent<T>(string childName) where T : Component
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(m_Object.transform);
            return child.AddComponent<T>();
        }
    }
}
