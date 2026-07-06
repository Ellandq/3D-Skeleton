using System;
using Cysharp.Threading.Tasks;
using Model.Data.Model;

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