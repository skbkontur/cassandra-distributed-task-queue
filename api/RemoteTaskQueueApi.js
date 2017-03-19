// @flow
import type { TaskState } from '../Domain/TaskState';
import type { DateTimeRange } from '../../Commons/DataTypes/DateTimeRange';
import { TimeZones } from '../../Commons/DataTypes/Time';
import RangeSelector from '../../Commons/DateTimeRangePicker/RangeSelector';

export type RemoteTaskQueueSearchResults = {
    totalCount: number;
    taskMetas: TaskMetaInformationModel[];
};

export type RemoteTaskQueueSearchRequest = {
    enqueueDateTimeRange: DateTimeRange;
    queryString: ?string;
    taskState: ?TaskState[];
    names: ?string[];
};

function isDateTimeRangeEmpty(range: ?DateTimeRange): boolean {
    return !(range && (range.lowerBound || range.upperBound));
}

export function createDefaultRemoteTaskQueueSearchRequest(): RemoteTaskQueueSearchRequest {
    const rangeSelector = new RangeSelector(TimeZones.UTC);
    return {
        enqueueDateTimeRange: rangeSelector.getToday(),
        queryString: '',
        taskState: null,
        names: null,
    };
}

export function isRemoteTaskQueueSearchRequestEmpty(searchRequest: ?RemoteTaskQueueSearchRequest): boolean {
    if (searchRequest === null || searchRequest === undefined) {
        return true;
    }
    return (
        isDateTimeRangeEmpty(searchRequest.enqueueDateTimeRange) &&
        (
            searchRequest.queryString === null ||
            searchRequest.queryString === undefined ||
            searchRequest.queryString.trim() === ''
        ) &&
        (
            searchRequest.taskState === null ||
            searchRequest.taskState === undefined ||
            searchRequest.taskState.length === 0
        ) &&
        (
            searchRequest.names === null ||
            searchRequest.names === undefined ||
            searchRequest.names.length === 0
        )
    );
}

export type TaskMetaInformationModel = {
    name: string;
    id: string;
    enqueueDateTime?: ?string;
    minimalStartDateTime?: ?string;
    startExecutingDateTime?: ?string;
    finishExecutingDateTime?: ?string;
    lastModificationDateTime?: ?string;
    expirationTimestamp?: ?string;
    expirationModificationDateTime?: ?string;
    state: TaskState;
    attempts?: number;
    parentTaskId?: string;
    childTaskIds?: string[];
    taskGroupLock?: string;
    traceId?: string;
    traceIsActive?: boolean;
};

type TaskExceptionInfo = {
    exceptionMessageInfo: string;
};

export type RemoteTaskInfoModel = {
    taskMeta: TaskMetaInformationModel;
    taskData?: any;
    exceptionInfos?: TaskExceptionInfo[];
};

type TaskManipulationResult =
    'Success' |
    'Failure_LockAcquiringFails' |
    'Failure_InvalidTaskState' |
    'Failure_TaskDoesNotExist';

export type TaskManupulationResultMap = {
    [taskId: string]: TaskManipulationResult;
};

export type IRemoteTaskQueueApi = {
    getAllTasksNames(): Promise<string[]>;

    search(searchRequest: RemoteTaskQueueSearchRequest, from: number, size: number):
        Promise<RemoteTaskQueueSearchResults>;

    getTaskDetails(id: string): Promise<RemoteTaskInfoModel>;

    cancelTasks(ids: string[]): Promise<TaskManupulationResultMap>;
    rerunTasks(ids: string[]): Promise<TaskManupulationResultMap>;
    cancelTasksByRequest(searchRequest: RemoteTaskQueueSearchRequest): Promise<TaskManupulationResultMap>;
    rerunTasksByRequest(searchRequest: RemoteTaskQueueSearchRequest): Promise<TaskManupulationResultMap>;
};


export default class RemoteTaskQueueApi {
    static additionalHeaders = {
        ['Cache-Control']: 'no-cache, no-store',
        ['Pragma']: 'no-cache',
        ['Expires']: 0,
        ['credentials']: 'same-origin',
    };

