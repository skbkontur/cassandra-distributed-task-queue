import { RowStack } from "@skbkontur/react-stack-layout";
import { Loader } from "@skbkontur/react-ui";
import { LocationDescriptor } from "history";
import _ from "lodash";
import React from "react";

import { IRtqMonitoringApi } from "../Domain/Api/RtqMonitoringApi";
import { RtqMonitoringSearchRequest } from "../Domain/Api/RtqMonitoringSearchRequest";
import { RtqMonitoringTaskModel } from "../Domain/Api/RtqMonitoringTaskModel";
import {
    createDefaultRemoteTaskQueueSearchRequest,
    isRemoteTaskQueueSearchRequestEmpty,
} from "../Domain/RtqMonitoringSearchRequestUtils";
import { ErrorHandlingContainer } from "../components/ErrorHandling/ErrorHandlingContainer";
import { GoBackLink } from "../components/GoBack/GoBackLink";
import { CommonLayout } from "../components/Layouts/CommonLayout";
import { TaskChainTree } from "../components/TaskChainTree/TaskChainTree";

import { searchRequestMapping } from "./TasksPageContainer";

interface TaskChainsTreeContainerProps {
    searchQuery: string;
    rtqMonitoringApi: IRtqMonitoringApi;
    path: string;
    useErrorHandlingContainer: boolean;
}

interface TaskChainsTreeContainerState {
    loading: boolean;
    loaderText: string;
    request: RtqMonitoringSearchRequest;
    taskDetails: RtqMonitoringTaskModel[];
}

function isNotNullOrUndefined<T>(input: null | undefined | T): input is T {
    return input != null;
}

export class TaskChainsTreeContainer extends React.Component<
    TaskChainsTreeContainerProps,
    TaskChainsTreeContainerState
> {
    public state: TaskChainsTreeContainerState = {
        loading: false,
        loaderText: "",
        request: createDefaultRemoteTaskQueueSearchRequest(),
        taskDetails: [],
    };

    public isSearchRequestEmpty(searchQuery: Nullable<string>): boolean {
        const request = searchRequestMapping.parse(searchQuery);
        return isRemoteTaskQueueSearchRequestEmpty(request);
    }

    public getRequestBySearchQuery(searchQuery: Nullable<string>): RtqMonitoringSearchRequest {
        const request = searchRequestMapping.parse(searchQuery);
        if (isRemoteTaskQueueSearchRequestEmpty(request)) {
            return createDefaultRemoteTaskQueueSearchRequest();
        }
        return request;
    }

    public componentDidMount(): void {
        const { searchQuery } = this.props;
        const request = this.getRequestBySearchQuery(searchQuery);
        this.setState({ request: request });
        if (!this.isSearchRequestEmpty(searchQuery)) {
            this.loadData(searchQuery, request);
        }
    }

    public componentDidUpdate(prevProps: TaskChainsTreeContainerProps): void {
        if (prevProps.searchQuery !== this.props.searchQuery) {
            const { searchQuery } = this.props;
            const request = this.getRequestBySearchQuery(searchQuery);
            this.setState({ request: request });
            if (!this.isSearchRequestEmpty(searchQuery)) {
                this.loadData(searchQuery, request);
            }
        }
    }

    public getParentAndChildrenTaskIds(taskDetails: RtqMonitoringTaskModel[]): string[] {
        const linkedIds = taskDetails
            .map(x => [x.taskMeta.parentTaskId, ...(x.childTaskIds || [])])
            .flat()
            .filter(isNotNullOrUndefined);
        return _.uniq(linkedIds);
    }

    public async loadData(searchQuery: undefined | string, request: RtqMonitoringSearchRequest): Promise<void> {
        const { rtqMonitoringApi } = this.props;
        let iterationCount = 0;

        this.setState({ loading: true, loaderText: "Загрузка задач: 0" });
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
                this.setState({ loading: true, loaderText: `Загрузка задач: ${taskDetails.length}` });
                const parentAndChildrenTaskIds = this.getParentAndChildrenTaskIds(loadedTaskDetails);
                taskIdsToLoad = _.difference(parentAndChildrenTaskIds, allTaskIds);
                taskDetails = [...taskDetails, ...loadedTaskDetails];
                if (iterationCount > 50) {
                    break;
                }
            }
            this.setState({ taskDetails: taskDetails });
        } finally {
            this.setState({ loading: false });
        }
    }

    public getTaskLocation(id: string): LocationDescriptor {
        return { pathname: `${this.props.path}/${id}` };
    }

    public render(): JSX.Element {
        const { useErrorHandlingContainer } = this.props;
        const { loaderText, loading, taskDetails } = this.state;
        return (
            <CommonLayout>
                <CommonLayout.Header
                    title={
                        <RowStack gap={3} verticalAlign="bottom">
                            <GoBackLink />
                            <span>Дерево задач</span>
                        </RowStack>
                    }
                />
                <CommonLayout.Content>
                    <Loader type="big" active={loading} caption={loaderText}>
                        <div style={{ overflowX: "auto" }}>
                            {taskDetails && (
                                <TaskChainTree
                                    getTaskLocation={id => this.getTaskLocation(id)}
                                    taskDetails={taskDetails}
                                />
                            )}
                        </div>
                    </Loader>
                    {useErrorHandlingContainer && <ErrorHandlingContainer />}
                </CommonLayout.Content>
            </CommonLayout>
        );
    }
}
