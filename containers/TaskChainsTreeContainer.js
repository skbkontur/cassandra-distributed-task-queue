// @flow
import * as React from "react";
import _ from "lodash";
import { withRouter } from "react-router";
import type { ReactRouter } from "react-router";
import DelayedLoader from "../../Commons/DelayedLoader/DelayedLoader";
import { withRemoteTaskQueueApi } from "../api/RemoteTaskQueueApiInjection";
import { takeLastAndRejectPrevious } from "PromiseUtils";
import {
    createDefaultRemoteTaskQueueSearchRequest,
    isRemoteTaskQueueSearchRequestEmpty,
} from "../api/RemoteTaskQueueApi";
import { TaskStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskState";
import { queryStringMapping } from "../../Commons/QueryStringMapping";
import CommonLayout from "../../Commons/Layouts";
import type { RemoteTaskQueueSearchRequest, RemoteTaskInfoModel } from "../api/RemoteTaskQueueApi";
import type { IRemoteTaskQueueApi } from "../api/RemoteTaskQueueApi";
import type { QueryStringMapping } from "../../Commons/QueryStringMapping";
import type { RouterLocationDescriptor } from "../../Commons/DataTypes/Routing";
import TaskChainTree from "../components/TaskChainTree/TaskChainTree";
import { ErrorHandlingContainer } from "Commons/ErrorHandling";

type TaskChainsTreeContainerProps = {
    searchQuery: string,
    router: ReactRouter,
    remoteTaskQueueApi: IRemoteTaskQueueApi,
    taskDetails: ?(RemoteTaskInfoModel[]),
    parentLocation: RouterLocationDescriptor,
};

type TaskChainsTreeContainerState = {
    loading: boolean,
    loaderText: string,
    request: RemoteTaskQueueSearchRequest,
};

const mapping: QueryStringMapping<RemoteTaskQueueSearchRequest> = queryStringMapping()
    .mapToDateTimeRange(x => x.enqueueDateTimeRange, "enqueue")
    .mapToString(x => x.queryString, "q")
    .mapToStringArray(x => x.names, "types")
    .mapToSet(x => x.states, "states", TaskStates)
    .build();

class TaskChainsTreeContainer extends React.Component<TaskChainsTreeContainerProps, TaskChainsTreeContainerState> {
    state: TaskChainsTreeContainerState = {
        loading: false,
        loaderText: "",
        request: createDefaultRemoteTaskQueueSearchRequest(),
    };
    searchTasks = takeLastAndRejectPrevious(this.props.remoteTaskQueueApi.search.bind(this.props.remoteTaskQueueApi));
    getTaskByIds = takeLastAndRejectPrevious(async (ids: string[]): Promise<RemoteTaskInfoModel[]> => {
        const result = await Promise.all(ids.map(id => this.props.remoteTaskQueueApi.getTaskDetails(id)));
        return result;
    });

    isSearchRequestEmpty(searchQuery: ?string): boolean {
        const request = mapping.parse(searchQuery);
        return isRemoteTaskQueueSearchRequestEmpty(request);
    }

    getRequestBySearchQuery(searchQuery: ?string): RemoteTaskQueueSearchRequest {
        const request = mapping.parse(searchQuery);
        if (isRemoteTaskQueueSearchRequestEmpty(request)) {
            return createDefaultRemoteTaskQueueSearchRequest();
        }
        return request;
    }

    componentWillMount() {
        const { searchQuery } = this.props;
        const request = this.getRequestBySearchQuery(searchQuery);

        this.setState({ request: request });
        if (!this.isSearchRequestEmpty(searchQuery)) {
            this.loadData(searchQuery, request);
        }
    }

    componentWillReceiveProps(nextProps: TaskChainsTreeContainerProps) {
        const { searchQuery, taskDetails } = nextProps;
        const request = this.getRequestBySearchQuery(searchQuery);

        this.setState({ request: request });
        if (!this.isSearchRequestEmpty(searchQuery) && !taskDetails) {
            this.loadData(searchQuery, request);
        }
    }

    getParentAndChildrenTaskIds(taskMetas: RemoteTaskInfoModel[]): string[] {
        const linkedIds = taskMetas
            .map(x => [x.taskMeta.parentTaskId, ...(x.taskMeta.childTaskIds || [])])
            .reduce((result, x) => [...result, ...x], [])
            .filter(Boolean);
        return _.uniq(linkedIds);
    }

    async loadData(searchQuery: ?string, request: RemoteTaskQueueSearchRequest): Promise<void> {
        const { router } = this.props;
        let iterationCount = 0;

        this.setState({ loading: true, loaderText: "Загрузка задач: 0" });
        try {
            let taskDetails = [];
            let allTaskIds = [];
            const results = await this.searchTasks(request, 0, 100);
            let taskIdsToLoad = results.taskMetas.map(x => x.id);
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
            router.replace({
                pathname: "/AdminTools/Tasks/Tree",
                search: searchQuery,
                state: {
                    taskDetails: taskDetails,
                },
            });
        } finally {
            this.setState({ loading: false });
        }
    }

    getTaskLocation(id: string): RouterLocationDescriptor {
        return { pathname: `/AdminTools/Tasks/${id}` };
    }

    render(): React.Node {
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

export default withRouter(withRemoteTaskQueueApi(TaskChainsTreeContainer));
