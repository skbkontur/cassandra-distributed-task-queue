using SKBKontur.Catalogue.WebTestCore.SystemControls;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.Controls
{
    public class TaskListItem : HtmlControl
    {
        public TaskListItem(int index)
            : base(string.Format("Task[{0}]", index))
        {
            TaskId = new Link(string.Format("TaskId[{0}]", index));
            TaskState = new StaticText(string.Format("TaskState[{0}]", index));
            TaskName = new StaticText(string.Format("TaskName[{0}]", index));
            EnqueueTicks = new StaticText(string.Format("EnqueueTicks[{0}]", index));
            StartExecutedTicks = new StaticText(string.Format("StartExecutedTicks[{0}]", index));
            MinimalStartTicks = new StaticText(string.Format("MinimalStartTicks[{0}]", index));
            Attempts = new StaticText(string.Format("Attempts[{0}]", index));
            ParentTaskId = new Link(string.Format("ParentTaskId[{0}]", index));
        }

        public Link TaskId { get; private set; }
        public StaticText TaskState { get; private set; }
        public StaticText TaskName { get; private set; }
        public StaticText EnqueueTicks { get; private set; }
        public StaticText StartExecutedTicks { get; private set; }
        public StaticText MinimalStartTicks { get; private set; }
        public StaticText Attempts { get; private set; }
        public Link ParentTaskId { get; private set; }
    }
}