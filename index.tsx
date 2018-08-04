import { LocationDescriptor } from "history";
import * as React from "react";
import ReactDom from "react-dom";
import { BrowserRouter, Route, RouteComponentProps, Switch } from "react-router-dom";
import "ui/styles/reset.less";
import "ui/styles/typography.less";
import { WindowUtils } from "Commons/DomUtils";

import { RemoteTaskQueueApi } from "../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueue";
import { ApiProvider } from "../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueueApiInjection";
import { ServiceHeader } from "../ServiceHeader/components/ServiceHeader";

import { TasksPageContainer } from "./containers/TasksPageContainer";
import { TaskChainsTreeContainer } from "./containers/TaskChainsTreeContainer";
import { TaskDetailsPageContainer } from "./containers/TaskDetailsPageContainer";

const api = new RemoteTaskQueueApi("/internal-api/remote-task-queue/");
const TasksPath = "/AdminTools/Tasks";

function tryGetParentLocationFromHistoryState(location: any): Nullable<LocationDescriptor> {
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

export function RemoteTaskQueueApplication({ match }: RouteComponentProps<any>): JSX.Element {
    const baseUrl = match.url;
    return (
        <ServiceHeader currentInterfaceType={null}>
            <Switch>
                <Route
                    exact
                    path={`${baseUrl}/`}
                    render={({ location }) => <TasksPageContainer searchQuery={location.search} {...location.state} />}
                />
                <Route
                    exact
                    path={`${baseUrl}/Tree`}
                    render={({ location }) => (
                        <TaskChainsTreeContainer
                            searchQuery={location.search}
                            {...location.state}
                            parentLocation={tryGetParentLocationFromHistoryState(location)}
                        />
                    )}
                />
                <Route
                    path={`${baseUrl}/:id`}
                    render={({ location, match: { params } }) => (
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
