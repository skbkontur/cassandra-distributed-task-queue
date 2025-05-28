import { ColumnStack, Fill, Fit, Fixed, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Checkbox, Loader, Paging } from "@skbkontur/react-ui";
import { useEffect, useState, ReactElement } from "react";
import { Location, useLocation, useNavigate } from "react-router-dom";

import { IRtqMonitoringApi } from "../Domain/Api/RtqMonitoringApi";
import { RtqMonitoringSearchRequest } from "../Domain/Api/RtqMonitoringSearchRequest";
import { RtqMonitoringSearchResults } from "../Domain/Api/RtqMonitoringSearchResults";
import { RtqMonitoringTaskMeta } from "../Domain/Api/RtqMonitoringTaskMeta";
import { TaskState } from "../Domain/Api/TaskState";
import { QueryStringMapping } from "../Domain/QueryStringMapping/QueryStringMapping";
import { QueryStringMappingBuilder } from "../Domain/QueryStringMapping/QueryStringMappingBuilder";
import { getEnumValues } from "../Domain/QueryStringMapping/QueryStringMappingExtensions";
import {
    createDefaultRemoteTaskQueueSearchRequest,
    isRemoteTaskQueueSearchRequestEmpty,
} from "../Domain/RtqMonitoringSearchRequestUtils";
import { RangeSelector } from "../components/DateTimeRangePicker/RangeSelector";
import { ErrorHandlingContainer } from "../components/ErrorHandling/ErrorHandlingContainer";
import { CommonLayout } from "../components/Layouts/CommonLayout";
import { TaskQueueFilter } from "../components/TaskQueueFilter/TaskQueueFilter";
import { TasksTable } from "../components/TaskTable/TaskTable";
import { TasksModal } from "../components/TaskTable/TasksModal";

interface TasksPageContainerProps {
    rtqMonitoringApi: IRtqMonitoringApi;
    isSuperUser: boolean;
    useErrorHandlingContainer: boolean;
    useFrontPaging?: boolean;
}

const maxTaskCountOnPage = 20;

export const searchRequestMapping: QueryStringMapping<RtqMonitoringSearchRequest> =
    new QueryStringMappingBuilder<RtqMonitoringSearchRequest>()
        .mapToDateTimeRange(x => x.enqueueTimestampRange, "enqueue")
        .mapToString(x => x.queryString, "q")
        .mapToStringArray(x => x.names, "types")
        .mapToSet(x => x.states, "states", getEnumValues(Object.keys(TaskState)))
        .mapToInteger(x => x.offset, "from")
        .mapToInteger(x => x.count, "size")
        .build();

