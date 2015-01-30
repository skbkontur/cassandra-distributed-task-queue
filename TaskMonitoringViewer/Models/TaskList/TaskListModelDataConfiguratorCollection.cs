using GrobExp.Mutators;

using GroboContainer.Core;

using SKBKontur.Catalogue.Core.Web.Models.ModelConfigurations;
using SKBKontur.Catalogue.Core.Web.PageModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class TaskListModelDataConfiguratorCollection : WebDataConfiguratorCollection<TaskListModelData>
    {
        public TaskListModelDataConfiguratorCollection(IContainer container, IPathFormatterCollection pathFormatterCollection)
            : base(container, pathFormatterCollection)
        {
        }

        protected override void Configure(PageModelContext context, MutatorsConfigurator<TaskListModelData> configurator)
        {
            configurator.Target(data => data.SearchPanel.Ticks.From.Date).Date();
            configurator.Target(data => data.SearchPanel.StartExecutedTicks.From.Date).Date();
            configurator.Target(data => data.SearchPanel.MinimalStartTicks.From.Date).Date();
            configurator.Target(data => data.SearchPanel.Ticks.To.Date).Date();
            configurator.Target(data => data.SearchPanel.StartExecutedTicks.To.Date).Date();
            configurator.Target(data => data.SearchPanel.MinimalStartTicks.To.Date).Date();
            configurator.Target(x => x.RestartTime.Time).Time();
            configurator.Target(x => x.RestartTime.Date).Date();

        }
    }
}