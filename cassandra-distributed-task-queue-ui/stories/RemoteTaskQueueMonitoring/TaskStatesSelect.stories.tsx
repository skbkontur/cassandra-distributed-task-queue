import { action } from "@storybook/addon-actions";
import * as React from "react";

import { TaskStatesSelect } from "../../src/RemoteTaskQueueMonitoring/components/TaskStatesSelect/TaskStatesSelect";

export default {
    title: "RemoteTaskQueueMonitoring/TaskStatesSelect",
    component: TaskStatesSelect,
};

export const Default = () => <TaskStatesSelect value={[]} onChange={action("onChange")} />;
