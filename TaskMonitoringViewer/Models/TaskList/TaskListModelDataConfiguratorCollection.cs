using GroboContainer.Core;

using SKBKontur.Catalogue.Core.Web.Models.ModelConfigurations;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Mutators;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class TaskListModelDataConfiguratorCollection : WebDataConfiguratorCollection<TaskListModelData>
    {
        public TaskListModelDataConfiguratorCollection(IContainer container)
            : base(container)
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
        }
    }
}