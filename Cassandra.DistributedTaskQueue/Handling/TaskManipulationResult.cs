using System.Diagnostics.CodeAnalysis;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum TaskManipulationResult
    {
        Success,
        Failure_LockAcquiringFails,
        Failure_InvalidTaskState,
        Failure_TaskDoesNotExist
    }
}