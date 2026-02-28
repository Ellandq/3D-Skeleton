using System;
using System.Threading.Tasks;
using Utils.SO;

namespace Utils.Contract
{
    public interface IAsyncInitializable
    {
        Task InitializeForScene(
            SceneProfile sceneProfile, 
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep
        );
    }
}