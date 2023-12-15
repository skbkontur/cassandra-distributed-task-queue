import type { JSX } from "react";
import { Routes, Route } from "react-router-dom";

import { CustomSettingsProvider, TaskStateDict } from "./CustomSettingsContext";
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
    customStateCaptions?: TaskStateDict;
    customSearchHelp?: JSX.Element;
    hideMissingMeta?: boolean;
    useFrontPaging?: boolean;
}

export const RemoteTaskQueueApplication = ({
    isSuperUser,
    rtqMonitoringApi,
    useErrorHandlingContainer,
    customRenderer,
    customStateCaptions,
    customSearchHelp,
    hideMissingMeta,
    useFrontPaging,
}: RemoteTaskQueueApplicationProps): JSX.Element => (
    <CustomSettingsProvider
        customStateCaptions={customStateCaptions}
        customSearchHelp={customSearchHelp}
        customDetailRenderer={customRenderer}
        hideMissingMeta={hideMissingMeta}>
        <Routes>
            <Route
                path="/"
                element={
                    <TasksPageContainer
                        isSuperUser={isSuperUser}
                        rtqMonitoringApi={rtqMonitoringApi}
                        useErrorHandlingContainer={useErrorHandlingContainer}
                        useFrontPaging={useFrontPaging}
                    />
                }
            />
            <Route
                path="Tree"
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
                        useErrorHandlingContainer={useErrorHandlingContainer}
                    />
                }
            />
        </Routes>
    </CustomSettingsProvider>
);
