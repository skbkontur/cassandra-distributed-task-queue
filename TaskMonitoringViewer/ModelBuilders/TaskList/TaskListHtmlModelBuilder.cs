using System;
using System.Linq;
using System.Linq.Expressions;

using GrobExp.Mutators;

using SKBKontur.Catalogue.Core.Web.Blocks.ActionButton;
using SKBKontur.Catalogue.Core.Web.Blocks.ActionButton.Get;
using SKBKontur.Catalogue.Core.Web.Blocks.ActionButton.Post;
using SKBKontur.Catalogue.Core.Web.Blocks.PostUrls;
using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.ObjectManipulation.Extender;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList.SearchPanel;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskList
{
    public class TaskListHtmlModelBuilder : ITaskListHtmlModelBuilder
    {
        public TaskListHtmlModelBuilder(
            IRemoteTaskQueueHtmlModelsCreator<TaskListModelData> htmlModelsCreator,
            ICatalogueExtender extender)
        {
            this.htmlModelsCreator = htmlModelsCreator;
            this.extender = extender;
        }

        public TaskListHtmlModel Build(TaskListPageModel pageModel)
        {
            return new TaskListHtmlModel
                {
                    TaskCount = htmlModelsCreator.TextFor(pageModel, x => x.TaskCount),
                    SearchPanel = BuildSearchPanel(pageModel),
                    Tasks = pageModel.Data.TaskModels.Select(
                        (model, i) => htmlModelsCreator.TaskInfoFor(
                            pageModel,
                            x => x.TaskModels[i],
                            true,
                            () => new TaskIdHtmlModel(pageModel.PaginatorModelData.PageNumber, pageModel.PaginatorModelData.SearchRequestId))
                        ).ToArray()
                };
        }

        private SearchPanelHtmlModel BuildSearchPanel(TaskListPageModel pageModel)
        {
            pageModel.Data.SearchPanel = extender.Extend(pageModel.Data.SearchPanel);
            return new SearchPanelHtmlModel
                {
                    TaskStates = GetGroup(pageModel, x => x.SearchPanel.TaskStates),
                    TaskNames = GetGroup(pageModel, x => x.SearchPanel.TaskNames),
                    TaskId = htmlModelsCreator.TextBoxFor(pageModel, x => x.SearchPanel.TaskId, new TextBoxOptions
                        {
                            Size = TextBoxSize.Large
                        }),
                    ParentTaskId = htmlModelsCreator.TextBoxFor(pageModel, x => x.SearchPanel.ParentTaskId, new TextBoxOptions
                        {
                            Size = TextBoxSize.Large
                        }),
                    SearchButton = htmlModelsCreator.PostActionButtonFor(new PostUrl<TaskListModelData>(url => url.GetSearchUrl()),
                                                                         new PostActionButtonOptions
                                                                             {
                                                                                 Id = "Search",
                                                                                 Title = "Search",
                                                                                 ValidationType = ActionValidationType.All
                                                                             }),
                    Ticks = BuildDateTimeRangeHtmlModel(pageModel, x => x.SearchPanel.Ticks),
                    StartExecutedTicks = BuildDateTimeRangeHtmlModel(pageModel, x => x.SearchPanel.StartExecutedTicks),
                    FinishExecutedTicks = BuildDateTimeRangeHtmlModel(pageModel, x => x.SearchPanel.FinishExecutedTicks),
                    MinimalStartTicks = BuildDateTimeRangeHtmlModel(pageModel, x => x.SearchPanel.MinimalStartTicks)
                };
        }

        private CheckboxWithValue[] GetGroup<T>(TaskListPageModel pageModel, Expression<Func<TaskListModelData, Pair<T, bool?>[]>> path)
        {
            var func = path.Compile();
            return func(pageModel.Data).Select(
                (element, i) => new CheckboxWithValue
                    {
                        Value = htmlModelsCreator.TextBoxFor(
                            pageModel,
                            path.Merge(x => x[i].Key),
                            new TextBoxOptions
                                {
                                    Hidden = true,
                                }),
                        CheckBox = htmlModelsCreator.CheckBoxFor(
                            pageModel,
                            path.Merge(x => x[i].Value),
                            new CheckBoxOptions
                                {
                                    Label = element.Key.ToString()
                                })
                    }).ToArray();
        }

        private DateTimeRangeHtmlModel BuildDateTimeRangeHtmlModel(TaskListPageModel pageModel, Expression<Func<TaskListModelData, DateTimeRangeModel>> pathToDateTimeRange)
        {
            var options = new DateAndTimeOptions
                {
                    TimeFormat = TimeFormat.Long
                };
            var from = htmlModelsCreator.DateAndTimeFor(pageModel, pathToDateTimeRange.Merge(dtr => dtr.From), options);
            var to = htmlModelsCreator.DateAndTimeFor(pageModel, pathToDateTimeRange.Merge(dtr => dtr.To), options);
            return new DateTimeRangeHtmlModel
                {
                    From = from,
                    To = to
                };
        }

        private int GetStateIndex(TaskState state, Pair<TaskState, bool?>[] states)
        {
            for(var i = 0; i < states.Length; i++)
            {
                if(states[i].Key == state)
                    return i;
            }
            return -1;
        }

        private readonly IRemoteTaskQueueHtmlModelsCreator<TaskListModelData> htmlModelsCreator;
        private readonly ICatalogueExtender extender;
    }
}