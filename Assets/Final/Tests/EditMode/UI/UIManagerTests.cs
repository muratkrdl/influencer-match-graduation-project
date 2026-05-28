using System;
using System.Collections.Generic;
using System.Reflection;
using Final.InfluencerMatch.Inputs;
using Final.InfluencerMatch.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;

namespace Final.Tests.UI
{
    [TestFixture]
    public sealed class UIManagerTests
    {
        private FakeInputService m_Input;

        [SetUp]
        public void SetUp()
        {
            m_Input = new FakeInputService();
        }

        [Test]
        public void Initialize_HidesAllRegisteredPanels()
        {
            PanelA a = new PanelA();
            PanelB b = new PanelB();
            BuildAndInitialize(a, b);

            Assert.IsFalse(a.Visible);
            Assert.IsFalse(b.Visible);
            Assert.GreaterOrEqual(a.HideCount, 1);
            Assert.GreaterOrEqual(b.HideCount, 1);
        }

        [Test]
        public void Show_ShowsTargetAndHidesOthers()
        {
            PanelA a = new PanelA();
            PanelB b = new PanelB();
            PanelC c = new PanelC();
            UIManager manager = BuildAndInitialize(a, b, c);

            manager.Show<PanelA>();

            Assert.IsTrue(a.Visible);
            Assert.IsFalse(b.Visible);
            Assert.IsFalse(c.Visible);
        }

        [Test]
        public void Show_ThenShowOther_GoBack_RestoresPrevious()
        {
            PanelA a = new PanelA();
            PanelB b = new PanelB();
            UIManager manager = BuildAndInitialize(a, b);

            manager.Show<PanelA>();
            manager.Show<PanelB>();
            manager.GoBack();

            Assert.IsTrue(a.Visible);
            Assert.IsFalse(b.Visible);
        }

        [Test]
        public void GoBack_OnEmptyHistory_LogsWarningAndDoesNotThrow()
        {
            PanelA a = new PanelA();
            UIManager manager = BuildAndInitialize(a);

            LogAssert.Expect(LogType.Warning, "UIManager: GoBack called with empty history.");
            manager.GoBack();
        }

        [Test]
        public void Show_SamePanelTwice_DoesNotRecordHistory()
        {
            PanelA a = new PanelA();
            PanelB b = new PanelB();
            UIManager manager = BuildAndInitialize(a, b);

            manager.Show<PanelA>();
            manager.Show<PanelA>();

            // History is empty (the second Show is a no-op for history), so GoBack warns.
            LogAssert.Expect(LogType.Warning, "UIManager: GoBack called with empty history.");
            manager.GoBack();
        }

        [Test]
        public void CancelRequested_WithHistory_GoesBack()
        {
            PanelA a = new PanelA();
            PanelB b = new PanelB();
            UIManager manager = BuildAndInitialize(a, b);

            manager.Show<PanelA>();
            manager.Show<PanelB>();
            m_Input.RaiseCancel();

            Assert.IsTrue(a.Visible);
            Assert.IsFalse(b.Visible);
        }

        [Test]
        public void CancelRequested_WithoutHistory_DoesNothing()
        {
            PanelA a = new PanelA();
            UIManager manager = BuildAndInitialize(a);

            manager.Show<PanelA>();
            m_Input.RaiseCancel();

            Assert.IsTrue(a.Visible);
        }

        private UIManager BuildAndInitialize(params IUIPanel[] panels)
        {
            UIManager manager = new UIManager();
            SetField(manager, "m_RegisteredPanels", new List<IUIPanel>(panels));
            SetField(manager, "m_InputService", m_Input);
            ((IInitializable)manager).Initialize();
            return manager;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field {fieldName} not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private class FakePanel : IUIPanel
        {
            private bool m_Visible;

            public bool Visible => m_Visible;
            public int ShowCount { get; private set; }
            public int HideCount { get; private set; }

            void IUIPanel.Show()
            {
                m_Visible = true;
                ShowCount++;
            }

            void IUIPanel.Hide()
            {
                m_Visible = false;
                HideCount++;
            }
        }

        // Distinct concrete types so UIManager's type-keyed registry and Show<TView> resolve them.
        private sealed class PanelA : FakePanel { }
        private sealed class PanelB : FakePanel { }
        private sealed class PanelC : FakePanel { }

        private sealed class FakeInputService : IInputService
        {
            public event CancelRequestedHandler CancelRequested;

            public void RaiseCancel()
            {
                CancelRequested?.Invoke();
            }
        }
    }
}