export const TasksPageContainer = ({
    rtqMonitoringApi,
    isSuperUser,
    useErrorHandlingContainer,
    useFrontPaging,
}: TasksPageContainerProps): ReactElement => {
    const navigate = useNavigate();
    const { search, pathname } = useLocation();
    const [loading, setLoading] = useState(false);
    const [request, setRequest] = useState<RtqMonitoringSearchRequest>(createDefaultRemoteTaskQueueSearchRequest());
    const [availableTaskNames, setAvailableTaskNames] = useState<string[]>([]);
    const [confirmMultipleModalOpened, setConfirmMultipleModalOpened] = useState(false);
    const [modalType, setModalType] = useState<"Rerun" | "Cancel">("Rerun");
    const [results, setResults] = useState<RtqMonitoringSearchResults>({
        taskMetas: [],
        totalCount: "0",
    });
    const [visibleTasks, setVisibleTasks] = useState<RtqMonitoringTaskMeta[]>([]);
    const [chosenTasks, setChosenTasks] = useState(new Set<string>());
    const isAllTasksChosen = chosenTasks.size === visibleTasks.length;

    useEffect(() => {
        const newRequest = getRequestBySearchQuery(search);
        if (useFrontPaging && request.offset !== newRequest.offset) {
            const offset = newRequest.offset || 0;
            setVisibleTasks(results.taskMetas.slice(offset, offset + maxTaskCountOnPage));
        } else {
            loadData(useFrontPaging ? { ...newRequest, offset: 0 } : { ...newRequest, count: maxTaskCountOnPage });
        }
        setRequest(newRequest);
    }, [search]);

    const getTaskLocation = (id: string): string | Partial<Location> => ({
        pathname: `${pathname}/${id}`,
        state: {
            parentLocation: {
                pathname,
                search: searchRequestMapping.stringify(request),
            },
        },
    });

    const handleTaskCheck = (id: string) => {
        const nextValue = new Set(chosenTasks);
        if (nextValue.has(id)) {
            nextValue.delete(id);
        } else {
            nextValue.add(id);
        }
        setChosenTasks(nextValue);
    };

    const handleCheckAll = () => {
        if (isAllTasksChosen) {
            setChosenTasks(new Set());
        } else {
            const ids = visibleTasks.map(x => x.id);
            setChosenTasks(new Set(ids));
        }
    };

    const handleSearch = () => {
        let newRequest = { ...request };
        if (isRemoteTaskQueueSearchRequestEmpty(request)) {
            newRequest = createDefaultRemoteTaskQueueSearchRequest();
        }
        if (!newRequest.enqueueTimestampRange.lowerBound || !newRequest.enqueueTimestampRange.upperBound) {
            const rangeSelector = new RangeSelector(undefined);
            newRequest.enqueueTimestampRange = rangeSelector.getToday();
        }

        const query = getQuery(newRequest);
        if (query === pathname + search) {
            loadData(newRequest);
            return;
        }
        navigate(query);
    };

    const clickMassRerun = () => {
        setConfirmMultipleModalOpened(true);
        setModalType("Rerun");
    };

    const clickMassCancel = () => {
        setConfirmMultipleModalOpened(true);
        setModalType("Cancel");
    };

    const closeModal = () => setConfirmMultipleModalOpened(false);

    const handleRerunTasks = async (ids: string[]): Promise<void> => {
        setLoading(true);
        try {
            await rtqMonitoringApi.rerunTasks(ids);
        } finally {
            setLoading(false);
        }
    };

    const handleCancelTasks = async (ids: string[]): Promise<void> => {
        setLoading(true);
        try {
            await rtqMonitoringApi.cancelTasks(ids);
        } finally {
            setLoading(false);
        }
    };

    const handleMassRerun = (): void => {
        if (chosenTasks.size > 0) {
            handleRerunTasks([...chosenTasks]);
        } else {
            rerunAll();
        }
    };

    const handleMassCancel = (): void => {
        if (chosenTasks.size > 0) {
            handleCancelTasks([...chosenTasks]);
        } else {
            cancelAll();
        }
    };

    const getRequestBySearchQuery = (searchQuery: string): RtqMonitoringSearchRequest => {
        const request: RtqMonitoringSearchRequest = searchRequestMapping.parse(searchQuery);
        request.offset ||= 0;
        request.count = useFrontPaging ? request.count : request.count || maxTaskCountOnPage;
        return request;
    };

    const getQuery = (overrides: Partial<RtqMonitoringSearchRequest> = {}): string =>
        pathname + searchRequestMapping.stringify({ ...request, ...overrides });

    const goToPage = (page: number) => {
        navigate(getQuery({ offset: (page - 1) * maxTaskCountOnPage }));
    };

    const onChangeFilter = (value: Partial<RtqMonitoringSearchRequest>) => setRequest({ ...request, ...value });

    const isStateCompletelyLoaded = results && availableTaskNames;
    const offset = request.offset || 0;
    const counter = Number((results && results.totalCount) || 0);
    const massActionTarget = chosenTasks.size > 0 ? "Chosen" : "All";

    return (
        <CommonLayout>
            <CommonLayout.Header data-tid="Header" title="Список задач" />
            <CommonLayout.Content>
                {useErrorHandlingContainer && <ErrorHandlingContainer />}
                <ColumnStack block stretch gap={2}>
                    <Loader type="big" active={loading} data-tid={"Loader"}>
                        <Fit>
                            <TaskQueueFilter
                                value={request}
                                availableTaskTypes={availableTaskNames}
                                onChange={onChangeFilter}
                                onSearchButtonClick={handleSearch}
                                withTaskLimit={useFrontPaging}
                            />
                        </Fit>
                        <Fit>
                            {results && isStateCompletelyLoaded && (
                                <ColumnStack block stretch gap={2}>
                                    {counter > 0 && <Fit>Всего результатов: {counter}</Fit>}
                                    {counter > 0 && isSuperUser && (
                                        <RowStack gap={2} data-tid={"ButtonsWrapper"} verticalAlign="center">
                                            <RowStack verticalAlign="center" block gap={2}>
                                                <Checkbox
                                                    style={{ height: 16, marginLeft: 8 }}
                                                    onValueChange={handleCheckAll}
                                                    checked={isAllTasksChosen}
                                                />
                                                <Fixed width={106}>Все на странице</Fixed>
                                            </RowStack>
                                            <Fill />
                                            <Fit>
                                                <Button
                                                    use="danger"
                                                    data-tid={"CancelAllButton"}
                                                    onClick={clickMassCancel}>
                                                    Cancel {massActionTarget}
                                                </Button>
                                            </Fit>
                                            <Fit>
                                                <Button
                                                    use="success"
                                                    data-tid={"RerunAllButton"}
                                                    onClick={clickMassRerun}>
                                                    Rerun {massActionTarget}
                                                </Button>
                                            </Fit>
                                        </RowStack>
                                    )}
                                    <Fit>
                                        <TasksTable
                                            getTaskLocation={getTaskLocation}
                                            allowRerunOrCancel={isSuperUser}
                                            taskInfos={visibleTasks}
                                            chosenTasks={chosenTasks}
                                            onRerun={id => handleRerunTasks([id])}
                                            onCancel={id => handleCancelTasks([id])}
                                            onCheck={handleTaskCheck}
                                        />
                                    </Fit>
                                    <Fit>
                                        {Math.ceil(counter / maxTaskCountOnPage) > 1 && (
                                            <Paging
                                                data-tid="Paging"
                                                activePage={Math.floor(offset / maxTaskCountOnPage) + 1}
                                                pagesCount={Math.ceil(Math.min(counter, 10000) / maxTaskCountOnPage)}
                                                onPageChange={goToPage}
                                            />
                                        )}
                                    </Fit>
                                </ColumnStack>
                            )}
                        </Fit>
                    </Loader>
                </ColumnStack>
                {confirmMultipleModalOpened && (
                    <TasksModal
                        modalType={modalType}
                        counter={chosenTasks.size || counter}
                        onCancelAll={handleMassCancel}
                        onRerunAll={handleMassRerun}
                        onCloseModal={closeModal}
                    />
                )}
            </CommonLayout.Content>
        </CommonLayout>
    );

    async function loadData(request: RtqMonitoringSearchRequest): Promise<void> {
        if (availableTaskNames.length === 0) {
            const newAvailableTaskNames = await rtqMonitoringApi.getAllTaskNames();
            if (newAvailableTaskNames.length === 0) {
                throw new Error("Expected availableTaskNames to contain elements");
            }
            setAvailableTaskNames(newAvailableTaskNames);
        }

        if (isRemoteTaskQueueSearchRequestEmpty(request)) {
            return;
        }

        setLoading(true);
        try {
            const results = await rtqMonitoringApi.search(request);
            setResults(results);
            setVisibleTasks(
                useFrontPaging ? results.taskMetas.slice(offset, offset + maxTaskCountOnPage) : results.taskMetas
            );
        } finally {
            setLoading(false);
        }
    }

    async function cancelAll(): Promise<void> {
        const request = getRequestBySearchQuery(search);
        setLoading(true);
        try {
            await rtqMonitoringApi.cancelTasksBySearchQuery(request);
        } finally {
            setLoading(false);
            setConfirmMultipleModalOpened(false);
        }
    }

    async function rerunAll(): Promise<void> {
        const request = getRequestBySearchQuery(search);
        setLoading(true);
        try {
            await rtqMonitoringApi.rerunTasksBySearchQuery(request);
        } finally {
            setLoading(false);
            setConfirmMultipleModalOpened(false);
        }
    }
};
