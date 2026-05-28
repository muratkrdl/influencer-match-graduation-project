using System;
using System.Collections.Generic;

namespace Final.InfluencerMatch.UI
{
    /// <summary>
    /// Back-stack of panel view types for navigation history.
    /// </summary>
    public class NavigationHistory
    {
        private readonly Stack<Type> m_Stack = new Stack<Type>();

        public bool CanGoBack => m_Stack.Count > 0;

        public void Push(Type viewType)
        {
            m_Stack.Push(viewType);
        }

        public bool TryPop(out Type viewType)
        {
            if (m_Stack.Count == 0)
            {
                viewType = null;
                return false;
            }

            viewType = m_Stack.Pop();
            return true;
        }

        public void Clear()
        {
            m_Stack.Clear();
        }
    }
}
