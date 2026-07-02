using System;
using System.Threading.Tasks;
using Utils.SO;
using Utils.SO.Scene;

namespace Utils.Contract
{
    public interface IAsyncInitializable
    {
        string ProcessName { get; }
        
        Task InitializeForScene(
            SceneProfile sceneProfile, 
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep
        );
    }
}