using System;
using System.Collections.Generic;
using Final.InfluencerMatch.Inputs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.UI
{
    /// <summary>
    /// Panel registry and navigation coordinator; tracks the current panel and which one is shown.
    /// </summary>
    public class UIManager : IInitializable, IDisposable
    {
        [Inject] private readonly IReadOnlyList<IUIPanel> m_RegisteredPanels;
        [Inject] private readonly IInputService m_InputService;

        private readonly Dictionary<Type, IUIPanel> m_Panels = new();
        private readonly NavigationHistory m_History = new();

        private Type m_CurrentPanel;

        void IInitializable.Initialize()
        {
            foreach (IUIPanel panel in m_RegisteredPanels)
            {
                if (panel == null)
                {
                    continue;
                }

                if (m_Panels.TryAdd(panel.GetType(), panel))
                {
                    panel.Hide();
                }
            }

            m_InputService.CancelRequested += OnCancelRequested;
        }

        void IDisposable.Dispose()
        {
            m_InputService.CancelRequested -= OnCancelRequested;
            m_Panels.Clear();
            m_History.Clear();
            m_CurrentPanel = null;
        }

        private void OnCancelRequested()
        {
            if (m_History.CanGoBack)
            {
                GoBack();
            }
        }

        public void Show<TView>() where TView : IUIPanel
        {
            Type viewType = typeof(TView);
            Type previous = m_CurrentPanel;

            if (SwitchTo(viewType) && previous != null && previous != viewType)
            {
                m_History.Push(previous);
            }
        }

        public void GoBack()
        {
            if (!m_History.TryPop(out Type previous))
            {
                Debug.LogWarning("UIManager: GoBack called with empty history.");
                return;
            }

            SwitchTo(previous);
        }

        private bool SwitchTo(Type viewType)
        {
            if (!m_Panels.TryGetValue(viewType, out IUIPanel target))
            {
                Debug.LogError($"UIManager: panel of type {viewType.Name} is not registered.");
                return false;
            }

            HideAllExcept(viewType);
            target.Show();
            m_CurrentPanel = viewType;
            return true;
        }

        private void HideAllExcept(Type viewType)
        {
            foreach (KeyValuePair<Type, IUIPanel> entry in m_Panels)
            {
                if (entry.Key != viewType)
                {
                    entry.Value.Hide();
                }
            }
        }
    }
}
