using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using SKBKontur.Catalogue.Expressions;

using SKBKontur.Catalogue.Core.Web.Blocks.Button;
using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Html;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.Html
{
    internal class RemoteTaskQueueHtmlModelBuilder : IRemoteTaskQueueHtmlModelBuilder
    {
        public RemoteTaskQueueHtmlModelBuilder(IHtmlModelsCreator<RemoteTaskQueuePageModel> htmlModelsCreator)
        {
            this.htmlModelsCreator = htmlModelsCreator;
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

        public SearchPanelHtmlModel Build(RemoteTaskQueueModel pageModel)
        {
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
                                    SelectBoxElements = pageModel.Data.SearchPanel.AllowedTaskNames.Select(x => new SelectBoxElement
                                        {
                                            Text = x,
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
                    SearchButton = htmlModelsCreator.ButtonFor(new ButtonOptions
                        {
                            Action = "Search",
                            Id = "Search",
                            Title = "Search",
                            ValidationType = ActionValidationType.All
                        }),
                    Ticks = BulildDateTimeRangeHtmlMode(pageModel, x => x.SearchPanel.Ticks),
                    StartExecutedTicks = BulildDateTimeRangeHtmlMode(pageModel, x => x.SearchPanel.StartExecutedTicks),
                    MinimalStartTicks = BulildDateTimeRangeHtmlMode(pageModel, x => x.SearchPanel.MinimalStartTicks)
                };
        }

        private KeyValuePair<TextBoxHtmlModel, CheckBoxHtmlModel>[] GetStatesGroup(RemoteTaskQueueModel pageModel, Func<Pair<TaskState, bool?>, bool> criterion)
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

        private DateTimeRangeHtmlModel BulildDateTimeRangeHtmlMode(RemoteTaskQueueModel pageModel, Expression<Func<RemoteTaskQueuePageModel, DateTimeRangeModel>> pathToDateTimeRange)
        {
            return new DateTimeRangeHtmlModel
                {
                    From = htmlModelsCreator.DateAndTimeFor(pageModel, pathToDateTimeRange.Merge(dtr => dtr.From), new DateAndTimeOptions
                        {
                            NeedTime = true
                        }),
                    To = htmlModelsCreator.DateAndTimeFor(pageModel, pathToDateTimeRange.Merge(dtr => dtr.To), new DateAndTimeOptions
                        {
                            NeedTime = true
                        })
                };
        }

        private int GetStateIndex(TaskState state, Pair<TaskState, bool?>[] states)
        {
            for(int i = 0; i < states.Length; i++)
            {
                if(states[i].Key == state)
                    return i;
            }
            return -1;
        }

        private readonly IHtmlModelsCreator<RemoteTaskQueuePageModel> htmlModelsCreator;
        private readonly Dictionary<TaskState, string> taskStates;
    }
}