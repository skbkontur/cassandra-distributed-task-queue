import React from "react";
import { Route, RouteComponentProps, Switch, withRouter } from "react-router-dom";

import { IRtqMonitoringApi } from "./Domain/Api/RtqMonitoringApi";
import { ICustomRenderer } from "./Domain/CustomRenderer";
import { TaskChainsTreeContainer } from "./containers/TaskChainsTreeContainer";
import { TaskDetailsPageContainer } from "./containers/TaskDetailsPageContainer";
import { TasksPageContainer } from "./containers/TasksPageContainer";

interface RemoteTaskQueueApplicationProps extends RouteComponentProps {
    rtqMonitoringApi: IRtqMonitoringApi;
    customRenderer: ICustomRenderer;
    useErrorHandlingContainer: boolean;
    isSuperUser: boolean;
}

function tryGetParentLocationFromHistoryState(location: any): null | string {
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
                return `${pathname}${search}`;
            }
        }
    }
    return null;
}

export function RemoteTaskQueueApplicationInternal({
    match,
    isSuperUser,
    rtqMonitoringApi,
    customRenderer,
    useErrorHandlingContainer,
}: RemoteTaskQueueApplicationProps): JSX.Element {
    return (
        <Switch>
            <Route
                exact
                path={`${match.url}/`}
                render={({ location }) => (
                    <TasksPageContainer
                        isSuperUser={isSuperUser}
                        rtqMonitoringApi={rtqMonitoringApi}
                        searchQuery={location.search || ""}
                        path={match.url}
                        useErrorHandlingContainer={useErrorHandlingContainer}
                    />
                )}
            />
            <Route
                exact
                path={`${match.url}/Tree`}
                render={({ location }) => (
                    <TaskChainsTreeContainer
                        rtqMonitoringApi={rtqMonitoringApi}
                        searchQuery={location.search}
                        path={match.url}
                        useErrorHandlingContainer={useErrorHandlingContainer}
                    />
                )}
            />
            <Route
                path={`${match.url}/:id`}
                render={({ location, match: { params } }) => (
                    <TaskDetailsPageContainer
                        isSuperUser={isSuperUser}
                        rtqMonitoringApi={rtqMonitoringApi}
                        path={match.url}
                        id={params.id || ""}
                        parentLocation={tryGetParentLocationFromHistoryState(location)}
                        customRenderer={customRenderer}
                        useErrorHandlingContainer={useErrorHandlingContainer}
                    />
                )}
            />
        </Switch>
    );
}

export const RemoteTaskQueueApplication = withRouter(RemoteTaskQueueApplicationInternal);
