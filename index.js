// @flow
import * as React from "react";
import ReactDom from "react-dom";
import { BrowserRouter, Switch, Route } from "react-router-dom";
import { WindowUtils } from "Commons/DomUtils";
import { type RouterLocationDescriptor, type RouteOptions } from "react-router-dom";

import ServiceHeader from "../ServiceHeader/components/ServiceHeader";
import { RemoteTaskQueueApi } from "../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueue";

import TasksPage from "./containers/TasksPageContainer";
import TaskDetailsPageContainer from "./containers/TaskDetailsPageContainer";
import TaskChainsTreeContainer from "./containers/TaskChainsTreeContainer";
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
    <BrowserRouter>
        <ApiProvider remoteTaskQueueApi={api}>
            <Route path={TasksPath} component={RemoteTaskQueueApplication} />
        </ApiProvider>
    </BrowserRouter>,
    WindowUtils.getElementByIdToRenderApp("content")
);

export function RemoteTaskQueueApplication({ match }: RouteOptions): React.Node {
    const baseUrl = match.url;
    return (
        <ServiceHeader currentInterfaceType={null}>
            <Switch>
                <Route
                    exact
                    path={`${baseUrl}/`}
                    component={({ location }) => <TasksPage searchQuery={location.search} {...location.state} />}
                />
                <Route
                    exact
                    path={`${baseUrl}/Tree`}
                    component={({ location }) => (
                        <TaskChainsTreeContainer
                            searchQuery={location.search}
                            {...location.state}
                            parentLocation={tryGetParentLocationFromHistoryState(location)}
                        />
                    )}
                />
                <Route
                    path={`${baseUrl}/:id`}
                    component={({ location, match: { params } }) => (
                        <TaskDetailsPageContainer
                            id={params.id || ""}
                            parentLocation={tryGetParentLocationFromHistoryState(location)}
                        />
                    )}
                />
            </Switch>
        </ServiceHeader>
    );
}
