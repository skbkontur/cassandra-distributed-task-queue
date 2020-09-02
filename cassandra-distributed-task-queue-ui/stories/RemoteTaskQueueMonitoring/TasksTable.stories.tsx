import { action } from "@storybook/addon-actions";
import * as React from "react";
import StoryRouter from "storybook-react-router";
import { TaskMetaInformation } from "Domain/EDI/Api/RemoteTaskQueue/TaskMetaInformation";
import { TaskState } from "Domain/EDI/Api/RemoteTaskQueue/TaskState";

import { TaskDetails } from "../../src/RemoteTaskQueueMonitoring/components/TaskTable/TaskDetails/TaskDetails";
import { TasksTable } from "../../src/RemoteTaskQueueMonitoring/components/TaskTable/TaskTable";

import { createTask } from "./TaskMetaInformationUtils";

export default {
    title: "RemoteTaskQueueMonitoring/TasksTable",
    decorators: [StoryRouter()],
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

const testTaskInfoSources: Array<Partial<TaskMetaInformation>> = [
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
