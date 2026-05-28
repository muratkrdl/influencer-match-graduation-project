using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Final.Systems.DI
{
    internal static class DIBootStrapper
    {
        private const string k_BootScopeResourcePath = "DI/[BootScope]";

        private static bool s_Initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (s_Initialized)
            {
                return;
            }

            BootLifetimeScope scopeResource = Resources.Load<BootLifetimeScope>(k_BootScopeResourcePath);
            if (scopeResource == null)
            {
                throw new NullReferenceException($"DIBootStrapper: BootLifetimeScope prefab not found at 'Resources/{k_BootScopeResourcePath}'. Run Tools > Influencer Match > Setup All Scenes to materialise it.");
            }

            BootLifetimeScope scopeInstance = Object.Instantiate(scopeResource);
            GameObject scopeGo = scopeInstance.gameObject;
            scopeGo.name = $"[{nameof(BootLifetimeScope)}]";
            scopeGo.hideFlags = HideFlags.NotEditable;

            Object.DontDestroyOnLoad(scopeGo);
            s_Initialized = true;
        }
    }
}
