// @flow
import * as React from "react";
import ReactDom from "react-dom";
import { WindowUtils } from "Commons/DomUtils";
import { IndexRoute, Router, Route, browserHistory, type RouterLocationDescriptor } from "react-router";

import { RemoteTaskQueueApi } from "../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueue";

import TasksPage from "./containers/TasksPageContainer";
import TaskDetailsPageContainer from "./containers/TaskDetailsPageContainer";
import TaskChainsTreeContainer from "./containers/TaskChainsTreeContainer";
import Layout from "./components/Layout/Layout";
import { ApiProvider } from "./api/RemoteTaskQueueApiInjection";

import "ui/styles/reset.less";
import "ui/styles/typography.less";

const api = new RemoteTaskQueueApi("/internal-api/remote-task-queue/");
const TasksPath = "/AdminTools/Tasks";

function tryGetParentLocationFromHistoryState(location: RouterLocationDescriptor): ?RouterLocationDescriptor {
    if (location.state == null) {
        return null;
    }
    if (location.state.parentLocation != null) {
        const parentLocation = location.state.parentLocation;
        if (typeof parentLocation === "string") {
            return parentLocation;
        }
        if (typeof parentLocation === "object") {
            const { pathname, search } = parentLocation;
            if (typeof pathname === "string" && (search == null || typeof search === "string")) {
                return {
                    pathname: pathname,
                    search: search,
                };
            }
        }
    }
    return null;
}

ReactDom.render(
    <ApiProvider remoteTaskQueueApi={api}>
        <Router history={browserHistory}>
            <Route path={TasksPath} component={Layout}>
                <IndexRoute
                    component={({ location }) => <TasksPage searchQuery={location.search} {...location.state} />}
                />
                <Route
                    path="Tree"
                    component={({ location }) => (
                        <TaskChainsTreeContainer
                            searchQuery={location.search}
                            {...location.state}
                            parentLocation={tryGetParentLocationFromHistoryState(location)}
                        />
                    )}
                />
                <Route
                    path=":id"
                    component={({ location, params }) => (
                        <TaskDetailsPageContainer
                            id={params.id || ""}
                            parentLocation={tryGetParentLocationFromHistoryState(location)}
                        />
                    )}
                />
            </Route>
        </Router>
    </ApiProvider>,
    WindowUtils.getElementByIdToRenderApp("content")
);
