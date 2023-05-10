import React from "react";
import { Routes } from "react-router";
import { Route } from "react-router-dom";

import { IRtqMonitoringApi } from "./Domain/Api/RtqMonitoringApi";
import { ICustomRenderer } from "./Domain/CustomRenderer";
import { TaskChainsTreeContainer } from "./containers/TaskChainsTreeContainer";
import { TaskDetailsPageContainer } from "./containers/TaskDetailsPageContainer";
import { TasksPageContainer } from "./containers/TasksPageContainer";

interface RemoteTaskQueueApplicationProps {
    rtqMonitoringApi: IRtqMonitoringApi;
    customRenderer: ICustomRenderer;
    useErrorHandlingContainer: boolean;
    isSuperUser: boolean;
}

export const RemoteTaskQueueApplication = ({
    isSuperUser,
    rtqMonitoringApi,
    customRenderer,
    useErrorHandlingContainer,
}: RemoteTaskQueueApplicationProps): JSX.Element => (
    <Routes>
        <Route
            path="/"
            element={
                <TasksPageContainer
                    isSuperUser={isSuperUser}
                    rtqMonitoringApi={rtqMonitoringApi}
                    useErrorHandlingContainer={useErrorHandlingContainer}
                />
            }
        />
        <Route
            path="/Tree"
            element={
                <TaskChainsTreeContainer
                    rtqMonitoringApi={rtqMonitoringApi}
                    useErrorHandlingContainer={useErrorHandlingContainer}
                />
            }
        />
        <Route
            path=":id"
            element={
                <TaskDetailsPageContainer
                    isSuperUser={isSuperUser}
                    rtqMonitoringApi={rtqMonitoringApi}
                    customRenderer={customRenderer}
                    useErrorHandlingContainer={useErrorHandlingContainer}
                />
            }
        />
    </Routes>
);
