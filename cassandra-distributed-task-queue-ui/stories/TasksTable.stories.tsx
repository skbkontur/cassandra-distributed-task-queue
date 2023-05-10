import { action } from "@storybook/addon-actions";
import React from "react";
import { withRouter } from "storybook-addon-react-router-v6";

import { RtqMonitoringTaskMeta } from "../src/Domain/Api/RtqMonitoringTaskMeta";
import { TaskState } from "../src/Domain/Api/TaskState";
import { TaskDetails } from "../src/components/TaskTable/TaskDetails/TaskDetails";
import { TasksTable } from "../src/components/TaskTable/TaskTable";

import { createTask } from "./TaskMetaInformationUtils";

export default {
    title: "RemoteTaskQueueMonitoring/TasksTable",
    decorators: [withRouter],
};

export const SeveralTasks = () => (
    <TasksTable
        getTaskLocation={id => id}
        allowRerunOrCancel
        taskInfos={taskInfos}
        onRerun={action("onRerun")}
        onCancel={action("onCancel")}
    />
);

export const OneTask = () => (
    <TaskDetails
        getTaskLocation={id => id}
        taskInfo={taskInfos[0]}
        allowRerunOrCancel
        onRerun={action("onRerun")}
        onCancel={action("onCancel")}
    />
);

const testTaskInfoSources: Array<Partial<RtqMonitoringTaskMeta>> = [
    {
        name: "SynchronizeUserPartiesToPortalTaskData",
        id: "1e813176-a672-11e6-8c67-1218c2e5c7a2",
        parentTaskId: "1e813176-a672-11e6-8c67-1218c2e5c7a2",
        ticks: "636275120594815095",
        minimalStartTicks: "636275120594815095",
        startExecutingTicks: "636275120594815095",
        finishExecutingTicks: "636275120594815095",
        state: TaskState.Finished,
        attempts: 1,
    },
    {
        name: "SynchronizeUserPartiesToPortalTaskData",
        id: "1e813176-a672-11e6-8c67-1218c2e5c723",
        ticks: "636275120594815095",
        minimalStartTicks: "636275120594815095",
        startExecutingTicks: "636275120594815095",
        finishExecutingTicks: "636275120594815095",
        state: TaskState.Finished,
        attempts: 1,
    },
    {
        name: "SynchronizeUserPartiesToPortalTaskData",
        id: "1e813176-a672-11e6-8c67-1218c2e5c7a5",
        ticks: "636275120594815095",
        minimalStartTicks: "636275120594815095",
        startExecutingTicks: "636275120594815095",
        finishExecutingTicks: "636275120594815095",
        state: TaskState.Finished,
        attempts: 1,
    },
    {
        name: "SynchronizeUserPartiesToPortalTaskData",
        id: "1e813176-a672-11e6-8c67-1218c2e5c7zz",
        ticks: "636275120594815095",
        minimalStartTicks: "636275120594815095",
        startExecutingTicks: "636275120594815095",
        finishExecutingTicks: "636275120594815095",
        state: TaskState.Finished,
        attempts: 1,
    },
    {
        name: "SynchronizeUserPartiesToPortalTaskData",
        id: "1e813176-a672-11e6-8c67-1218c2e5c789",
        ticks: "636275120594815095",
        minimalStartTicks: "636275120594815095",
        startExecutingTicks: "636275120594815095",
        finishExecutingTicks: "636275120594815095",
        state: TaskState.Finished,
        attempts: 1,
    },
    {
        name: "SynchronizeUserPartiesToPortalTaskData",
        id: "1e813176-a672-11e6-8c67-1218c2e5c736",
        ticks: "636275120594815095",
        minimalStartTicks: "636275120594815095",
        startExecutingTicks: "636275120594815095",
        finishExecutingTicks: "636275120594815095",
        state: TaskState.Finished,
        attempts: 1,
    },
    {
        name: "SynchronizeUserPartiesToPortalTaskData",
        id: "1e813176-a672-11e6-8c67-1218c2e5c7f2",
        ticks: "636275120594815095",
        minimalStartTicks: "636275120594815095",
        startExecutingTicks: "636275120594815095",
        finishExecutingTicks: "636275120594815095",
        state: TaskState.Finished,
        attempts: 1,
    },
    {
        name: "SynchronizeUserPartiesToPortalTaskData",
        id: "1e813176-a672-11e6-8c67-1218c2e5c7xd",
        ticks: "636275120594815095",
        minimalStartTicks: "636275120594815095",
        startExecutingTicks: "636275120594815095",
        finishExecutingTicks: "636275120594815095",
        state: TaskState.Finished,
        attempts: 1,
    },
    {
        name: "SynchronizeUserPartiesToPortalTaskData",
        id: "1e813176-a672-11e6-8c67-1218c2e5c54d",
        ticks: "636275120594815095",
        minimalStartTicks: "636275120594815095",
        startExecutingTicks: "636275120594815095",
        finishExecutingTicks: "636275120594815095",
        state: TaskState.Finished,
        attempts: 1,
    },
];

const taskInfos = testTaskInfoSources.map(createTask);
