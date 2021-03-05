import { ColumnStack, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Input, Loader, Modal, Paging } from "@skbkontur/react-ui";
import { LocationDescriptor } from "history";
import React from "react";
import { RouteComponentProps, withRouter } from "react-router-dom";

import { IRtqMonitoringApi } from "../Domain/Api/RtqMonitoringApi";
import { RtqMonitoringSearchRequest } from "../Domain/Api/RtqMonitoringSearchRequest";
import { RtqMonitoringSearchResults } from "../Domain/Api/RtqMonitoringSearchResults";
import { TaskState } from "../Domain/Api/TaskState";
import { QueryStringMapping } from "../Domain/QueryStringMapping/QueryStringMapping";
import { QueryStringMappingBuilder } from "../Domain/QueryStringMapping/QueryStringMappingBuilder";
import { getEnumValues } from "../Domain/QueryStringMapping/QueryStringMappingExtensions";
import {
    createDefaultRemoteTaskQueueSearchRequest,
    isRemoteTaskQueueSearchRequestEmpty,
} from "../Domain/RtqMonitoringSearchRequestUtils";
import { RouteUtils } from "../Domain/Utils/RouteUtils";
import { TimeUtils } from "../Domain/Utils/TimeUtils";
import { numberToString } from "../Domain/numberToString";
import { RangeSelector } from "../components/DateTimeRangePicker/RangeSelector";
import { ErrorHandlingContainer } from "../components/ErrorHandling/ErrorHandlingContainer";
import { CommonLayout } from "../components/Layouts/CommonLayout";
import { TaskQueueFilter } from "../components/TaskQueueFilter/TaskQueueFilter";
import { TasksTable } from "../components/TaskTable/TaskTable";

interface TasksPageContainerProps extends RouteComponentProps {
    searchQuery: string;
    rtqMonitoringApi: IRtqMonitoringApi;
    isSuperUser: boolean;
    path: string;
    useErrorHandlingContainer: boolean;
}

interface TasksPageContainerState {
    loading: boolean;
    request: RtqMonitoringSearchRequest;
    availableTaskNames: string[];
    confirmMultipleModalOpened: boolean;
    modalType: "Rerun" | "Cancel";
    manyTaskConfirm: string;
    results: RtqMonitoringSearchResults;
}

export const searchRequestMapping: QueryStringMapping<RtqMonitoringSearchRequest> = new QueryStringMappingBuilder<RtqMonitoringSearchRequest>()
    .mapToDateTimeRange(x => x.enqueueTimestampRange, "enqueue")
    .mapToString(x => x.queryString, "q")
    .mapToStringArray(x => x.names, "types")
    .mapToSet(x => x.states, "states", getEnumValues(Object.keys(TaskState)))
    .mapToInteger(x => x.offset, "from")
    .mapToInteger(x => x.count, "size")
    .build();

class TasksPageContainerInternal extends React.Component<TasksPageContainerProps, TasksPageContainerState> {
    public state: TasksPageContainerState = {
        loading: false,
        request: createDefaultRemoteTaskQueueSearchRequest(),
        availableTaskNames: [],
        results: {
            taskMetas: [],
            totalCount: 0,
        },
        confirmMultipleModalOpened: false,
        modalType: "Rerun",
        manyTaskConfirm: "",
    };

    public componentDidMount() {
        const { searchQuery } = this.props;
        this.setState({ request: this.getRequestBySearchQuery(searchQuery) }, this.loadData);
    }

    public componentDidUpdate(prevProps: TasksPageContainerProps) {
        if (prevProps.searchQuery !== this.props.searchQuery) {
            this.setState({ request: this.getRequestBySearchQuery(this.props.searchQuery) }, this.loadData);
        }
    }

