using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

namespace Final.Systems.DI
{
    /// <summary>
    /// Per-scope tracker of Addressables handles; loads + DI-registers assets in one call and releases every tracked handle when the owning DI scope disposes.
    /// </summary>
    public class AddressableHandleRegistry : IDisposable
    {
        private readonly List<AsyncOperationHandle> m_Handles = new List<AsyncOperationHandle>();

        public void LoadAndRegister<T>(IContainerBuilder builder, string address) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            T instance = handle.WaitForCompletion();
            builder.RegisterInstance(instance).AsSelf();
            Track(handle);
        }

        public void Track(AsyncOperationHandle handle)
        {
            m_Handles.Add(handle);
        }

        void IDisposable.Dispose()
        {
            for (int i = 0; i < m_Handles.Count; i++)
            {
                if (m_Handles[i].IsValid())
                {
                    Addressables.Release(m_Handles[i]);
                }
            }
            m_Handles.Clear();
        }
    }
}
