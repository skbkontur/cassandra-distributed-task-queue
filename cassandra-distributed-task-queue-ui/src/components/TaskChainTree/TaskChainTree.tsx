import ClearIcon from "@skbkontur/react-icons/Clear";
import ClockIcon from "@skbkontur/react-icons/Clock";
import DeleteIcon from "@skbkontur/react-icons/Delete";
import HelpLiteIcon from "@skbkontur/react-icons/HelpLite";
import OkIcon from "@skbkontur/react-icons/Ok";
import { ColumnStack, Fit } from "@skbkontur/react-stack-layout";
import { LocationDescriptor } from "history";
import _ from "lodash";
import React from "react";
import { Link } from "react-router-dom";

import { RtqMonitoringTaskModel } from "../../Domain/Api/RtqMonitoringTaskModel";
import { TaskState } from "../../Domain/Api/TaskState";
import { AllowCopyToClipboard } from "../AllowCopyToClipboard";
import { TimeLine } from "../TaskTimeLine/TimeLine/TimeLine";

import styles from "./TaskChainTree.less";

const IconColors = {
    red: "#d43517",
    green: "#3F9726",
    grey: "#a0a0a0",
    warning: "#ff9900",
};

interface TaskChainTreeProps {
    taskDetails: RtqMonitoringTaskModel[];
    getTaskLocation: (id: string) => LocationDescriptor;
}

export class TaskChainTree extends React.Component<TaskChainTreeProps> {
    public buildTaskTimeLineEntry({ taskMeta }: RtqMonitoringTaskModel): JSX.Element {
        const { getTaskLocation } = this.props;

        let iconAndColorProps: { icon: JSX.Element; iconColor: undefined | string } = {
            icon: <OkIcon />,
            iconColor: undefined,
        };
        switch (taskMeta.state) {
            case TaskState.Unknown:
                iconAndColorProps = {
                    icon: <HelpLiteIcon />,
                    iconColor: IconColors.warning,
                };
                break;
            case TaskState.New:
                iconAndColorProps = {
                    icon: <ClockIcon />,
                    iconColor: IconColors.grey,
                };
                break;
            case TaskState.WaitingForRerun:
                iconAndColorProps = {
                    icon: <ClockIcon />,
                    iconColor: IconColors.grey,
                };
                break;
            case TaskState.WaitingForRerunAfterError:
                iconAndColorProps = {
                    icon: <ClockIcon />,
                    iconColor: IconColors.red,
                };
                break;
            case TaskState.Finished:
                iconAndColorProps = {
                    icon: <OkIcon />,
                    iconColor: IconColors.green,
                };
                break;
            case TaskState.InProcess:
                iconAndColorProps = {
                    icon: <ClockIcon />,
                    iconColor: IconColors.grey,
                };
                break;
            case TaskState.Fatal:
                iconAndColorProps = {
                    icon: <ClearIcon />,
                    iconColor: IconColors.red,
                };
                break;
            case TaskState.Canceled:
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
                <div className={styles.taskName} data-tid="TaskName">
                    <Link className={styles.routerLink} to={getTaskLocation(taskMeta.id)}>
                        {taskMeta.name}
                    </Link>
                </div>
                <div className={styles.taskId} data-tid="TaskId">
                    <AllowCopyToClipboard>{taskMeta.id}</AllowCopyToClipboard>
                </div>
            </TimeLineEntry>
        );
    }

    public buildChildEntries(
        { taskMeta, childTaskIds }: RtqMonitoringTaskModel,
        taskMetaHashSet: { [key: string]: RtqMonitoringTaskModel }
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
        taskMeta: RtqMonitoringTaskModel,
        taskMetaHashSet: { [key: string]: RtqMonitoringTaskModel }
    ): JSX.Element[] {
        return [this.buildTaskTimeLineEntry(taskMeta), ...this.buildChildEntries(taskMeta, taskMetaHashSet)];
    }

    public findMostParentTask(
        taskMetaHashSet: { [key: string]: RtqMonitoringTaskModel },
        startTaskMeta: RtqMonitoringTaskModel
    ): RtqMonitoringTaskModel {
        let result = startTaskMeta;
        while (result.taskMeta.parentTaskId) {
            if (!taskMetaHashSet[result.taskMeta.parentTaskId]) {
                return result;
            }
            result = taskMetaHashSet[result.taskMeta.parentTaskId];
        }
        return result;
    }

    public findAllMostParents(taskMetaHashSet: { [key: string]: RtqMonitoringTaskModel }): RtqMonitoringTaskModel[] {
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
