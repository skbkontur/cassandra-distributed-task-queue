import * as React from "react";
import $c from "property-chain";
import _ from "lodash";
import { Modal, ModalHeader, ModalBody, ModalFooter, Input, Button } from "ui";
import { RowStack, ColumnStack, Fit } from "ui/layout";
import { withRouter } from "react-router-dom";
import { Loader } from "ui";
import { takeLastAndRejectPrevious } from "PromiseUtils";
import { SuperUserAccessLevels } from "Domain/Globals";
import { getCurrentUserInfo } from "Domain/Globals";
import { TaskStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskState";
import { SearchQuery, queryStringMapping } from "Commons/QueryStringMapping";
import CommonLayout from "Commons/Layouts";
import { QueryStringMapping } from "Commons/QueryStringMapping";
import { Location, LocationDescriptor, LocationDescriptorObject } from "history";
import { ErrorHandlingContainer } from "Commons/ErrorHandling";

import { IRemoteTaskQueueApi } from "../api/RemoteTaskQueueApi";
import { RemoteTaskQueueSearchRequest, RemoteTaskQueueSearchResults } from "../api/RemoteTaskQueueApi";
import {
    createDefaultRemoteTaskQueueSearchRequest,
    isRemoteTaskQueueSearchRequestEmpty,
} from "../api/RemoteTaskQueueApi";
import { withRemoteTaskQueueApi } from "../api/RemoteTaskQueueApiInjection";
import TaskQueueFilter from "../components/TaskQueueFilter/TaskQueueFilter";
import TasksPaginator from "../components/TasksPaginator/TasksPaginator";
import TasksTable from "../components/TaskTable/TaskTable";
import numberToString from "../Domain/numberToString";
import { RouteComponentProps } from "react-router-dom";

interface TasksPageContainerProps extends RouteComponentProps<any> {
    searchQuery: string;
    remoteTaskQueueApi: IRemoteTaskQueueApi;
    results: Nullable<RemoteTaskQueueSearchResults>;
    requestParams: Nullable<string>;
}

type TasksPageContainerState = {
    loading: boolean;
    request: RemoteTaskQueueSearchRequest;
    availableTaskNames: string[] | null;
    confirmMultipleModalOpened: boolean;
    modalType: "Rerun" | "Cancel";
    manyTaskConfirm: string;
    searchRequested: boolean;
};

const provisionalMapping: QueryStringMapping<RemoteTaskQueueSearchRequest> = queryStringMapping<
    RemoteTaskQueueSearchRequest
>()
    .mapToDateTimeRange(x => x.enqueueDateTimeRange, "enqueue")
    .mapToString(x => x.queryString, "q")
    .mapToStringArray(x => x.names, "types")
    .mapToSet(x => x.states, "states", TaskStates)
    .build();

function createSearchRequestMapping(availableTaskNames: string[]): QueryStringMapping<RemoteTaskQueueSearchRequest> {
    const availableTaskNamesMap = availableTaskNames.reduce((result, name) => {
        result[name] = name;
        return result;
    }, {});
    return queryStringMapping<RemoteTaskQueueSearchRequest>()
        .mapToDateTimeRange(x => x.enqueueDateTimeRange, "enqueue")
        .mapToString(x => x.queryString, "q")
        .mapToSet(x => x.names, "types", availableTaskNamesMap, true)
        .mapToSet(x => x.states, "states", TaskStates)
        .build();
}

const pagingMapping: QueryStringMapping<{ from: Nullable<number>; size: Nullable<number> }> = queryStringMapping<{
    from: Nullable<number>;
    size: Nullable<number>;
}>()
    .mapToInteger(x => x.from, "from")
    .mapToInteger(x => x.size, "size")
    .build();

export function buildSearchQueryForRequest(request: RemoteTaskQueueSearchRequest): string {
    if (request.names && request.names.length > 0) {
        throw new Error("Cannot build search request with names.");
    }
    return provisionalMapping.stringify(request);
}

class TasksPageContainer extends React.Component<TasksPageContainerProps, TasksPageContainerState> {
    state: TasksPageContainerState = {
        loading: false,
        request: createDefaultRemoteTaskQueueSearchRequest(),
        availableTaskNames: null,
        confirmMultipleModalOpened: false,
        modalType: "Rerun",
        manyTaskConfirm: "",
        searchRequested: false,
    };
    searchTasks = takeLastAndRejectPrevious(this.props.remoteTaskQueueApi.search.bind(this.props.remoteTaskQueueApi));

    isSearchRequestEmpty(searchQuery: Nullable<string>): boolean {
        const request = provisionalMapping.parse(searchQuery);
        return isRemoteTaskQueueSearchRequestEmpty(request);
    }

    getSearchRequestMapping(): QueryStringMapping<RemoteTaskQueueSearchRequest> {
        const { availableTaskNames } = this.state;
        if (!availableTaskNames) {
            throw new Error("InvalidProgramState");
        }
        return createSearchRequestMapping(availableTaskNames);
    }

    getRequestBySearchQuery(searchQuery: Nullable<string>): RemoteTaskQueueSearchRequest {
        const request = this.getSearchRequestMapping().parse(searchQuery);
        if (isRemoteTaskQueueSearchRequestEmpty(request)) {
            return createDefaultRemoteTaskQueueSearchRequest();
        }
        return request;
    }

    async componentWillMount() {
        const { searchQuery, results, requestParams } = this.props;
        await this.updateAvailableTaskNamesIfNeed();

        const request = this.getRequestBySearchQuery(searchQuery);
        this.setState({ request: request });

        if ((requestParams !== searchQuery || !results) && !this.isSearchRequestEmpty(searchQuery)) {
            this.loadData(searchQuery, request);
        }
    }

    async updateAvailableTaskNamesIfNeed(): Promise<void> {
        if (this.state.availableTaskNames === null) {
            const availableTaskNames = await this.props.remoteTaskQueueApi.getAllTaskNames();
            this.setState({ availableTaskNames: availableTaskNames });
        }
    }

    async componentWillReceiveProps(nextProps: TasksPageContainerProps) {
        const { searchQuery, results } = nextProps;
        const prevPaging = pagingMapping.parse(this.props.searchQuery);
        const nextPaging = pagingMapping.parse(searchQuery);

        await this.updateAvailableTaskNamesIfNeed();
        const request = this.getRequestBySearchQuery(searchQuery);

        this.setState({ request: request });

        if (
            (this.state.searchRequested && !this.isSearchRequestEmpty(searchQuery) && !results) ||
            !_.isEqual(prevPaging, nextPaging)
        ) {
            this.loadData(searchQuery, request);
        }
    }

    async loadData(searchQuery: Nullable<string>, request: RemoteTaskQueueSearchRequest): Promise<void> {
        const { from, size } = pagingMapping.parse(searchQuery);
        const { history } = this.props;
        this.setState({ loading: true });
        try {
            const results = await this.searchTasks(request, from || 0, size || 20);
            history.replace({
                pathname: "/AdminTools/Tasks",
                search: searchQuery,
                state: {
                    requestParams: searchQuery,
                    results: results,
                },
            } as LocationDescriptorObject);
        } finally {
            this.setState({ loading: false, searchRequested: false });
        }
    }

    handleSearch() {
        const { history } = this.props;
        const { request } = this.state;
        this.setState({ searchRequested: true }, () => {
            history.push({
                pathname: "/AdminTools/Tasks",
                search: SearchQuery.combine(
                    this.getSearchRequestMapping().stringify(request),
                    pagingMapping.stringify({ from: 0, size: 20 })
                ),
                state: null,
            } as LocationDescriptorObject);
        });
    }

    getTaskLocation(id: string): LocationDescriptor {
        const { results, searchQuery } = this.props;
        const { request } = this.state;
        const { from, size } = pagingMapping.parse(searchQuery);

        return {
            pathname: `/AdminTools/Tasks/${id}`,
            state: {
                parentLocation: {
                    pathname: "/AdminTools/Tasks",
                    search: SearchQuery.combine(
                        this.getSearchRequestMapping().stringify(request),
                        pagingMapping.stringify({ from: from, size: size })
                    ),
                    state: {
                        results: results,
                    },
                },
            },
        };
    }

    getNextPageLocation(): LocationDescriptor | null {
        const { searchQuery, results } = this.props;
        const { from, size } = pagingMapping.parse(searchQuery);
        const request = this.getRequestBySearchQuery(searchQuery);

        if (!results) {
            return null;
        }
        if ((from || 0) + (size || 0) >= results.totalCount) {
            return null;
        }
        return {
            pathname: "/AdminTools/Tasks",
            search: SearchQuery.combine(
                this.getSearchRequestMapping().stringify(request),
                pagingMapping.stringify({ from: (from || 0) + (size || 20), size: size || 20 })
            ),
        } as LocationDescriptorObject;
    }

    getPrevPageLocation(): LocationDescriptor | null {
        const { searchQuery } = this.props;
        const { from, size } = pagingMapping.parse(searchQuery);
        const request = this.getRequestBySearchQuery(searchQuery);

        if ((from || 0) === 0) {
            return null;
        }
        return {
            pathname: "/AdminTools/Tasks",
            search: SearchQuery.combine(
                this.getSearchRequestMapping().stringify(request),
                pagingMapping.stringify({ from: Math.max(0, (from || 0) - (size || 20)), size: size || 20 })
            ),
        } as LocationDescriptorObject;
    }

    async handleRerunTask(id: string): Promise<void> {
        const { remoteTaskQueueApi } = this.props;
        this.setState({ loading: true });
        try {
            await remoteTaskQueueApi.rerunTasks([id]);
        } finally {
            this.setState({ loading: false });
        }
    }

    async handleCancelTask(id: string): Promise<void> {
        const { remoteTaskQueueApi } = this.props;
        this.setState({ loading: true });
        try {
            await remoteTaskQueueApi.cancelTasks([id]);
        } finally {
            this.setState({ loading: false });
        }
    }

    async handleRerunAll(): Promise<void> {
        const { searchQuery, remoteTaskQueueApi } = this.props;
        const request = this.getRequestBySearchQuery(searchQuery);

        this.setState({ loading: true });
        try {
            await remoteTaskQueueApi.rerunTasksBySearchQuery(request);
        } finally {
            this.setState({ loading: false });
        }
    }

    async handleCancelAll(): Promise<void> {
        const { searchQuery, remoteTaskQueueApi } = this.props;
        const request = this.getRequestBySearchQuery(searchQuery);

        this.setState({ loading: true });
        try {
            await remoteTaskQueueApi.cancelTasksBySearchQuery(request);
        } finally {
            this.setState({ loading: false });
        }
    }

    renderModal(): JSX.Element {
        const { results } = this.props;
        const { modalType, manyTaskConfirm } = this.state;
        const confirmedRegExp = /б.*л.*я/i;
        const counter = (results && results.totalCount) || 0;

        return (
            <Modal onClose={() => this.closeModal()} width={500} data-tid="ConfirmMultipleOperationModal">
                <ModalHeader>Нужно подтверждение</ModalHeader>
                <ModalBody>
                    <ColumnStack gap={2}>
                        <Fit>
                            <span data-tid="ModalText">
                                {modalType === "Rerun"
                                    ? "Уверен, что все эти таски надо перезапустить?"
                                    : "Уверен, что все эти таски надо остановить?"}
                            </span>
                        </Fit>
                        {counter > 100 && [
                            <Fit key="text">
                                Это действие может задеть больше 100 тасок, если это точно надо сделать, то напиши
                                прописью количество тасок (их {counter}):
                            </Fit>,
                            <Fit key="input">
                                <Input
                                    data-tid="ConfirmationInput"
                                    value={manyTaskConfirm}
                                    onChange={(e, val) => this.setState({ manyTaskConfirm: val })}
                                />
                            </Fit>,
                        ]}
                    </ColumnStack>
                </ModalBody>
                <ModalFooter>
                    <RowStack gap={2}>
                        <Fit>
                            {modalType === "Rerun" ? (
                                <Button
                                    data-tid="RerunButton"
                                    use="success"
                                    disabled={
                                        counter > 100 &&
                                        (!confirmedRegExp.test(manyTaskConfirm) &&
                                            manyTaskConfirm !== numberToString(counter))
                                    }
                                    onClick={() => {
                                        this.handleRerunAll();
                                        this.closeModal();
                                    }}>
                                    Перезапустить все
                                </Button>
                            ) : (
                                <Button
                                    data-tid="CancelButton"
                                    use="danger"
                                    disabled={
                                        counter > 100 &&
                                        (!confirmedRegExp.test(manyTaskConfirm) &&
                                            manyTaskConfirm !== numberToString(counter))
                                    }
                                    onClick={() => {
                                        this.handleCancelAll();
                                        this.closeModal();
                                    }}>
                                    Остановить все
                                </Button>
                            )}
                        </Fit>
                        <Fit>
                            <Button data-tid="CloseButton" onClick={() => this.closeModal()}>
                                Закрыть
                            </Button>
                        </Fit>
                    </RowStack>
                </ModalFooter>
            </Modal>
        );
    }

    clickRerunAll() {
        this.setState({
            confirmMultipleModalOpened: true,
            modalType: "Rerun",
        });
    }

    clickCancelAll() {
        this.setState({
            confirmMultipleModalOpened: true,
            modalType: "Cancel",
        });
    }

    closeModal() {
        this.setState({
            confirmMultipleModalOpened: false,
        });
    }

    render(): JSX.Element {
        const currentUser = getCurrentUserInfo();
        const allowRerunOrCancel = $c(currentUser)
            .with(x => x.superUserAccessLevel)
            .with(x => [SuperUserAccessLevels.God, SuperUserAccessLevels.Developer].includes(x))
            .return(false);

        const { availableTaskNames, request, loading } = this.state;
        const { results } = this.props;
        const isStateCompletelyLoaded = results && availableTaskNames;
        const counter = (results && results.totalCount) || 0;

        return (
            <CommonLayout>
                <CommonLayout.GoBack href="/AdminTools">Вернуться к инструментам администратора</CommonLayout.GoBack>
                <CommonLayout.Header data-tid="Header" title="Список задач" />
                <CommonLayout.Content>
                    <ErrorHandlingContainer />
                    <ColumnStack block stretch gap={2}>
                        <Fit>
                            <TaskQueueFilter
                                value={request}
                                availableTaskTypes={availableTaskNames}
                                onChange={value => this.setState({ request: { ...this.state.request, ...value } })}
                                onSearchButtonClick={() => this.handleSearch()}
                            />
                        </Fit>
                        <Fit>
                            <Loader type="big" active={loading} data-tid={"Loader"}>
                                {results &&
                                    isStateCompletelyLoaded && (
                                        <ColumnStack block stretch gap={2}>
                                            {counter > 0 && <Fit>Всего результатов: {counter}</Fit>}
                                            {counter > 0 &&
                                                allowRerunOrCancel && (
                                                    <Fit>
                                                        <RowStack gap={2} data-tid={"ButtonsWrapper"}>
                                                            <Fit>
                                                                <Button
                                                                    use="danger"
                                                                    data-tid={"CancelAllButton"}
                                                                    onClick={() => this.clickCancelAll()}>
                                                                    Cancel All
                                                                </Button>
                                                            </Fit>
                                                            <Fit>
                                                                <Button
                                                                    use="success"
                                                                    data-tid={"RerunAllButton"}
                                                                    onClick={() => this.clickRerunAll()}>
                                                                    Rerun All
                                                                </Button>
                                                            </Fit>
                                                        </RowStack>
                                                    </Fit>
                                                )}
                                            <Fit>
                                                <TasksTable
                                                    getTaskLocation={id => this.getTaskLocation(id)}
                                                    allowRerunOrCancel={allowRerunOrCancel}
                                                    taskInfos={results.taskMetas}
                                                    onRerun={id => {
                                                        this.handleRerunTask(id);
                                                    }}
                                                    onCancel={id => {
                                                        this.handleCancelTask(id);
                                                    }}
                                                />
                                            </Fit>
                                            <Fit>
                                                <TasksPaginator
                                                    nextPageLocation={this.getNextPageLocation()}
                                                    prevPageLocation={this.getPrevPageLocation()}
                                                />
                                            </Fit>
                                        </ColumnStack>
                                    )}
                            </Loader>
                        </Fit>
                    </ColumnStack>
                    {this.state.confirmMultipleModalOpened && this.renderModal()}
                </CommonLayout.Content>
            </CommonLayout>
        );
    }
}

export default withRouter(withRemoteTaskQueueApi(TasksPageContainer));
