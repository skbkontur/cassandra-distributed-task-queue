import { action } from "@storybook/addon-actions";

import { TaskStatesSelect } from "../src/components/TaskStatesSelect/TaskStatesSelect";

export default {
    title: "RemoteTaskQueueMonitoring/TaskStatesSelect",
    component: TaskStatesSelect,
};

export const Default = () => <TaskStatesSelect value={[]} onChange={action("onChange")} />;
