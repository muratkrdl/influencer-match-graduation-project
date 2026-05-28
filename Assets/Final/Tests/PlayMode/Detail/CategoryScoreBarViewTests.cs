using System.Collections;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Detail;
using Final.Tests.PlayMode.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Final.Tests.PlayMode.Detail
{
    /// <summary>
    /// PlayMode tests for <see cref="CategoryScoreBarView"/>: Bind paints dots based on score,
    /// applies bold label + highlight when the user has selected the category.
    /// </summary>
    public sealed class CategoryScoreBarViewTests
    {
        private GameObject m_Object;
        private CategoryScoreBarView m_Bar;
        private ScoreBarConfig m_Config;
        private Image m_IconImage;
        private TMP_Text m_Label;
        private Image[] m_Dots;
        private Image m_Highlight;

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
        public IEnumerator Bind_PaintsFilledDotsProportionalToScore()
        {
            BuildBar();
            CategoryDefinition def = ReflectionHelpers.CreateCategoryDefinition(CategoryId.Education, "Education");

            // dotsPerPoint = DotCount / MaxScore = 10 / 5 = 2. Score 3 -> 6 filled dots.
            m_Bar.Bind(def, influencerScore: 3, isSelectedByUser: false);
            yield return null;

            int expectedFilled = 6;
            for (int i = 0; i < expectedFilled; i++)
            {
                Assert.AreEqual(m_Config.FilledColor, m_Dots[i].color, "Dot " + i + " should be filled.");
            }
            for (int i = expectedFilled; i < m_Dots.Length; i++)
            {
                Assert.AreEqual(m_Config.EmptyColor, m_Dots[i].color, "Dot " + i + " should be empty.");
            }
        }

        [UnityTest]
        public IEnumerator Bind_MaxScore_FillsAllDots()
        {
            BuildBar();
            CategoryDefinition def = ReflectionHelpers.CreateCategoryDefinition(CategoryId.Sports, "Sports");

            m_Bar.Bind(def, influencerScore: 5, isSelectedByUser: false);
            yield return null;

            for (int i = 0; i < m_Dots.Length; i++)
            {
                Assert.AreEqual(m_Config.FilledColor, m_Dots[i].color, "Dot " + i + " should be filled.");
            }
        }

        [UnityTest]
        public IEnumerator Bind_ZeroScore_LeavesAllDotsEmpty()
        {
            BuildBar();
            CategoryDefinition def = ReflectionHelpers.CreateCategoryDefinition(CategoryId.Food, "Food");

            m_Bar.Bind(def, influencerScore: 0, isSelectedByUser: false);
            yield return null;

            for (int i = 0; i < m_Dots.Length; i++)
            {
                Assert.AreEqual(m_Config.EmptyColor, m_Dots[i].color, "Dot " + i + " should be empty.");
            }
        }

        [UnityTest]
        public IEnumerator Bind_SelectedByUser_AppliesBoldLabelAndShowsHighlight()
        {
            BuildBar();
            CategoryDefinition def = ReflectionHelpers.CreateCategoryDefinition(CategoryId.Technology, "Technology");

            m_Bar.Bind(def, influencerScore: 3, isSelectedByUser: true);
            yield return null;

            Assert.AreEqual(FontStyles.Bold, m_Label.fontStyle);
            Assert.IsTrue(m_Highlight.enabled);
            Assert.AreEqual(m_Config.HighlightColor, m_Highlight.color);
        }

        [UnityTest]
        public IEnumerator Bind_NotSelectedByUser_AppliesNormalLabelAndHidesHighlight()
        {
            BuildBar();
            CategoryDefinition def = ReflectionHelpers.CreateCategoryDefinition(CategoryId.Travel, "Travel");

            m_Bar.Bind(def, influencerScore: 3, isSelectedByUser: false);
            yield return null;

            Assert.AreEqual(FontStyles.Normal, m_Label.fontStyle);
            Assert.IsFalse(m_Highlight.enabled);
        }

        private void BuildBar()
        {
            m_Object = new GameObject(nameof(CategoryScoreBarViewTests));
            m_Object.SetActive(false);

            m_IconImage = NewChildWithComponent<Image>("Icon");
            m_Label = NewChildWithComponent<TextMeshProUGUI>("Label");
            m_Highlight = NewChildWithComponent<Image>("Highlight");

            m_Config = ScriptableObject.CreateInstance<ScoreBarConfig>();
            m_Dots = new Image[m_Config.DotCount];
            for (int i = 0; i < m_Dots.Length; i++)
            {
                m_Dots[i] = NewChildWithComponent<Image>("Dot_" + i);
            }

            m_Bar = m_Object.AddComponent<CategoryScoreBarView>();
            ReflectionHelpers.SetPrivateField(m_Bar, "m_IconImage", m_IconImage);
            ReflectionHelpers.SetPrivateField(m_Bar, "m_LabelText", m_Label);
            ReflectionHelpers.SetPrivateField(m_Bar, "m_ScoreDots", m_Dots);
            ReflectionHelpers.SetPrivateField(m_Bar, "m_HighlightBackground", m_Highlight);
            ReflectionHelpers.SetPrivateField(m_Bar, "m_Config", m_Config);

            m_Object.SetActive(true);
        }

        private T NewChildWithComponent<T>(string childName) where T : Component
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(m_Object.transform);
            return child.AddComponent<T>();
        }
    }
}
