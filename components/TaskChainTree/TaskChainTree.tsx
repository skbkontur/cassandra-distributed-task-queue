import ClearIcon from "@skbkontur/react-icons/Clear";
import ClockIcon from "@skbkontur/react-icons/Clock";
import DeleteIcon from "@skbkontur/react-icons/Delete";
import HelpLiteIcon from "@skbkontur/react-icons/HelpLite";
import OkIcon from "@skbkontur/react-icons/Ok";
import { LocationDescriptor } from "history";
import _ from "lodash";
import * as React from "react";
import { RouterLink } from "ui";
import { ColumnStack, Fit } from "ui/layout";
import { AllowCopyToClipboard } from "Commons/AllowCopyToClipboard";
import { RemoteTaskInfoModel } from "Domain/EDI/Api/RemoteTaskQueue/RemoteTaskInfoModel";
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
    taskDetails: RemoteTaskInfoModel[];
    getTaskLocation: (id: string) => LocationDescriptor;
}

export class TaskChainTree extends React.Component<TaskChainTreeProps> {
    public buildTaskTimeLineEntry({ taskMeta }: RemoteTaskInfoModel): JSX.Element {
        const { getTaskLocation } = this.props;

        let iconAndColorProps: { icon: JSX.Element; iconColor: undefined | string } = {
            icon: <OkIcon />,
            iconColor: undefined,
        };
        switch (taskMeta.state) {
            case TaskStates.Unknown:
                iconAndColorProps = {
                    icon: <HelpLiteIcon />,
                    iconColor: IconColors.warning,
                };
                break;
            case TaskStates.New:
                iconAndColorProps = {
                    icon: <ClockIcon />,
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.WaitingForRerun:
                iconAndColorProps = {
                    icon: <ClockIcon />,
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.WaitingForRerunAfterError:
                iconAndColorProps = {
                    icon: <ClockIcon />,
                    iconColor: IconColors.red,
                };
                break;
            case TaskStates.Finished:
                iconAndColorProps = {
                    icon: <OkIcon />,
                    iconColor: IconColors.green,
                };
                break;
            case TaskStates.InProcess:
                iconAndColorProps = {
                    icon: <ClockIcon />,
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.Fatal:
                iconAndColorProps = {
                    icon: <ClearIcon />,
                    iconColor: IconColors.red,
                };
                break;
            case TaskStates.Canceled:
                iconAndColorProps = {
                    icon: <DeleteIcon />,
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
        { taskMeta, childTaskIds }: RemoteTaskInfoModel,
        taskMetaHashSet: { [key: string]: RemoteTaskInfoModel }
    ): JSX.Element[] {
        if (!childTaskIds || childTaskIds.length === 0) {
            return [];
        }
        if (childTaskIds.length === 1) {
            return this.buildTaskTimeLine(taskMetaHashSet[childTaskIds[0]], taskMetaHashSet);
        }
        const TimeLineBranch = TimeLine.Branch;
        const TimeLineBranchNode = TimeLine.BranchNode;
        return [
            <TimeLineBranchNode key={`${taskMeta.id}-branches`}>
                {childTaskIds
                    .map(x => taskMetaHashSet[x])
                    .filter(x => x)
                    .map((x, i) => (
                        <TimeLineBranch key={i}>{this.buildTaskTimeLine(x, taskMetaHashSet)}</TimeLineBranch>
                    ))}
            </TimeLineBranchNode>,
        ];
    }

    public buildTaskTimeLine(
        taskMeta: RemoteTaskInfoModel,
        taskMetaHashSet: { [key: string]: RemoteTaskInfoModel }
    ): JSX.Element[] {
        return [this.buildTaskTimeLineEntry(taskMeta), ...this.buildChildEntries(taskMeta, taskMetaHashSet)];
    }

    public findMostParentTask(
        taskMetaHashSet: { [key: string]: RemoteTaskInfoModel },
        startTaskMeta: RemoteTaskInfoModel
    ): RemoteTaskInfoModel {
        let result = startTaskMeta;
        while (result.taskMeta.parentTaskId) {
            if (!taskMetaHashSet[result.taskMeta.parentTaskId]) {
                return result;
            }
            result = taskMetaHashSet[result.taskMeta.parentTaskId];
        }
        return result;
    }

    public findAllMostParents(taskMetaHashSet: { [key: string]: RemoteTaskInfoModel }): RemoteTaskInfoModel[] {
        let mostParentTasks = Object.getOwnPropertyNames(taskMetaHashSet)
            .map(x => taskMetaHashSet[x])
            .map(x => this.findMostParentTask(taskMetaHashSet, x));
        mostParentTasks = _.uniq(mostParentTasks);
        mostParentTasks = _.sortBy(mostParentTasks, x => x.taskMeta.ticks);
        mostParentTasks = _.reverse(mostParentTasks);
        return mostParentTasks;
    }

    public render(): JSX.Element {
        const { taskDetails } = this.props;
        const taskMetaHashSet = taskDetails.reduce((result, taskDetails) => {
            result[taskDetails.taskMeta.id] = taskDetails;
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
