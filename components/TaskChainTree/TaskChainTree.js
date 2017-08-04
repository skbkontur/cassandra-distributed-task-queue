// @flow
import React from "react";
import { RouterLink } from "ui";
import { ColumnStack, Fit } from "ui/layout";
import type { RouterLocationDescriptor } from "../../../Commons/DataTypes/Routing";
import TimeLine from "../TaskTimeLine/TimeLine/TimeLine";
import type { TaskMetaInformationAndTaskMetaInformationChildTasks } from "../../api/RemoteTaskQueueApi";
import { TaskStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskState";
import _ from "lodash";
import AllowCopyToClipboard from "../../../Commons/AllowCopyToClipboard";
import cn from "./TaskChainTree.less";

const IconColors = {
    red: "#d43517",
    green: "#3F9726",
    grey: "#a0a0a0",
    warning: "#ff9900",
};

type TaskChainTreeProps = {
    taskMetas: TaskMetaInformationAndTaskMetaInformationChildTasks[],
    getTaskLocation: (id: string) => RouterLocationDescriptor,
};

export default class TaskChainTree extends React.Component {
    props: TaskChainTreeProps;

    buildTaskTimeLineEntry(taskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks): React.Element<any> {
        const { getTaskLocation } = this.props;

        let iconAndColorProps = {
            icon: "ok",
            iconColor: undefined,
        };
        switch (taskMeta.state) {
            case TaskStates.Unknown:
                iconAndColorProps = {
                    icon: "help-o",
                    iconColor: IconColors.warning,
                };
                break;
            case TaskStates.New:
                iconAndColorProps = {
                    icon: "wait",
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.WaitingForRerun:
                iconAndColorProps = {
                    icon: "wait",
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.WaitingForRerunAfterError:
                iconAndColorProps = {
                    icon: "wait",
                    iconColor: IconColors.red,
                };
                break;
            case TaskStates.Finished:
                iconAndColorProps = {
                    icon: "ok",
                    iconColor: IconColors.green,
                };
                break;
            case TaskStates.InProcess:
                iconAndColorProps = {
                    icon: "wait",
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.Fatal:
                iconAndColorProps = {
                    icon: "clear",
                    iconColor: IconColors.red,
                };
                break;
            case TaskStates.Canceled:
                iconAndColorProps = {
                    icon: "remove",
                    iconColor: IconColors.red,
                };
                break;
            default:
                break;
        }
        const TimeLineEntry = TimeLine.Entry;
        return (
            <TimeLineEntry {...iconAndColorProps} key={taskMeta.id}>
                <div className={cn("task-name")}>
                    <RouterLink to={getTaskLocation(taskMeta.id)}>
                        {taskMeta.name}
                    </RouterLink>
                </div>
                <div className={cn("task-id")}>
                    <AllowCopyToClipboard>
                        {taskMeta.id}
                    </AllowCopyToClipboard>
                </div>
            </TimeLineEntry>
        );
    }

    buildChildEntries(
        taskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks,
        taskMetaHashSet: { [key: string]: TaskMetaInformationAndTaskMetaInformationChildTasks }
    ): React.Element<any>[] {
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
                {taskMeta.childTaskIds.map(x => taskMetaHashSet[x]).filter(x => x).map((x, i) =>
                    <TimeLineBranch key={i}>
                        {this.buildTaskTimeLine(x, taskMetaHashSet)}
                    </TimeLineBranch>
                )}
            </TimeLineBranchNode>,
        ];
    }

    buildTaskTimeLine(
        taskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks,
        taskMetaHashSet: { [key: string]: TaskMetaInformationAndTaskMetaInformationChildTasks }
    ): React.Element<any>[] {
        return [this.buildTaskTimeLineEntry(taskMeta), ...this.buildChildEntries(taskMeta, taskMetaHashSet)];
    }

    findMostParentTask(
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

    findAllMostParents(taskMetaHashSet: {
        [key: string]: TaskMetaInformationAndTaskMetaInformationChildTasks,
    }): TaskMetaInformationAndTaskMetaInformationChildTasks[] {
        let mostParentTasks = Object.getOwnPropertyNames(taskMetaHashSet)
            .map(x => taskMetaHashSet[x])
            .map(x => this.findMostParentTask(taskMetaHashSet, x));
        mostParentTasks = _.uniq(mostParentTasks);
        mostParentTasks = _.sortBy(mostParentTasks, x => x.ticks);
        mostParentTasks = _.reverse(mostParentTasks);
        return mostParentTasks;
    }

    render(): React.Element<*> {
        const { taskMetas } = this.props;
        const taskMetaHashSet = taskMetas.reduce((result, taskMeta) => {
            result[taskMeta.id] = taskMeta;
            return result;
        }, {});

        const mostParentTasks = this.findAllMostParents(taskMetaHashSet);

        return (
            <ColumnStack block stretch gap={8}>
                {mostParentTasks.map((x, i) =>
                    <Fit key={i}>
                        <TimeLine>
                            {this.buildTaskTimeLine(x, taskMetaHashSet)}
                        </TimeLine>
                    </Fit>
                )}
            </ColumnStack>
        );
    }
}
