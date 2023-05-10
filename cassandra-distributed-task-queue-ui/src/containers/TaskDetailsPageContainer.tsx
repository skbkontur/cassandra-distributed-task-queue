import { Loader } from "@skbkontur/react-ui";
import React, { useEffect, useState } from "react";
import { useLocation, Location, useParams } from "react-router-dom";

import { IRtqMonitoringApi } from "../Domain/Api/RtqMonitoringApi";
import { RtqMonitoringTaskModel } from "../Domain/Api/RtqMonitoringTaskModel";
import { ICustomRenderer } from "../Domain/CustomRenderer";
import { ErrorHandlingContainer } from "../components/ErrorHandling/ErrorHandlingContainer";
import { TaskDetailsPage } from "../components/TaskDetailsPage/TaskDetailsPage";
import { TaskNotFoundPage } from "../components/TaskNotFoundPage/TaskNotFoundPage";

interface TaskDetailsPageContainerProps {
    rtqMonitoringApi: IRtqMonitoringApi;
    customRenderer: ICustomRenderer;
    isSuperUser: boolean;
    useErrorHandlingContainer: boolean;
}

export const TaskDetailsPageContainer = ({
    rtqMonitoringApi,
    useErrorHandlingContainer,
    isSuperUser,
    customRenderer,
}: TaskDetailsPageContainerProps): JSX.Element => {
    const { pathname, state } = useLocation();
    const parentLocation = tryGetParentLocationFromHistoryState(state);
    const { id = "" } = useParams<"id">();

    const [loading, setLoading] = useState(false);
    const [taskDetails, setTaskDetails] = useState<Nullable<RtqMonitoringTaskModel>>(null);
    const [notFoundError, setNotFoundError] = useState(false);

    useEffect(() => {
        loadData(id);
    }, [id]);

    const getTaskLocation = (id: string): string | Partial<Location> => {
        return {
            pathname: `${pathname}/${id}`,
            state: { parentLocation },
        };
    };

    const handlerRerun = async (): Promise<void> => {
        setLoading(true);
        try {
            await rtqMonitoringApi.rerunTasks([id]);
            const taskDetails = await rtqMonitoringApi.getTaskDetails(id);
            setTaskDetails(taskDetails);
        } finally {
            setLoading(false);
        }
    };

    const handlerCancel = async (): Promise<void> => {
        setLoading(true);
        try {
            await rtqMonitoringApi.cancelTasks([id]);
            const taskDetails = await rtqMonitoringApi.getTaskDetails(id);
            setTaskDetails(taskDetails);
        } finally {
            setLoading(false);
        }
    };

    if (notFoundError) {
        return <TaskNotFoundPage />;
    }

    return (
        <Loader active={loading} type="big" data-tid="Loader">
            {taskDetails && (
                <TaskDetailsPage
                    getTaskLocation={getTaskLocation}
                    parentLocation={parentLocation || pathname}
                    allowRerunOrCancel={isSuperUser}
                    taskDetails={taskDetails}
                    customRenderer={customRenderer}
                    onRerun={handlerRerun}
                    onCancel={handlerCancel}
                    path={pathname}
                />
            )}
            {useErrorHandlingContainer && <ErrorHandlingContainer />}
        </Loader>
    );

    async function loadData(id: string): Promise<void> {
        setLoading(true);
        setNotFoundError(false);
        try {
            const taskDetails = await rtqMonitoringApi.getTaskDetails(id);
            if (taskDetails.taskMeta) {
                setTaskDetails(taskDetails);
                return;
            }
            setNotFoundError(true);
        } finally {
            setLoading(false);
        }
    }
};

function tryGetParentLocationFromHistoryState(state: Nullable<{ parentLocation: string | Location }>): null | string {
    if (!state) {
        return null;
    }
    if (state.parentLocation) {
        const parentLocation = state.parentLocation;
        if (typeof parentLocation === "string") {
            return parentLocation;
        }
        if (typeof parentLocation === "object") {
            const { pathname, search } = parentLocation;
            return `${pathname}${search}`;
        }
    }
    return null;
}
