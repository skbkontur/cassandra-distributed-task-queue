import { LocationDescriptor } from "history";
import _ from "lodash";
import * as React from "react";
import { IconName, RouterLink } from "ui";
import { ColumnStack, Fit } from "ui/layout";
import { AllowCopyToClipboard } from "Commons/AllowCopyToClipboard";
import { TaskMetaInformationAndTaskMetaInformationChildTasks } from "Domain/EDI/Api/RemoteTaskQueue/TaskMetaInformationChildTasks";
import { TaskStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskState";

import { TimeLine } from "../TaskTimeLine/TimeLine/TimeLine";

import cn from "./TaskChainTree.less";

const IconColors = {
    red: "#d43517",
    green: "#3F9726",
    grey: "#a0a0a0",
    warning: "#ff9900",
};

interface TaskChainTreeProps {
    taskMetas: TaskMetaInformationAndTaskMetaInformationChildTasks[];
    getTaskLocation: (id: string) => LocationDescriptor;
}

export class TaskChainTree extends React.Component<TaskChainTreeProps, $FlowFixMeState> {
    public buildTaskTimeLineEntry(taskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks): JSX.Element {
        const { getTaskLocation } = this.props;

        let iconAndColorProps: { icon: IconName; iconColor: undefined | string } = {
            icon: "Ok",
            iconColor: undefined,
        };
        switch (taskMeta.state) {
            case TaskStates.Unknown:
                iconAndColorProps = {
                    icon: "HelpLite",
                    iconColor: IconColors.warning,
                };
                break;
            case TaskStates.New:
                iconAndColorProps = {
                    icon: "Clock",
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.WaitingForRerun:
                iconAndColorProps = {
                    icon: "Clock",
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.WaitingForRerunAfterError:
                iconAndColorProps = {
                    icon: "Clock",
                    iconColor: IconColors.red,
                };
                break;
            case TaskStates.Finished:
                iconAndColorProps = {
                    icon: "Ok",
                    iconColor: IconColors.green,
                };
                break;
            case TaskStates.InProcess:
                iconAndColorProps = {
                    icon: "Clock",
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.Fatal:
                iconAndColorProps = {
                    icon: "Clear",
                    iconColor: IconColors.red,
                };
                break;
            case TaskStates.Canceled:
                iconAndColorProps = {
                    icon: "Delete",
                    iconColor: IconColors.red,
                };
                break;
            default:
                break;
        }
        const TimeLineEntry = TimeLine.Entry;
        return (
            <TimeLineEntry {...iconAndColorProps} key={taskMeta.id} data-tid="TimeLineTaskItem">
                <div className={cn("task-name")} data-tid="TaskName">
                    <RouterLink to={getTaskLocation(taskMeta.id)}>{taskMeta.name}</RouterLink>
                </div>
                <div className={cn("task-id")} data-tid="TaskId">
                    <AllowCopyToClipboard>{taskMeta.id}</AllowCopyToClipboard>
                </div>
            </TimeLineEntry>
        );
    }

    public buildChildEntries(
        taskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks,
        taskMetaHashSet: { [key: string]: TaskMetaInformationAndTaskMetaInformationChildTasks }
    ): JSX.Element[] {
        if (!taskMeta.childTaskIds || taskMeta.childTaskIds.length === 0) {
            return [];
        }
        if (taskMeta.childTaskIds.length === 1) {
            return this.buildTaskTimeLine(taskMetaHashSet[taskMeta.childTaskIds[0]], taskMetaHashSet);
        }
        const TimeLineBranch = TimeLine.Branch;
        const TimeLineBranchNode = TimeLine.BranchNode;
        return [
            <TimeLineBranchNode key={`${taskMeta.id}-branches`}>
                {taskMeta.childTaskIds
                    .map(x => taskMetaHashSet[x])
                    .filter(x => x)
                    .map((x, i) => (
                        <TimeLineBranch key={i}>{this.buildTaskTimeLine(x, taskMetaHashSet)}</TimeLineBranch>
                    ))}
            </TimeLineBranchNode>,
        ];
    }

    public buildTaskTimeLine(
        taskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks,
        taskMetaHashSet: { [key: string]: TaskMetaInformationAndTaskMetaInformationChildTasks }
    ): JSX.Element[] {
        return [this.buildTaskTimeLineEntry(taskMeta), ...this.buildChildEntries(taskMeta, taskMetaHashSet)];
    }

    public findMostParentTask(
        taskMetaHashSet: { [key: string]: TaskMetaInformationAndTaskMetaInformationChildTasks },
        startTaskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks
    ): TaskMetaInformationAndTaskMetaInformationChildTasks {
        let result = startTaskMeta;
        while (result.parentTaskId) {
            if (!taskMetaHashSet[result.parentTaskId]) {
                return result;
            }
            result = taskMetaHashSet[result.parentTaskId];
        }
        return result;
    }

    public findAllMostParents(taskMetaHashSet: {
        [key: string]: TaskMetaInformationAndTaskMetaInformationChildTasks;
    }): TaskMetaInformationAndTaskMetaInformationChildTasks[] {
        let mostParentTasks = Object.getOwnPropertyNames(taskMetaHashSet)
            .map(x => taskMetaHashSet[x])
            .map(x => this.findMostParentTask(taskMetaHashSet, x));
        mostParentTasks = _.uniq(mostParentTasks);
        mostParentTasks = _.sortBy(mostParentTasks, x => x.ticks);
        mostParentTasks = _.reverse(mostParentTasks);
        return mostParentTasks;
    }

    public render(): JSX.Element {
        const { taskMetas } = this.props;
        const taskMetaHashSet = taskMetas.reduce((result, taskMeta) => {
            result[taskMeta.id] = taskMeta;
            return result;
        }, {});

        const mostParentTasks = this.findAllMostParents(taskMetaHashSet);
        return (
            <ColumnStack block stretch gap={8} data-tid={"TimeLines"}>
                {mostParentTasks.map((x, i) => (
                    <Fit key={i} data-tid="TimeLine">
                        <TimeLine>{this.buildTaskTimeLine(x, taskMetaHashSet)}</TimeLine>
                    </Fit>
                ))}
            </ColumnStack>
        );
    }
}
