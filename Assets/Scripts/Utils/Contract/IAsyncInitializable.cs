using System;
using Cysharp.Threading.Tasks;
using Model.Data.Scene;

namespace Utils.Contract
{
    public interface IAsyncInitializable
    {
        string ProcessName { get; }
        
        UniTask InitializeForScene(
            RuntimeSceneProfile sceneProfile, 
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep
        );
    }
}