import { LocationDescriptor } from "history";
import _ from "lodash";
import * as React from "react";
import { RouteComponentProps, withRouter } from "react-router-dom";
import { ErrorHandlingContainer } from "Commons/ErrorHandling";
import { CommonLayout } from "Commons/Layouts";
import { RemoteTaskInfoModel } from "Domain/EDI/Api/RemoteTaskQueue/RemoteTaskInfoModel";
import { IRemoteTaskQueueApi } from "Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueue";
import { RemoteTaskQueueSearchRequest } from "Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueueSearchRequest";
import { TaskStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskState";
import { takeLastAndRejectPrevious } from "PromiseUtils";

import { DelayedLoader } from "../../Commons/DelayedLoader/DelayedLoader";
import { queryStringMapping, QueryStringMapping } from "../../Commons/QueryStringMapping";
import { withRemoteTaskQueueApi } from "../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueueApiInjection";
import {
    createDefaultRemoteTaskQueueSearchRequest,
    isRemoteTaskQueueSearchRequestEmpty,
} from "../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueueSearchRequestUtils";
import { TaskChainTree } from "../components/TaskChainTree/TaskChainTree";

interface TaskChainsTreeContainerProps extends RouteComponentProps<any> {
    searchQuery: string;
    remoteTaskQueueApi: IRemoteTaskQueueApi;
    taskDetails: Nullable<RemoteTaskInfoModel[]>;
    parentLocation: LocationDescriptor;
    searchedQuery: Nullable<string>;
}

interface TaskChainsTreeContainerState {
    loading: boolean;
    loaderText: string;
    request: RemoteTaskQueueSearchRequest;
}

const mapping: QueryStringMapping<RemoteTaskQueueSearchRequest> = queryStringMapping<RemoteTaskQueueSearchRequest>()
    .mapToDateTimeRange(x => x.enqueueDateTimeRange, "enqueue")
    .mapToString(x => x.queryString, "q")
    .mapToStringArray(x => x.names, "types")
    .mapToSet(x => x.states, "states", TaskStates)
    .build();

function isNotNullOrUndefined<T extends Object>(input: null | undefined | T): input is T {
    return input != null;
}

class TaskChainsTreeContainerInternal extends React.Component<
    TaskChainsTreeContainerProps,
    TaskChainsTreeContainerState
> {
    public state: TaskChainsTreeContainerState = {
        loading: false,
        loaderText: "",
        request: createDefaultRemoteTaskQueueSearchRequest(),
    };
    public searchTasks = takeLastAndRejectPrevious(
        this.props.remoteTaskQueueApi.search.bind(this.props.remoteTaskQueueApi)
    );
    public getTaskByIds = takeLastAndRejectPrevious(async (ids: string[]): Promise<RemoteTaskInfoModel[]> => {
        const result = await Promise.all(ids.map(id => this.props.remoteTaskQueueApi.getTaskDetails(id)));
        return result;
    });

    public isSearchRequestEmpty(searchQuery: Nullable<string>): boolean {
        const request = mapping.parse(searchQuery);
        return isRemoteTaskQueueSearchRequestEmpty(request);
    }

    public getRequestBySearchQuery(searchQuery: Nullable<string>): RemoteTaskQueueSearchRequest {
        const request = mapping.parse(searchQuery);
        if (isRemoteTaskQueueSearchRequestEmpty(request)) {
            return createDefaultRemoteTaskQueueSearchRequest();
        }
        return request;
    }

    public componentWillMount() {
        const { searchQuery, searchedQuery } = this.props;
        const request = this.getRequestBySearchQuery(searchQuery);
        if (searchedQuery !== searchQuery) {
            this.setState({ request: request });
            if (!this.isSearchRequestEmpty(searchQuery)) {
                this.loadData(searchQuery, request);
            }
        }
    }

    public componentWillReceiveProps(nextProps: TaskChainsTreeContainerProps) {
        const { searchQuery, taskDetails } = nextProps;
        const request = this.getRequestBySearchQuery(searchQuery);

        this.setState({ request: request });
        if (!this.isSearchRequestEmpty(searchQuery) && !taskDetails) {
            this.loadData(searchQuery, request);
        }
    }

    public getParentAndChildrenTaskIds(taskMetas: RemoteTaskInfoModel[]): string[] {
        const linkedIds = taskMetas
            .map(x => [x.taskMeta.parentTaskId, ...(x.taskMeta.childTaskIds || [])])
            .flatten()
            .filter(isNotNullOrUndefined);
        return _.uniq(linkedIds);
    }

    public async loadData(searchQuery: undefined | string, request: RemoteTaskQueueSearchRequest): Promise<void> {
        const { history } = this.props;
        let iterationCount = 0;

        this.setState({ loading: true, loaderText: "Загрузка задач: 0" });
        try {
            let taskDetails: RemoteTaskInfoModel[] = [];
            let allTaskIds: string[] = [];
            const results = await this.searchTasks(request, 0, 100);
            let taskIdsToLoad = results.taskMetas.map((x: any) => x.id);
            while (taskIdsToLoad.length > 0) {
                iterationCount++;
                if (taskIdsToLoad.length > 100) {
                    throw new Error("Количство задач в дереве превысило допустимый предел: 100 зачад");
                }
                const loadedtaskDetails = await this.getTaskByIds(taskIdsToLoad);
                allTaskIds = [...allTaskIds, ...taskIdsToLoad];
                this.setState({ loading: true, loaderText: `Загрузка задач: ${taskDetails.length}` });
                const parentAndChildrenTaskIds = this.getParentAndChildrenTaskIds(loadedtaskDetails);
                taskIdsToLoad = _.difference(parentAndChildrenTaskIds, allTaskIds);
                taskDetails = [...taskDetails, ...loadedtaskDetails];
                if (iterationCount > 50) {
                    break;
                }
            }
            history.replace({
                pathname: "/AdminTools/Tasks/Tree",
                search: searchQuery,
                state: {
                    taskDetails: taskDetails,
                    searchedQuery: searchQuery,
                },
            });
        } finally {
            this.setState({ loading: false });
        }
    }

    public getTaskLocation(id: string): LocationDescriptor {
        return { pathname: `/AdminTools/Tasks/${id}` };
    }

    public render(): JSX.Element {
        const { taskDetails, searchQuery, parentLocation } = this.props;
        const { loaderText, loading } = this.state;
        return (
            <CommonLayout>
                <CommonLayout.GoBack
                    data-tid={"GoBack"}
                    to={
                        parentLocation || {
                            pathname: "/AdminTools/Tasks",
                            search: searchQuery,
                        }
                    }>
                    Вернуться к поиску задач
                </CommonLayout.GoBack>
                <CommonLayout.Header title="Дерево задач" />
                <CommonLayout.Content>
                    <DelayedLoader type="big" active={loading} caption={loaderText}>
                        <div style={{ overflowX: "auto" }}>
                            {taskDetails && (
                                <TaskChainTree
                                    getTaskLocation={id => this.getTaskLocation(id)}
                                    taskMetas={taskDetails.map(x => x.taskMeta)}
                                />
                            )}
                        </div>
                    </DelayedLoader>
                    <ErrorHandlingContainer />
                </CommonLayout.Content>
            </CommonLayout>
        );
    }
}

export const TaskChainsTreeContainer = withRouter(withRemoteTaskQueueApi(TaskChainsTreeContainerInternal));
