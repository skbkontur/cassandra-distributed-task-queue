using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using SKBKontur.Catalogue.Core.Web.Blocks.ActionButton.Post;
using SKBKontur.Catalogue.Core.Web.Blocks.Button;
using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.Expressions;
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
            IRemoteTaskQueueHtmlModelCreator<TaskListModelData> htmlModelsCreator,
            ICatalogueExtender extender)
        {
            this.htmlModelsCreator = htmlModelsCreator;
            this.extender = extender;
            taskStates = new Dictionary<TaskState, string>
                {
                    {TaskState.Canceled, "Canceled"},
                    {TaskState.Fatal, "Fatal"},
                    {TaskState.Finished, "Finished"},
                    {TaskState.InProcess, "InProcess"},
                    {TaskState.New, "New"},
                    {TaskState.Unknown, "Unknown"},
                    {TaskState.WaitingForRerun, "WaitingForRerun"},
                    {TaskState.WaitingForRerunAfterError, "WaitingForRerunAfterError"}
                };
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
                    States = GetStatesGroup(pageModel, x => taskStates.ContainsKey(x.Key)),
                    TaskName = htmlModelsCreator.SelectBoxFor(pageModel, x => x.SearchPanel.TaskName, new SelectBoxOptions
                        {
                            Size = SelectBoxSize.Medium,
                            ReferenceConfig = new ReferenceConfig
                                {
                                    ReferenceType = "TaskNames",
                                    NeedEmptyValue = true,
                                    SelectBoxElements = (pageModel.Data.SearchPanel.AllowedTaskNames.Length == 0 ? new string[1] : pageModel.Data.SearchPanel.AllowedTaskNames).Select(x => new SelectBoxElement
                                        {
                                            Text = string.IsNullOrEmpty(x) ? "Ничего нет" : x,
                                            Value = x
                                        }).ToArray()
                                }
                        }),
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
                    Ticks = BulildDateTimeRangeHtmlMode(pageModel, x => x.SearchPanel.Ticks),
                    StartExecutedTicks = BulildDateTimeRangeHtmlMode(pageModel, x => x.SearchPanel.StartExecutedTicks),
                    FinishExecutedTicks = BulildDateTimeRangeHtmlMode(pageModel, x => x.SearchPanel.FinishExecutedTicks),
                    MinimalStartTicks = BulildDateTimeRangeHtmlMode(pageModel, x => x.SearchPanel.MinimalStartTicks)
                };
        }

        private KeyValuePair<TextBoxHtmlModel, CheckBoxHtmlModel>[] GetStatesGroup(TaskListPageModel pageModel, Func<Pair<TaskState, bool?>, bool> criterion)
        {
            return pageModel.Data.SearchPanel.States.Where(criterion).Select(
                (state, i) => new KeyValuePair<TextBoxHtmlModel, CheckBoxHtmlModel>(
                                  htmlModelsCreator.TextBoxFor(
                                      pageModel, x => x.SearchPanel.States[GetStateIndex(state.Key, pageModel.Data.SearchPanel.States)].Key, new TextBoxOptions
                                          {
                                              Hidden = true,
                                          }),
                                  htmlModelsCreator.CheckBoxFor(
                                      pageModel, x => x.SearchPanel.
                                                        States[GetStateIndex(state.Key, pageModel.Data.SearchPanel.States)].Value, new CheckBoxOptions
                                                            {
                                                                Label = taskStates[pageModel.Data.SearchPanel.States[GetStateIndex(state.Key, pageModel.Data.SearchPanel.States)].Key]
                                                            }))).ToArray();
        }

        private DateTimeRangeHtmlModel BulildDateTimeRangeHtmlMode(TaskListPageModel pageModel, Expression<Func<TaskListModelData, DateTimeRangeModel>> pathToDateTimeRange)
        {
            var options = new DateAndTimeOptions
                {
                    NeedTime = true,
                    TimeFormat = TimeFormat.Long
                };
            var from = htmlModelsCreator.DateAndTimeFor(pageModel, pathToDateTimeRange.Merge(dtr => dtr.From), options);
            from.Time.MaxLength = 8;
            var to = htmlModelsCreator.DateAndTimeFor(pageModel, pathToDateTimeRange.Merge(dtr => dtr.To), options);
            to.Time.MaxLength = 8;
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

        private readonly IRemoteTaskQueueHtmlModelCreator<TaskListModelData> htmlModelsCreator;
        private readonly ICatalogueExtender extender;
        private readonly Dictionary<TaskState, string> taskStates;
    }
}