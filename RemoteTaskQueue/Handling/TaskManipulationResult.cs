namespace RemoteQueue.Handling
{
    public enum TaskManipulationResult
    {
        Success,
        Unsuccess_LockAcquiringFails,
        Unsuccess_InvalidTaskState,
        Unsuccess_TaskDoesNotExist
    }
}