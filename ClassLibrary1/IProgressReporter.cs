using System.Threading;

namespace ClassLibrary1
{
    public interface IProgressReporter
    {
        void UpdateProgress(string message, int progress);
        bool IsCancelled { get; }
    }
}