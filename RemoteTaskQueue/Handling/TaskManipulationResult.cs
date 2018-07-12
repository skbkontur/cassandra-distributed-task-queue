namespace RemoteQueue.Handling
{
    public enum TaskManipulationResult
    {
        Success,
        Failure_LockAcquiringFails,
        Failure_InvalidTaskState,
        Failure_TaskDoesNotExist
    }
}