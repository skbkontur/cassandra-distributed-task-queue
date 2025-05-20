import { withRouter } from "storybook-addon-remix-react-router";

import { TaskNotFoundPage } from "../src/components/TaskNotFoundPage/TaskNotFoundPage";

export default {
    title: "RemoteTaskQueueMonitoring/TaskNotFoundPage",
    component: TaskNotFoundPage,
    decorators: [withRouter],
};

export const Default = () => <TaskNotFoundPage />;
