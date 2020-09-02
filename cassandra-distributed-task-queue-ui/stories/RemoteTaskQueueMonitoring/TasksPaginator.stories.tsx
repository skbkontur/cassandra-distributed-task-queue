import * as React from "react";
import StoryRouter from "storybook-react-router";

import { TasksPaginator } from "../../src/RemoteTaskQueueMonitoring/components/TasksPaginator/TasksPaginator";

export default {
    title: "RemoteTaskQueueMonitoring/TasksPaginator",
    component: TasksPaginator,
    decorators: [StoryRouter()],
};

export const Default = () => (
    <TasksPaginator nextPageLocation={"nextPageLocation"} prevPageLocation={"prevPageLocation"} />
);

export const WithoutPrevLink = () => <TasksPaginator nextPageLocation={"nextPageLocation"} prevPageLocation={null} />;

export const WithoutNextLink = () => <TasksPaginator nextPageLocation={null} prevPageLocation={"prevPageLocation"} />;
