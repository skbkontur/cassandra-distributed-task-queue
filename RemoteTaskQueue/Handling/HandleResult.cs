using System;

namespace RemoteQueue.Handling
{
    public class HandleResult
    {
        public FinishAction FinishAction { get; set; }
        public TimeSpan RerunDelay { get; set; }
        public Exception Error { get; set; }
    }
}