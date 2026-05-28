using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Final.Systems.DI
{
    /// <summary>
    /// App-wide scene transition utility exposing awaitable Addressables-backed scene loads.
    /// </summary>
    public static class SceneLoader
    {
        public static UniTask LoadAsync(string sceneAddress, CancellationToken cancellationToken = default)
        {
            return Addressables.LoadSceneAsync(sceneAddress).ToUniTask(cancellationToken: cancellationToken);
        }
    }
}
