using System.Collections;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using Final.Tests.PlayMode.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Final.Tests.PlayMode.Budget
{
    /// <summary>
    /// PlayMode tests for <see cref="CategoryToggleView"/>: Bind populates label / icon /
    /// accent, the underlying Toggle's onValueChanged routes to ToggleChanged with the
    /// bound CategoryId, and SetIsOn programmatic flips do NOT raise the event
    /// (state-restore must not echo back to the controller).
    /// </summary>
    public sealed class CategoryToggleViewTests
    {
        private GameObject m_Object;
        private CategoryToggleView m_View;
        private Toggle m_Toggle;
        private TMP_Text m_LabelText;
        private Image m_AccentImage;
        private Image m_IconImage;
        private BudgetConfig m_Config;

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
        public IEnumerator Bind_PopulatesLabelAndIcon()
        {
            BuildToggle();
            Sprite icon = NewDummySprite();
            CategoryDefinition def = ReflectionHelpers.CreateCategoryDefinition(CategoryId.Education, "Education", icon, Color.red);

            m_View.Bind(def);
            yield return null;

            Assert.AreEqual("Education", m_LabelText.text);
            Assert.AreSame(icon, m_IconImage.sprite);
            Assert.IsTrue(m_IconImage.enabled);
            Assert.AreEqual(Color.red, m_AccentImage.color);
            Assert.IsFalse(m_Toggle.isOn, "Bind should reset toggle to off without raising the event.");
            Assert.AreEqual(CategoryId.Education, m_View.CategoryId);

            Object.DestroyImmediate(icon.texture);
            Object.DestroyImmediate(icon);
        }

        [UnityTest]
        public IEnumerator UserToggle_FiresToggleChanged_WithBoundCategoryId()
        {
            BuildToggle();
            CategoryDefinition def = ReflectionHelpers.CreateCategoryDefinition(CategoryId.Sports, "Sports");
            m_View.Bind(def);

            CategoryId? capturedId = null;
            bool? capturedIsOn = null;
            m_View.ToggleChanged += (id, isOn) =>
            {
                capturedId = id;
                capturedIsOn = isOn;
            };

            m_Toggle.isOn = true;
            yield return null;

            Assert.IsTrue(capturedId.HasValue);
            Assert.AreEqual(CategoryId.Sports, capturedId.Value);
            Assert.IsTrue(capturedIsOn.HasValue);
            Assert.IsTrue(capturedIsOn.Value);
        }

        [UnityTest]
        public IEnumerator SetIsOn_DoesNotRaiseToggleChanged()
        {
            BuildToggle();
            CategoryDefinition def = ReflectionHelpers.CreateCategoryDefinition(CategoryId.Fashion, "Fashion");
            m_View.Bind(def);

            bool eventFired = false;
            m_View.ToggleChanged += (_, __) => eventFired = true;

            m_View.SetIsOn(true);
            yield return null;

            Assert.IsTrue(m_Toggle.isOn, "Toggle state should reflect programmatic SetIsOn.");
            Assert.IsFalse(eventFired, "SetIsOn must use SetIsOnWithoutNotify to avoid event echo.");
        }

        private void BuildToggle()
        {
            m_Object = new GameObject(nameof(CategoryToggleViewTests));
            m_Object.SetActive(false);

            m_Toggle = m_Object.AddComponent<Toggle>();
            m_LabelText = NewChildWithComponent<TextMeshProUGUI>("Label");
            m_AccentImage = NewChildWithComponent<Image>("Accent");
            m_IconImage = NewChildWithComponent<Image>("Icon");

            m_Config = ScriptableObject.CreateInstance<BudgetConfig>();

            m_View = m_Object.AddComponent<CategoryToggleView>();
            ReflectionHelpers.SetPrivateField(m_View, "m_Toggle", m_Toggle);
            ReflectionHelpers.SetPrivateField(m_View, "m_LabelText", m_LabelText);
            ReflectionHelpers.SetPrivateField(m_View, "m_AccentImage", m_AccentImage);
            ReflectionHelpers.SetPrivateField(m_View, "m_IconImage", m_IconImage);
            ReflectionHelpers.SetPrivateField(m_View, "m_Config", m_Config);

            m_Object.SetActive(true);
        }

        private T NewChildWithComponent<T>(string childName) where T : Component
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(m_Object.transform);
            return child.AddComponent<T>();
        }

        private static Sprite NewDummySprite()
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f));
        }
    }
}
