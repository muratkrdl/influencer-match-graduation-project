using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using NUnit.Framework;
using UnityEditor;

namespace Final.Tests.Common
{
    /// <summary>
    /// Loads the production CategoryConfig asset and asserts data-integrity invariants.
    /// </summary>
    [TestFixture]
    public sealed class CategoryConfigValidationTests
    {
        private const string k_ConfigPath = "Assets/Final/Runtime/Data/Configs/CategoryConfig.asset";
        private const int k_ExpectedCount = 8;

        private CategoryConfig m_Config;

        [SetUp]
        public void SetUp()
        {
            m_Config = AssetDatabase.LoadAssetAtPath<CategoryConfig>(k_ConfigPath);
            Assert.IsNotNull(m_Config, "CategoryConfig asset not found at " + k_ConfigPath);
        }

        [Test]
        public void Config_HasExpectedCategoryCount()
        {
            Assert.AreEqual(k_ExpectedCount, m_Config.Categories.Count);
        }

        [Test]
        public void Config_AllIdsAreNonNoneAndUnique()
        {
            HashSet<CategoryId> seen = new HashSet<CategoryId>();
            for (int i = 0; i < m_Config.Categories.Count; i++)
            {
                CategoryDefinition def = m_Config.Categories[i];
                Assert.AreNotEqual(CategoryId.None, def.Id, "Category at index " + i + " has Id=None.");
                Assert.IsTrue(seen.Add(def.Id), "Duplicate CategoryId '" + def.Id + "' at index " + i + ".");
            }
        }

        [Test]
        public void Config_AllCategoriesHaveDisplayName()
        {
            for (int i = 0; i < m_Config.Categories.Count; i++)
            {
                CategoryDefinition def = m_Config.Categories[i];
                Assert.IsFalse(string.IsNullOrEmpty(def.DisplayName), "Category " + def.Id + " has empty DisplayName.");
            }
        }

        [Test]
        public void Config_AllCategoriesHaveIcon()
        {
            for (int i = 0; i < m_Config.Categories.Count; i++)
            {
                CategoryDefinition def = m_Config.Categories[i];
                Assert.IsNotNull(def.Icon, "Category " + def.Id + " has null Icon.");
            }
        }
    }
}
