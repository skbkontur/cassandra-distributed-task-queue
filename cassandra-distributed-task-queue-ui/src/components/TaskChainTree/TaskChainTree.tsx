import { ColumnStack, Fit } from "@skbkontur/react-stack-layout";
import { ThemeContext } from "@skbkontur/react-ui";
import { LocationDescriptor } from "history";
import _ from "lodash";
import React from "react";

import { RtqMonitoringTaskModel } from "../../Domain/Api/RtqMonitoringTaskModel";
import { AllowCopyToClipboard } from "../AllowCopyToClipboard";
import { RouterLink } from "../RouterLink/RouterLink";
import { TimeLine } from "../TaskTimeLine/TimeLine/TimeLine";

import { jsStyles } from "./TaskChainTree.styles";
import { TaskStateIcon } from "./TaskStateIcon";

interface TaskChainTreeProps {
    taskDetails: RtqMonitoringTaskModel[];
    getTaskLocation: (id: string) => LocationDescriptor;
}

export function TaskChainTree({ taskDetails, getTaskLocation }: TaskChainTreeProps): JSX.Element {
    const theme = React.useContext(ThemeContext);

    const buildTaskTimeLineEntry = ({ taskMeta }: RtqMonitoringTaskModel): JSX.Element => {
        const TimeLineEntry = TimeLine.Entry;
        return (
            <TimeLineEntry
                icon={<TaskStateIcon taskState={taskMeta.state} />}
                key={taskMeta.id}
                data-tid="TimeLineTaskItem">
                <div data-tid="TaskName">
                    <RouterLink to={getTaskLocation(taskMeta.id)}>{taskMeta.name}</RouterLink>
                </div>
                <div className={jsStyles.taskId(theme)} data-tid="TaskId">
                    <AllowCopyToClipboard>{taskMeta.id}</AllowCopyToClipboard>
                </div>
            </TimeLineEntry>
        );
    };

    const buildChildEntries = (
        { taskMeta, childTaskIds }: RtqMonitoringTaskModel,
        taskMetaHashSet: { [key: string]: RtqMonitoringTaskModel }
    ): JSX.Element[] => {
        if (!childTaskIds || childTaskIds.length === 0) {
            return [];
        }
        if (childTaskIds.length === 1) {
            return buildTaskTimeLine(taskMetaHashSet[childTaskIds[0]], taskMetaHashSet);
        }
        const TimeLineBranch = TimeLine.Branch;
        const TimeLineBranchNode = TimeLine.BranchNode;
        return [
            <TimeLineBranchNode key={`${taskMeta.id}-branches`}>
                {childTaskIds
                    .map(x => taskMetaHashSet[x])
                    .filter(x => x)
                    .map((x, i) => (
                        <TimeLineBranch key={i}>{buildTaskTimeLine(x, taskMetaHashSet)}</TimeLineBranch>
                    ))}
            </TimeLineBranchNode>,
        ];
    };

    const buildTaskTimeLine = (
        taskMeta: RtqMonitoringTaskModel,
        taskMetaHashSet: { [key: string]: RtqMonitoringTaskModel }
    ): JSX.Element[] => {
        return [buildTaskTimeLineEntry(taskMeta), ...buildChildEntries(taskMeta, taskMetaHashSet)];
    };

    const findMostParentTask = (
        taskMetaHashSet: { [key: string]: RtqMonitoringTaskModel },
        startTaskMeta: RtqMonitoringTaskModel
    ): RtqMonitoringTaskModel => {
        let result = startTaskMeta;
        while (result.taskMeta.parentTaskId) {
            if (!taskMetaHashSet[result.taskMeta.parentTaskId]) {
                return result;
            }
            result = taskMetaHashSet[result.taskMeta.parentTaskId];
        }
        return result;
    };

    const findAllMostParents = (taskMetaHashSet: {
        [key: string]: RtqMonitoringTaskModel;
    }): RtqMonitoringTaskModel[] => {
        let mostParentTasks = Object.getOwnPropertyNames(taskMetaHashSet)
            .map(x => taskMetaHashSet[x])
            .map(x => findMostParentTask(taskMetaHashSet, x));
        mostParentTasks = _.uniq(mostParentTasks);
        mostParentTasks = _.sortBy(mostParentTasks, x => x.taskMeta.ticks);
        mostParentTasks = _.reverse(mostParentTasks);
        return mostParentTasks;
    };

    const taskMetaHashSet = taskDetails.reduce((result, taskDetails) => {
        result[taskDetails.taskMeta.id] = taskDetails;
        return result;
    }, {});

    const mostParentTasks = findAllMostParents(taskMetaHashSet);
    return (
        <ColumnStack block stretch gap={8} data-tid={"TimeLines"}>
            {mostParentTasks.map((x, i) => (
                <Fit key={i} data-tid="TimeLine">
                    <TimeLine>{buildTaskTimeLine(x, taskMetaHashSet)}</TimeLine>
                </Fit>
            ))}
        </ColumnStack>
    );
}
