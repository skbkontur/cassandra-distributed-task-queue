import { RowStack } from "@skbkontur/react-stack-layout";
import { Loader } from "@skbkontur/react-ui";
import _ from "lodash";
import React, { useEffect, useState } from "react";
import { Location, useLocation } from "react-router-dom";

import { IRtqMonitoringApi } from "../Domain/Api/RtqMonitoringApi";
import { RtqMonitoringSearchRequest } from "../Domain/Api/RtqMonitoringSearchRequest";
import { RtqMonitoringTaskModel } from "../Domain/Api/RtqMonitoringTaskModel";
import {
    createDefaultRemoteTaskQueueSearchRequest,
    isRemoteTaskQueueSearchRequestEmpty,
} from "../Domain/RtqMonitoringSearchRequestUtils";
import { RouteUtils } from "../Domain/Utils/RouteUtils";
import { ErrorHandlingContainer } from "../components/ErrorHandling/ErrorHandlingContainer";
import { GoBackLink } from "../components/GoBack/GoBackLink";
import { CommonLayout } from "../components/Layouts/CommonLayout";
import { TaskChainTree } from "../components/TaskChainTree/TaskChainTree";

import { searchRequestMapping } from "./TasksPageContainer";

interface TaskChainsTreeContainerProps {
    rtqMonitoringApi: IRtqMonitoringApi;
    useErrorHandlingContainer: boolean;
}

const isNotNullOrUndefined = <T extends {}>(input: null | undefined | T): input is T => Boolean(input);

export const TaskChainsTreeContainer = ({
    useErrorHandlingContainer,
    rtqMonitoringApi,
}: TaskChainsTreeContainerProps): JSX.Element => {
    const { search, pathname } = useLocation();
    const [loading, setLoading] = useState(false);
    const [loaderText, setLoaderText] = useState("");
    const [taskDetails, setTaskDetails] = useState<RtqMonitoringTaskModel[]>([]);

    useEffect(() => {
        const request = getRequestBySearchQuery(search);
        if (!isSearchRequestEmpty(search)) {
            loadData(search, request);
        }
    }, [search]);

    const isSearchRequestEmpty = (searchQuery: Nullable<string>): boolean => {
        const request = searchRequestMapping.parse(searchQuery);
        return isRemoteTaskQueueSearchRequestEmpty(request);
    };

    const getRequestBySearchQuery = (searchQuery: Nullable<string>): RtqMonitoringSearchRequest => {
        const request = searchRequestMapping.parse(searchQuery);
        if (isRemoteTaskQueueSearchRequestEmpty(request)) {
            return createDefaultRemoteTaskQueueSearchRequest();
        }
        return request;
    };

    const getParentAndChildrenTaskIds = (taskDetails: RtqMonitoringTaskModel[]): string[] => {
        const linkedIds = taskDetails
            .map(({ childTaskIds, taskMeta: { parentTaskId } }) => [parentTaskId, ...(childTaskIds || [])])
            .flat()
            .filter(isNotNullOrUndefined);
        return _.uniq(linkedIds);
    };

    const getTaskLocation = (id: string): string | Partial<Location> => ({ pathname: `../${id}` });

    return (
        <CommonLayout>
            <CommonLayout.Header
                title={
                    <RowStack gap={3} verticalAlign="bottom">
                        <GoBackLink backUrl={`${RouteUtils.backUrl(pathname)}${search}`} />
                        <span>Дерево задач</span>
                    </RowStack>
                }
            />
            <CommonLayout.Content>
                <Loader type="big" active={loading} caption={loaderText}>
                    <div style={{ overflowX: "auto" }}>
                        {taskDetails && <TaskChainTree getTaskLocation={getTaskLocation} taskDetails={taskDetails} />}
                    </div>
                </Loader>
                {useErrorHandlingContainer && <ErrorHandlingContainer />}
            </CommonLayout.Content>
        </CommonLayout>
    );

    async function loadData(searchQuery: undefined | string, request: RtqMonitoringSearchRequest): Promise<void> {
        let iterationCount = 0;
        setLoading(true);
        setLoaderText("Загрузка задач: 0");
        try {
            let taskDetails: RtqMonitoringTaskModel[] = [];
            let allTaskIds: string[] = [];
            const results = await rtqMonitoringApi.search(request);
            let taskIdsToLoad = results.taskMetas.map(x => x.id);
            while (taskIdsToLoad.length > 0) {
                iterationCount++;
                if (taskIdsToLoad.length > 100) {
                    throw new Error("Количство задач в дереве превысило допустимый предел: 100 зачад");
                }
                const loadedTaskDetails = await Promise.all(
                    taskIdsToLoad.map(id => rtqMonitoringApi.getTaskDetails(id))
                );
                allTaskIds = [...allTaskIds, ...taskIdsToLoad];
                setLoading(true);
                setLoaderText(`Загрузка задач: ${taskDetails.length}`);
                const parentAndChildrenTaskIds = getParentAndChildrenTaskIds(loadedTaskDetails);
                taskIdsToLoad = _.difference(parentAndChildrenTaskIds, allTaskIds);
                taskDetails = [...taskDetails, ...loadedTaskDetails];
                if (iterationCount > 50) {
                    break;
                }
            }
            setTaskDetails(taskDetails);
        } finally {
            setLoading(false);
        }
    }
};
