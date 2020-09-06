import React from "react";
import StoryRouter from "storybook-react-router";

import { TaskState } from "../src/Domain/Api/TaskState";
import { TaskChainTree } from "../src/components/TaskChainTree/TaskChainTree";

import { createTask } from "./TaskMetaInformationUtils";

export default {
    title: "RemoteTaskQueueMonitoring/TaskChainTree",
    component: TaskChainTree,
    decorators: [StoryRouter()],
};

export const Direct = () => <TaskChainTree getTaskLocation={id => id} taskDetails={[regularSuccessfulTask, newTask]} />;

export const WithBranching = () => (
    <TaskChainTree getTaskLocation={id => id} taskDetails={[regularSuccessfulTaskWithChildren, newTask1, newTask2]} />
);

export const WithBranchingAndSeveralSeparateTrees = () => (
    <TaskChainTree
        getTaskLocation={id => id}
        taskDetails={[regularSuccessfulTask, newTask, regularSuccessfulTaskWithChildren, newTask1, newTask2]}
    />
);

const regularSuccessfulTask = {
    taskMeta: createTask({
        id: "task-1",
        state: TaskState.Finished,
    }),
    taskData: {},
    childTaskIds: ["task-2"],
    exceptionInfos: [],
};
const regularSuccessfulTaskWithChildren = {
    taskMeta: createTask({
        id: "task-5",
        state: TaskState.Finished,
    }),
    taskData: {},
    childTaskIds: ["task-6", "task-7"],
    exceptionInfos: [],
};
const newTask = {
    taskMeta: createTask({
        id: "task-2",
        parentTaskId: "task-1",
        state: TaskState.New,
        startExecutingTicks: null,
        finishExecutingTicks: null,
        attempts: 0,
    }),
    taskData: {},
    childTaskIds: [],
    exceptionInfos: [],
};
const newTask1 = {
    taskMeta: createTask({
        id: "task-6",
        parentTaskId: "task-1",
        state: TaskState.New,
        startExecutingTicks: null,
        finishExecutingTicks: null,
        attempts: 0,
    }),
    taskData: {},
    childTaskIds: [],
    exceptionInfos: [],
};
const newTask2 = {
    taskMeta: createTask({
        id: "task-7",
        parentTaskId: "task-1",
        state: TaskState.New,
        startExecutingTicks: null,
        finishExecutingTicks: null,
        attempts: 0,
    }),
    taskData: {},
    childTaskIds: [],
    exceptionInfos: [],
};
