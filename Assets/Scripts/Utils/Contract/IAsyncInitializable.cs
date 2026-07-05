using System;
using Cysharp.Threading.Tasks;
using Utils.Data.Scene;

namespace Utils.Contract
{
    public interface IAsyncInitializable
    {
        string ProcessName { get; }
        
        UniTask InitializeForScene(
            SceneProfile sceneProfile, 
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep
        );
    }
}