using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.Tests.Helpers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Final.Tests.Recommendation
{
    [TestFixture]
    public sealed class InfluencerDatabaseTests
    {
        private InfluencerDatabase m_Database;
        private List<InfluencerData> m_Influencers;

        [SetUp]
        public void SetUp()
        {
            m_Database = ScriptableObject.CreateInstance<InfluencerDatabase>();
            m_Influencers = new List<InfluencerData>();
        }

        [TearDown]
        public void TearDown()
        {
            TestDataFactory.DestroyAll(m_Influencers);
            if (m_Database != null)
            {
                Object.DestroyImmediate(m_Database);
                m_Database = null;
            }
        }

        [Test]
        public void TryFindById_EmptyId_ReturnsFalse()
        {
            AddInfluencer("a");
            AssignInfluencersList();

            bool found = m_Database.TryFindById(SerializableGuid.Empty, out InfluencerData result);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void TryFindById_KnownId_ReturnsInfluencer()
        {
            InfluencerData target = AddInfluencer("a");
            AddInfluencer("b");
            AssignInfluencersList();

            bool found = m_Database.TryFindById(target.Id, out InfluencerData result);

            Assert.IsTrue(found);
            Assert.AreSame(target, result);
        }

        [Test]
        public void TryFindById_UnknownId_ReturnsFalse()
        {
            AddInfluencer("a");
            AssignInfluencersList();
            SerializableGuid randomId = SerializableGuid.NewGuid();

            bool found = m_Database.TryFindById(randomId, out InfluencerData result);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        private InfluencerData AddInfluencer(string id)
        {
            InfluencerData data = TestDataFactory.CreateInfluencer(id, id, 1_000, 0.05f, 1_000, (CategoryId.Education, 3));
            m_Influencers.Add(data);
            return data;
        }

        private void AssignInfluencersList()
        {
            SerializedObject so = new SerializedObject(m_Database);
            SerializedProperty list = so.FindProperty("m_Influencers");
            list.arraySize = m_Influencers.Count;
            for (int i = 0; i < m_Influencers.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = m_Influencers[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

    }
}
