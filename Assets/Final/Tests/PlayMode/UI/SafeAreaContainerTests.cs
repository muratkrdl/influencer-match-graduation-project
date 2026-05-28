using System.Collections;
using Final.InfluencerMatch.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Final.Tests.PlayMode.UI
{
    /// <summary>
    /// PlayMode tests for <see cref="SafeAreaContainer"/>.
    /// </summary>
    public sealed class SafeAreaContainerTests
    {
        private GameObject m_Object;

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
        public IEnumerator OnEnable_AppliesNormalizedSafeAreaToAnchors()
        {
            m_Object = new GameObject(nameof(SafeAreaContainerTests), typeof(RectTransform), typeof(SafeAreaContainer));
            // Wait a frame so Awake + OnEnable have fired and ApplySafeArea has set the anchors.
            yield return null;

            Rect safeArea = Screen.safeArea;
            int sw = Screen.width;
            int sh = Screen.height;

            Assume.That(sw, Is.GreaterThan(0), "Screen.width must be positive in PlayMode.");
            Assume.That(sh, Is.GreaterThan(0), "Screen.height must be positive in PlayMode.");

            Vector2 expectedMin = new Vector2(safeArea.x / sw, safeArea.y / sh);
            Vector2 expectedMax = new Vector2((safeArea.x + safeArea.width) / sw, (safeArea.y + safeArea.height) / sh);

            RectTransform rect = m_Object.GetComponent<RectTransform>();
            Assert.That(rect.anchorMin.x, Is.EqualTo(expectedMin.x).Within(0.001f));
            Assert.That(rect.anchorMin.y, Is.EqualTo(expectedMin.y).Within(0.001f));
            Assert.That(rect.anchorMax.x, Is.EqualTo(expectedMax.x).Within(0.001f));
            Assert.That(rect.anchorMax.y, Is.EqualTo(expectedMax.y).Within(0.001f));
            Assert.AreEqual(Vector2.zero, rect.offsetMin);
            Assert.AreEqual(Vector2.zero, rect.offsetMax);
        }
    }
}
