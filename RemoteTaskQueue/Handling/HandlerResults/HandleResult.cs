using System;

namespace RemoteQueue.Handling.HandlerResults
{
    public class HandleResult
    {
        public FinishAction FinishAction { get; set; }
        public TimeSpan RerunDelay { get; set; }
        public Exception Error { get; set; }
    }
}