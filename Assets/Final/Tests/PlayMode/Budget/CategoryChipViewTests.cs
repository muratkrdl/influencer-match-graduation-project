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
    /// PlayMode tests for <see cref="CategoryChipView"/>: Bind populates label + icon,
    /// SetAlpha applies uniformly to all renderable graphics.
    /// </summary>
    public sealed class CategoryChipViewTests
    {
        private GameObject m_Object;
        private CategoryChipView m_Chip;
        private Image m_Background;
        private TMP_Text m_Label;
        private Image m_IconImage;

        [TearDown]
        public void TearDown()
        {
            if (m_Object != null)
            {
                Object.Destroy(m_Object);
                m_Object = null;
            }
        }

        [UnityTest]
        public IEnumerator Bind_SetsLabelTextAndIconSprite()
        {
            BuildChip();
            Sprite icon = NewDummySprite();
            CategoryDefinition def = ReflectionHelpers.CreateCategoryDefinition(CategoryId.Education, "Education", icon);

            m_Chip.Bind(def);
            yield return null;

            Assert.AreEqual("Education", m_Label.text);
            Assert.AreSame(icon, m_IconImage.sprite);
            Assert.IsTrue(m_IconImage.enabled);

            Object.DestroyImmediate(icon.texture);
            Object.DestroyImmediate(icon);
        }

        [UnityTest]
        public IEnumerator SetAlpha_AppliesToAllGraphics()
        {
            BuildChip();
            m_Chip.SetAlpha(0.42f);
            yield return null;

            Assert.AreEqual(0.42f, m_Background.color.a, 0.001f);
            Assert.AreEqual(0.42f, m_Label.color.a, 0.001f);
            Assert.AreEqual(0.42f, m_IconImage.color.a, 0.001f);
        }

        private void BuildChip()
        {
            m_Object = new GameObject(nameof(CategoryChipViewTests));
            m_Object.SetActive(false);

            m_Background = NewChildWithComponent<Image>("Background");
            m_Label = NewChildWithComponent<TextMeshProUGUI>("Label");
            m_IconImage = NewChildWithComponent<Image>("Icon");
            m_Chip = m_Object.AddComponent<CategoryChipView>();

            ReflectionHelpers.SetPrivateField(m_Chip, "m_Background", m_Background);
            ReflectionHelpers.SetPrivateField(m_Chip, "m_LabelText", m_Label);
            ReflectionHelpers.SetPrivateField(m_Chip, "m_IconImage", m_IconImage);

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
