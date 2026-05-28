using Final.InfluencerMatch.Common;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Final.Tests.Common
{
    [TestFixture]
    public sealed class CategoryConfigTests
    {
        private CategoryConfig m_Config;

        [SetUp]
        public void SetUp()
        {
            m_Config = ScriptableObject.CreateInstance<CategoryConfig>();
            AddDefinition(CategoryId.Education, "Education");
            AddDefinition(CategoryId.Sports, "Sports");
        }

        [TearDown]
        public void TearDown()
        {
            if (m_Config != null)
            {
                Object.DestroyImmediate(m_Config);
                m_Config = null;
            }
        }

        [Test]
        public void TryGetDefinition_KnownId_ReturnsDefinition()
        {
            bool found = m_Config.TryGetDefinition(CategoryId.Education, out CategoryDefinition def);

            Assert.IsTrue(found);
            Assert.IsNotNull(def);
            Assert.AreEqual(CategoryId.Education, def.Id);
            Assert.AreEqual("Education", def.DisplayName);
        }

        [Test]
        public void TryGetDefinition_None_ReturnsFalse()
        {
            bool found = m_Config.TryGetDefinition(CategoryId.None, out CategoryDefinition def);

            Assert.IsFalse(found);
            Assert.IsNull(def);
        }

        [Test]
        public void TryGetDefinition_UnknownId_ReturnsFalse()
        {
            // Fashion not added in SetUp.
            bool found = m_Config.TryGetDefinition(CategoryId.Fashion, out CategoryDefinition def);

            Assert.IsFalse(found);
            Assert.IsNull(def);
        }

        [Test]
        public void Categories_PreservesAuthoredOrder()
        {
            Assert.AreEqual(2, m_Config.Categories.Count);
            Assert.AreEqual(CategoryId.Education, m_Config.Categories[0].Id);
            Assert.AreEqual(CategoryId.Sports, m_Config.Categories[1].Id);
        }

        private void AddDefinition(CategoryId id, string displayName)
        {
            SerializedObject so = new SerializedObject(m_Config);
            SerializedProperty list = so.FindProperty("m_Categories");
            int index = list.arraySize;
            list.arraySize = index + 1;
            SerializedProperty element = list.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("m_Id").enumValueIndex = (int)id;
            element.FindPropertyRelative("m_DisplayName").stringValue = displayName;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
