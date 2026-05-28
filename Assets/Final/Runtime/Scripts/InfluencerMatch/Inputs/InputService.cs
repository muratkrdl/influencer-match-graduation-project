using System;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Final.InfluencerMatch.Inputs
{
    /// <summary>
    /// Owns the generated input actions and translates them into high-level input events.
    /// </summary>
    public class InputService : IInputService, IInitializable, IDisposable
    {
        private InputSchema m_Actions;

        public event CancelRequestedHandler CancelRequested;

        void IInitializable.Initialize()
        {
            m_Actions = new InputSchema();
            m_Actions.UI.Enable();
            m_Actions.UI.Cancel.performed += OnCancelPerformed;
        }

        void IDisposable.Dispose()
        {
            m_Actions.UI.Cancel.performed -= OnCancelPerformed;
            m_Actions.Dispose();
            m_Actions = null;
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            CancelRequested?.Invoke();
        }
    }
}
