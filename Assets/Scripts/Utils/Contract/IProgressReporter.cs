using System;

namespace Utils.Contract
{
    public interface IProgressReporter
    {
        event Action<float> OnProgress;
        event Action<string> OnStepChanged;
    }
}