using System;
using Final.InfluencerMatch.UI;
using NUnit.Framework;

namespace Final.Tests.UI
{
    [TestFixture]
    public sealed class NavigationHistoryTests
    {
        [Test]
        public void Initial_CanGoBack_IsFalse()
        {
            NavigationHistory history = new NavigationHistory();
            Assert.IsFalse(history.CanGoBack);
        }

        [Test]
        public void Push_MakesCanGoBackTrue()
        {
            NavigationHistory history = new NavigationHistory();
            history.Push(typeof(int));
            Assert.IsTrue(history.CanGoBack);
        }

        [Test]
        public void TryPop_OnEmpty_ReturnsFalseAndNull()
        {
            NavigationHistory history = new NavigationHistory();

            bool popped = history.TryPop(out Type result);

            Assert.IsFalse(popped);
            Assert.IsNull(result);
        }

        [Test]
        public void TryPop_ReturnsMostRecentlyPushed_LifoOrder()
        {
            NavigationHistory history = new NavigationHistory();
            history.Push(typeof(int));
            history.Push(typeof(string));

            Assert.IsTrue(history.TryPop(out Type first));
            Assert.AreEqual(typeof(string), first);

            Assert.IsTrue(history.TryPop(out Type second));
            Assert.AreEqual(typeof(int), second);

            Assert.IsFalse(history.CanGoBack);
        }

        [Test]
        public void Clear_EmptiesTheStack()
        {
            NavigationHistory history = new NavigationHistory();
            history.Push(typeof(int));
            history.Push(typeof(string));

            history.Clear();

            Assert.IsFalse(history.CanGoBack);
            Assert.IsFalse(history.TryPop(out _));
        }
    }
}