    public render(): JSX.Element {
        const { availableTaskNames, request, loading, results } = this.state;
        const { isSuperUser, useErrorHandlingContainer } = this.props;
        const isStateCompletelyLoaded = results && availableTaskNames;
        const count = request.count || 20;
        const offset = request.offset || 0;
        const counter = (results && results.totalCount) || 0;
        return (
            <CommonLayout>
                <CommonLayout.GoBack to={RouteUtils.backUrl(this.props)}>
                    Вернуться к инструментам администратора
                </CommonLayout.GoBack>
                <CommonLayout.Header data-tid="Header" title="Список задач" />
                <CommonLayout.Content>
                    {useErrorHandlingContainer && <ErrorHandlingContainer />}
                    <ColumnStack block stretch gap={2}>
                        <Loader type="big" active={loading} data-tid={"Loader"}>
                            <Fit>
                                <TaskQueueFilter
                                    value={request}
                                    availableTaskTypes={availableTaskNames}
                                    onChange={value => this.setState({ request: { ...this.state.request, ...value } })}
                                    onSearchButtonClick={this.handleSearch}
                                />
                            </Fit>
                            <Fit>
                                {results && isStateCompletelyLoaded && (
                                    <ColumnStack block stretch gap={2}>
                                        {counter > 0 && <Fit>Всего результатов: {counter}</Fit>}
                                        {counter > 0 && isSuperUser && (
                                            <Fit>
                                                <RowStack gap={2} data-tid={"ButtonsWrapper"}>
                                                    <Fit>
                                                        <Button
                                                            use="danger"
                                                            data-tid={"CancelAllButton"}
                                                            onClick={this.clickCancelAll}>
                                                            Cancel All
                                                        </Button>
                                                    </Fit>
                                                    <Fit>
                                                        <Button
                                                            use="success"
                                                            data-tid={"RerunAllButton"}
                                                            onClick={this.clickRerunAll}>
                                                            Rerun All
                                                        </Button>
                                                    </Fit>
                                                </RowStack>
                                            </Fit>
                                        )}
                                        <Fit>
                                            <TasksTable
                                                getTaskLocation={id => this.getTaskLocation(id)}
                                                allowRerunOrCancel={isSuperUser}
                                                taskInfos={results.taskMetas}
                                                onRerun={this.handleRerunTask}
                                                onCancel={this.handleCancelTask}
                                            />
                                        </Fit>
                                        <Fit>
                                            {Math.ceil(counter / count) > 1 && (
                                                <Paging
                                                    data-tid="Paging"
                                                    activePage={Math.floor(offset / count) + 1}
                                                    pagesCount={Math.ceil(Math.min(counter, 10000) / count)}
                                                    onPageChange={this.goToPage}
                                                />
                                            )}
                                        </Fit>
                                    </ColumnStack>
                                )}
                            </Fit>
                        </Loader>
                    </ColumnStack>
                    {this.state.confirmMultipleModalOpened && this.renderModal()}
                </CommonLayout.Content>
            </CommonLayout>
        );
    }

