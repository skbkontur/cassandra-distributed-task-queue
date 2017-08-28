// @flow
import React from "react";
import ReactDom from "react-dom";
import { IndexRoute, Router, Route, browserHistory } from "react-router";
import TasksPage from "./containers/TasksPageContainer";
import TaskDetailsPageContainer from "./containers/TaskDetailsPageContainer";
import TaskChainsTreeContainer from "./containers/TaskChainsTreeContainer";
import Layout from "./components/Layout/Layout";
import { RemoteTaskQueueApi } from "../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueue";
import { ApiProvider } from "./api/RemoteTaskQueueApiInjection";

import "ui/styles/reset.less";
import "ui/styles/typography.less";

const api = new RemoteTaskQueueApi("/internal-api/remote-task-queue/");
const TasksPath = "/AdminTools/Tasks";

ReactDom.render(
    <ApiProvider remoteTaskQueueApi={api}>
        <Router history={browserHistory}>
            <Route path={TasksPath} component={Layout}>
                <IndexRoute
                    component={({ location }) => <TasksPage searchQuery={location.search} {...location.state} />}
                />
                <Route
                    path="Tree"
                    component={({ location }) =>
                        <TaskChainsTreeContainer
                            searchQuery={location.search}
                            {...location.state}
                            parentLocation={(location.state && location.state.parentLocation) || null}
                        />}
                />
                <Route
                    path=":id"
                    component={({ location, params }) =>
                        <TaskDetailsPageContainer
                            id={params.id || ""}
                            parentLocation={(location.state && location.state.parentLocation) || null}
                        />}
                />
            </Route>
        </Router>
    </ApiProvider>,
    document.getElementById("content")
);
