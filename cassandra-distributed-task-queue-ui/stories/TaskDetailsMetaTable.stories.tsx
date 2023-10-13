import { withRouter } from "storybook-addon-react-router-v6";

import { TaskState } from "../src/Domain/Api/TaskState";
import { TaskDetailsMetaTable } from "../src/components/TaskDetailsMetaTable/TaskDetailsMetaTable";

export default {
    title: "RemoteTaskQueueMonitoring/TaskDetailsMetaTable",
    component: TaskDetailsMetaTable,
    decorators: [withRouter],
};

export const Default = () => (
    <TaskDetailsMetaTable
        taskMeta={{
            name: "SynchronizeUserPartiesToPortalTaskData",
            id: "1231312312312312",
            ticks: "636275120594815095",
            minimalStartTicks: "636275120594815095",
            startExecutingTicks: "636275120594815095",
            finishExecutingTicks: "636275120594815095",
            executionDurationTicks: "0",
            state: TaskState.Finished,
            attempts: 1,
            lastModificationTicks: null,
            expirationTimestampTicks: null,
            expirationModificationTicks: null,
            parentTaskId: null,
        }}
        childTaskIds={["1e813176-a672-11e6-8c67-1218c2e5c7a5", "1e813176-a672-11e6-8c67-1218c2e5cwew"]}
    />
);
