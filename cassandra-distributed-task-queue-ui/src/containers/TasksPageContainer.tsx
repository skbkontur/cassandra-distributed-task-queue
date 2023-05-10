import { ColumnStack, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Loader, Paging } from "@skbkontur/react-ui";
import React, { useEffect, useState } from "react";
import { Location, useLocation, useNavigate } from "react-router-dom";

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
}

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
}: TasksPageContainerProps): JSX.Element => {
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

    useEffect(() => {
        const newRequest = getRequestBySearchQuery(search);
        loadData(newRequest);
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

    const clickRerunAll = () => {
        setConfirmMultipleModalOpened(true);
        setModalType("Rerun");
    };

    const clickCancelAll = () => {
        setConfirmMultipleModalOpened(true);
        setModalType("Cancel");
    };

    const closeModal = () => setConfirmMultipleModalOpened(false);

    const handleRerunTask = async (id: string): Promise<void> => {
        setLoading(true);
        try {
            await rtqMonitoringApi.rerunTasks([id]);
        } finally {
            setLoading(false);
        }
    };

    const handleCancelTask = async (id: string): Promise<void> => {
        setLoading(true);
        try {
            await rtqMonitoringApi.cancelTasks([id]);
        } finally {
            setLoading(false);
        }
    };

    const handleRerunAll = async (): Promise<void> => {
        const request = getRequestBySearchQuery(search);
        setLoading(true);
        try {
            await rtqMonitoringApi.rerunTasksBySearchQuery(request);
        } finally {
            setLoading(false);
            setConfirmMultipleModalOpened(false);
        }
    };

    const handleCancelAll = async (): Promise<void> => {
        const request = getRequestBySearchQuery(search);
        setLoading(true);
        try {
            await rtqMonitoringApi.cancelTasksBySearchQuery(request);
        } finally {
            setLoading(false);
            setConfirmMultipleModalOpened(false);
        }
    };

    const getRequestBySearchQuery = (searchQuery: string): RtqMonitoringSearchRequest => {
        const request: RtqMonitoringSearchRequest = searchRequestMapping.parse(searchQuery);
        request.offset ||= 0;
        request.count ||= 20;
        return request;
    };

    const getQuery = (overrides: Partial<RtqMonitoringSearchRequest> = {}): string =>
        pathname + searchRequestMapping.stringify({ ...request, ...overrides });

    const goToPage = (page: number) => {
        const count = request.count || 20;
        navigate(getQuery({ offset: (page - 1) * count }));
    };

    const onChangeFilter = (value: Partial<RtqMonitoringSearchRequest>) => setRequest({ ...request, ...value });

    const isStateCompletelyLoaded = results && availableTaskNames;
    const count = request.count || 20;
    const offset = request.offset || 0;
    const counter = Number((results && results.totalCount) || 0);

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
                                                        onClick={clickCancelAll}>
                                                        Cancel All
                                                    </Button>
                                                </Fit>
                                                <Fit>
                                                    <Button
                                                        use="success"
                                                        data-tid={"RerunAllButton"}
                                                        onClick={clickRerunAll}>
                                                        Rerun All
                                                    </Button>
                                                </Fit>
                                            </RowStack>
                                        </Fit>
                                    )}
                                    <Fit>
                                        <TasksTable
                                            getTaskLocation={getTaskLocation}
                                            allowRerunOrCancel={isSuperUser}
                                            taskInfos={results.taskMetas}
                                            onRerun={handleRerunTask}
                                            onCancel={handleCancelTask}
                                        />
                                    </Fit>
                                    <Fit>
                                        {Math.ceil(counter / count) > 1 && (
                                            <Paging
                                                data-tid="Paging"
                                                activePage={Math.floor(offset / count) + 1}
                                                pagesCount={Math.ceil(Math.min(counter, 10000) / count)}
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
                        counter={counter}
                        onCancelAll={handleCancelAll}
                        onRerunAll={handleRerunAll}
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
        } finally {
            setLoading(false);
        }
    }
};
