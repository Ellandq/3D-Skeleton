using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utils.Contract;
using Utils.SO;

namespace SaveAndLoad
{
    public class LoadQueue : IProgressReporter
    {
        public event Action<float> OnProgress;
        public event Action<string> OnStepChanged;

        private readonly Action _onFinishLoad;
        
        private readonly Queue<IAsyncInitializable> _steps = new();
        private SceneProfile _sceneProfile;
        
        private float _progress;
        
        private readonly int _stepCount;
        private int _currentStepSubprocessCount;
        private int _currentSubProcessStepsCount;

        private string _processName;

        private int _currentSubProcess;
        private int _currentSubProcessStep;
        
        private float _stepProgress;
        private float _subProcessProgress;
        private float _singleStepProgress;

        public LoadQueue(Action onFinishLoad, params IAsyncInitializable[] steps)
        {
            _onFinishLoad = onFinishLoad;
            foreach (var step in steps)
                _steps.Enqueue(step);
            _stepCount = _steps.Count;
        }
        
        public async Task StartLoad(SceneProfile profile)
        {
            if (_steps.Count == 0)
            {
                Finish();
                return;
            }

            _sceneProfile = profile;
            _progress = 0f;
            _stepProgress = 1f / Math.Max(1, _steps.Count);

            await GoNext();
        }

        private async Task GoNext()
        {
            while (_steps.Count > 0)
            {
                var loadAction = _steps.Dequeue();
                _processName = loadAction.ProcessName;

                _currentSubProcess = 0;
                _currentSubProcessStep = 0;

                try
                {
                    await loadAction.InitializeForScene(_sceneProfile,
                        DeclareSubProcesses,
                        DeclareSubProcessSteps,
                        DeclareStep
                    );
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error during load step {_processName}: {ex}");
                }
            }
            
            Finish();
        }

        private void DeclareSubProcesses(int subProcessCount)
        {
            _currentStepSubprocessCount = Math.Max(1, subProcessCount);
            _subProcessProgress = _stepProgress / _currentStepSubprocessCount;
        }

        private void DeclareSubProcessSteps(int subProcessStepCount)
        {
            _currentSubProcess++;
            _currentSubProcessStepsCount = Math.Max(1, subProcessStepCount);
            _currentSubProcessStep = 0;

            _singleStepProgress = _subProcessProgress / _currentSubProcessStepsCount;
        }

        private void DeclareStep(string message)
        {
            _currentSubProcessStep++;

            _progress += _singleStepProgress;
            _progress = Math.Clamp(_progress, 0f, 1f);

            OnStepChanged?.Invoke($"{_processName} - {message}");
            OnProgress?.Invoke(_progress);
        }

        private void Finish()
        {
            _progress = 1f;
            OnProgress?.Invoke(_progress);
            _onFinishLoad?.Invoke();
        }
    }
}