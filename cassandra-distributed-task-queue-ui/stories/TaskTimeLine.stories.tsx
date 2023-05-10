import _ from "lodash";
import React from "react";
import { withRouter } from "storybook-addon-react-router-v6";

import { TaskState } from "../src/Domain/Api/TaskState";
import { TaskTimeLine } from "../src/components/TaskTimeLine/TaskTimeLine";

import { createTask } from "./TaskMetaInformationUtils";

export default {
    title: "RemoteTaskQueueMonitoring/TaskTimeLine",
    component: TaskTimeLine,
    decorators: [withRouter],
};

export const RegularTask = () => (
    <TaskTimeLine getHrefToTask={id => "/" + id} taskMeta={regularSuccessfulTask} childTaskIds={[]} />
);

export const RegularTaskWithKnownParent = () => (
    <TaskTimeLine
        taskMeta={{ ...regularSuccessfulTask, parentTaskId: "some-parent-task-id" }}
        childTaskIds={[]}
        getHrefToTask={id => "/" + id}
    />
);

export const RegularTaskWithKnownParentAndChilds = () => (
    <TaskTimeLine
        taskMeta={{
            ...regularSuccessfulTask,
            parentTaskId: "some-parent-task-id",
        }}
        childTaskIds={["some-child-task-id-1", "some-child-task-id-2", "some-child-task-id-3"]}
        getHrefToTask={id => "/" + id}
    />
);

export const RegularTaskWithKnownParentAnd100Childs = () => (
    <TaskTimeLine
        taskMeta={{
            ...regularSuccessfulTask,
            parentTaskId: "some-parent-task-id",
        }}
        childTaskIds={_.range(100).map(x => `some-child-task-id-${x}`)}
        getHrefToTask={id => "/" + id}
    />
);

export const RegularTaskWithKnownParentAnd4Childs = () => (
    <TaskTimeLine
        taskMeta={{
            ...regularSuccessfulTask,
            parentTaskId: "some-parent-task-id",
        }}
        childTaskIds={_.range(4).map(x => `some-child-task-id-${x}`)}
        getHrefToTask={id => "/" + id}
    />
);

export const RegularTaskWithRestarts = () => (
    <TaskTimeLine
        taskMeta={{ ...regularSuccessfulTask, attempts: 10 }}
        childTaskIds={[]}
        getHrefToTask={id => "/" + id}
    />
);

export const FatalErrorTask = () => (
    <TaskTimeLine taskMeta={instantFailTask} childTaskIds={[]} getHrefToTask={id => "/" + id} />
);

export const FatalErrorTaskWithRestarts = () => (
    <TaskTimeLine taskMeta={{ ...instantFailTask, attempts: 10 }} childTaskIds={[]} getHrefToTask={id => "/" + id} />
);

export const CancelledUntilExecution = () => (
    <TaskTimeLine taskMeta={canceledBeforeExecution} childTaskIds={[]} getHrefToTask={id => "/" + id} />
);

export const CancelledUntilSeveralTries = () => (
    <TaskTimeLine taskMeta={canceledAfterReruns} childTaskIds={[]} getHrefToTask={id => "/" + id} />
);

export const Новая = () => <TaskTimeLine taskMeta={newTask} childTaskIds={[]} getHrefToTask={id => "/" + id} />;

export const InProcess = () => (
    <TaskTimeLine taskMeta={inProcessTask} childTaskIds={[]} getHrefToTask={id => "/" + id} />
);

export const WaitedForRestart = () => (
    <TaskTimeLine taskMeta={waitingForRerunTask} childTaskIds={[]} getHrefToTask={id => "/" + id} />
);

export const WaitedForRestartAfterError = () => (
    <TaskTimeLine taskMeta={waitingForRerunAfterErrorTask} childTaskIds={[]} getHrefToTask={id => "/" + id} />
);

const regularSuccessfulTask = createTask({ state: TaskState.Finished });
const instantFailTask = createTask({ state: TaskState.Fatal });
const canceledBeforeExecution = createTask({
    state: TaskState.Canceled,
    startExecutingTicks: null,
    finishExecutingTicks: null,
    attempts: 0,
});

const canceledAfterReruns = createTask({
    state: TaskState.Canceled,
    attempts: 10,
});

const newTask = createTask({
    state: TaskState.New,
    startExecutingTicks: null,
    finishExecutingTicks: null,
    attempts: 0,
});

const inProcessTask = createTask({
    state: TaskState.InProcess,
    finishExecutingTicks: null,
    attempts: 0,
});

const waitingForRerunTask = createTask({
    state: TaskState.WaitingForRerun,
    attempts: 2,
});

const waitingForRerunAfterErrorTask = createTask({
    state: TaskState.WaitingForRerunAfterError,
    attempts: 2,
});
