import { withRouter } from "storybook-addon-react-router-v6";

import { TaskNotFoundPage } from "../src/components/TaskNotFoundPage/TaskNotFoundPage";

export default {
    title: "RemoteTaskQueueMonitoring/TaskNotFoundPage",
    component: TaskNotFoundPage,
    decorators: [withRouter],
};

export const Default = () => <TaskNotFoundPage />;