    async checkStatus(response: Response): Promise<void> {
        if (!(response.status >= 200 && response.status < 300)) {
            const errorText = await response.text();
            throw new Error(errorText);
        }
    }

    async search(searchRequest: RemoteTaskQueueSearchRequest,
                 from: number, size: number): Promise<RemoteTaskQueueSearchResults> {
        const rightSearchRequest = createRightSearchRequest(searchRequest);
        const response = await fetch('/internal-api/remote-task-queue/tasks/search', {
            ...RemoteTaskQueueApi.additionalHeaders,
            method: 'POST',
            body: JSON.stringify({
                from: from,
                size: size,
                ...rightSearchRequest,
            }),
        });
        await this.checkStatus(response);
        const result = await response.json();
        return result;
    }

    async getAllTasksNames(): Promise<string[]> {
        const response = await fetch('/internal-api/remote-task-queue/available-task-names', {
            ...RemoteTaskQueueApi.additionalHeaders,
            method: 'GET',
        });
        await this.checkStatus(response);
        const result = await response.json();
        return result;
    }

    async cancelTasks(ids: string[]): Promise<TaskManupulationResultMap> {
        const response = await fetch('/internal-api/remote-task-queue/tasks/cancel', {
            ...RemoteTaskQueueApi.additionalHeaders,
            method: 'POST',
            body: JSON.stringify({
                ids: ids,
            }),
        });
        await this.checkStatus(response);
        const result = await response.json();
        return result;
    }

    async rerunTasks(ids: string[]): Promise<TaskManupulationResultMap> {
        const response = await fetch('/internal-api/remote-task-queue/tasks/rerun', {
            ...RemoteTaskQueueApi.additionalHeaders,
            method: 'POST',
            body: JSON.stringify({
                ids: ids,
            }),
        });
        await this.checkStatus(response);
        const result = await response.json();
        return result;
    }

    async cancelTasksByRequest(searchRequest: RemoteTaskQueueSearchRequest): Promise<TaskManupulationResultMap> {
        const rightSearchRequest = createRightSearchRequest(searchRequest);
        const response = await fetch('/internal-api/remote-task-queue/tasks/cancel-by-request', {
            ...RemoteTaskQueueApi.additionalHeaders,
            method: 'POST',
            body: JSON.stringify({
                ...rightSearchRequest,
            }),
        });
        await this.checkStatus(response);
        const result = await response.json();
        return result;
    }

    async rerunTasksByRequest(searchRequest: RemoteTaskQueueSearchRequest): Promise<TaskManupulationResultMap> {
        const rightSearchRequest = createRightSearchRequest(searchRequest);
        const response = await fetch('/internal-api/remote-task-queue/tasks/rerun-by-request', {
            ...RemoteTaskQueueApi.additionalHeaders,
            method: 'POST',
            body: JSON.stringify({
                ...rightSearchRequest,
            }),
        });
        await this.checkStatus(response);
        const result = await response.json();
        return result;
    }

    async getTaskDetails(id: string): Promise<RemoteTaskInfoModel> {
        const response = await fetch(`/internal-api/remote-task-queue/tasks/${id}`, {
            ...RemoteTaskQueueApi.additionalHeaders,
            method: 'GET',
        });
        await this.checkStatus(response);
        const result = await response.json();
        return result;
    }
}

type RightRemoteTaskQueueSearchRequest = {
    enqueueDateTimeRange: DateTimeRange;
    queryString: ?string;
    states: ?TaskState[];
    names: ?string[];
};
//TODO: Поправить модель RemoteTaskQueueSearchRequest
function createRightSearchRequest(searchRequest: RemoteTaskQueueSearchRequest): RightRemoteTaskQueueSearchRequest {
    return {
        enqueueDateTimeRange: {
            lowerBound: searchRequest.enqueueDateTimeRange.lowerBound,
            upperBound: searchRequest.enqueueDateTimeRange.upperBound,
        },
        queryString: searchRequest.queryString,
        states: searchRequest.taskState,
        names: searchRequest.names,
    };
}