    private renderModal(): JSX.Element {
        const { modalType, manyTaskConfirm, results } = this.state;
        const confirmedRegExp = /б.*л.*я/i;
        const counter = (results && results.totalCount) || 0;

        return (
            <Modal onClose={this.closeModal} width={500} data-tid="ConfirmMultipleOperationModal">
                <Modal.Header>Нужно подтверждение</Modal.Header>
                <Modal.Body>
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
                                    onValueChange={val => this.setState({ manyTaskConfirm: val })}
                                />
                            </Fit>,
                        ]}
                    </ColumnStack>
                </Modal.Body>
                <Modal.Footer>
                    <RowStack gap={2}>
                        <Fit>
                            {modalType === "Rerun" ? (
                                <Button
                                    data-tid="RerunButton"
                                    use="success"
                                    disabled={
                                        counter > 100 &&
                                        !confirmedRegExp.test(manyTaskConfirm) &&
                                        manyTaskConfirm !== numberToString(counter)
                                    }
                                    onClick={this.handleRerunAll}>
                                    Перезапустить все
                                </Button>
                            ) : (
                                <Button
                                    data-tid="CancelButton"
                                    use="danger"
                                    disabled={
                                        counter > 100 &&
                                        !confirmedRegExp.test(manyTaskConfirm) &&
                                        manyTaskConfirm !== numberToString(counter)
                                    }
                                    onClick={this.handleCancelAll}>
                                    Остановить все
                                </Button>
                            )}
                        </Fit>
                        <Fit>
                            <Button data-tid="CloseButton" onClick={this.closeModal}>
                                Закрыть
                            </Button>
                        </Fit>
                    </RowStack>
                </Modal.Footer>
            </Modal>
        );
    }

    private async loadData(): Promise<void> {
        const { request } = this.state;

        let availableTaskNames = this.state.availableTaskNames;
        if (availableTaskNames.length === 0) {
            availableTaskNames = await this.props.rtqMonitoringApi.getAllTaskNames();
            if (availableTaskNames.length === 0) {
                throw new Error("Expected availableTaskNames to contain elements");
            }
            this.setState({ availableTaskNames: availableTaskNames });
        }

        if (isRemoteTaskQueueSearchRequestEmpty(request)) {
            return;
        }

        this.setState({ loading: true });
        try {
            const results = await this.props.rtqMonitoringApi.search(request);
            this.setState({ results: results });
        } finally {
            this.setState({ loading: false });
        }
    }

    private getTaskLocation(id: string): LocationDescriptor {
        const { request } = this.state;

        return {
            pathname: `${this.props.path}/${id}`,
            state: {
                parentLocation: {
                    pathname: this.props.path,
                    search: searchRequestMapping.stringify(request),
                },
            },
        };
    }

    private readonly handleSearch = () => {
        let request = this.state.request;
        if (isRemoteTaskQueueSearchRequestEmpty(request)) {
            request = createDefaultRemoteTaskQueueSearchRequest();
        }
        if (request.enqueueTimestampRange.lowerBound == null || request.enqueueTimestampRange.upperBound == null) {
            const rangeSelector = new RangeSelector(TimeUtils.TimeZones.UTC);
            request.enqueueTimestampRange = rangeSelector.getToday();
        }

        const query = this.getQuery(request);
        if (query === this.props.path + this.props.searchQuery) {
            this.loadData();
            return;
        }
        this.props.history.push(query);
    };

    private readonly clickRerunAll = () => {
        this.setState({ confirmMultipleModalOpened: true, modalType: "Rerun" });
    };

    private readonly clickCancelAll = () => {
        this.setState({ confirmMultipleModalOpened: true, modalType: "Cancel" });
    };

    private readonly closeModal = () => {
        this.setState({ confirmMultipleModalOpened: false });
    };

    private readonly handleRerunTask = async (id: string): Promise<void> => {
        const { rtqMonitoringApi } = this.props;
        this.setState({ loading: true });
        try {
            await rtqMonitoringApi.rerunTasks([id]);
        } finally {
            this.setState({ loading: false });
        }
    };

    private readonly handleCancelTask = async (id: string): Promise<void> => {
        const { rtqMonitoringApi } = this.props;
        this.setState({ loading: true });
        try {
            await rtqMonitoringApi.cancelTasks([id]);
        } finally {
            this.setState({ loading: false });
        }
    };

    private readonly handleRerunAll = async (): Promise<void> => {
        const { searchQuery, rtqMonitoringApi } = this.props;
        const request = this.getRequestBySearchQuery(searchQuery);

        this.setState({ loading: true });
        try {
            await rtqMonitoringApi.rerunTasksBySearchQuery(request);
        } finally {
            this.setState({ loading: false, confirmMultipleModalOpened: false });
        }
    };

    private readonly handleCancelAll = async (): Promise<void> => {
        const { searchQuery, rtqMonitoringApi } = this.props;
        const request = this.getRequestBySearchQuery(searchQuery);

        this.setState({ loading: true });
        try {
            await rtqMonitoringApi.cancelTasksBySearchQuery(request);
        } finally {
            this.setState({ loading: false, confirmMultipleModalOpened: false });
        }
    };

    private readonly getRequestBySearchQuery = (searchQuery: string): RtqMonitoringSearchRequest => {
        const request: RtqMonitoringSearchRequest = searchRequestMapping.parse(searchQuery);
        request.offset = request.offset || 0;
        request.count = request.count || 20;
        return request;
    };

    private readonly getQuery = (overrides: Partial<RtqMonitoringSearchRequest> = {}): string => {
        const { request } = this.state;
        const { path } = this.props;
        return path + searchRequestMapping.stringify({ ...request, ...overrides });
    };

    private readonly goToPage = (page: number) => {
        const count = this.state.request.count || 20;
        this.props.history.push(this.getQuery({ offset: (page - 1) * count }));
    };
}

export const TasksPageContainer = withRouter(TasksPageContainerInternal);
