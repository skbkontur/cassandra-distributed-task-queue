// @flow
import type { DateTimeRange } from '../../Domain/DataTypes/DateTimeRange';
import { TimeZones } from '../../Commons/DataTypes/Time';
import RangeSelector from '../../Commons/DateTimeRangePicker/RangeSelector';
import type { TaskMetaInformation } from '../../Domain/EDI/Api/RemoteTaskQueue/TaskMetaInformation';
import type { RemoteTaskQueueSearchRequest } from '../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueueSearchRequest';
import type { RemoteTaskQueueSearchResults } from '../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueueSearchResults';
import type { IRemoteTaskQueueApi } from '../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueue';
import type { RemoteTaskInfoModel } from '../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskInfoModel';
import type { TaskMetaInformationAndTaskMetaInformationChildTasks } from '../../Domain/EDI/Api/RemoteTaskQueue/TaskMetaInformationChildTasks';

export type {
    IRemoteTaskQueueApi,
    TaskMetaInformation,
    RemoteTaskQueueSearchRequest,
    RemoteTaskQueueSearchResults,
    RemoteTaskInfoModel,
    TaskMetaInformationAndTaskMetaInformationChildTasks,
};

function isDateTimeRangeEmpty(range: ?DateTimeRange): boolean {
    return !(range && (range.lowerBound || range.upperBound));
}

export function createDefaultRemoteTaskQueueSearchRequest(): RemoteTaskQueueSearchRequest {
    const rangeSelector = new RangeSelector(TimeZones.UTC);
    return {
        enqueueDateTimeRange: rangeSelector.getToday(),
        queryString: '',
        states: null,
        names: null,
        from: 0,
        size: 20,
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
            searchRequest.states === null ||
            searchRequest.states === undefined ||
            searchRequest.states.length === 0
        ) &&
        (
            searchRequest.names === null ||
            searchRequest.names === undefined ||
            searchRequest.names.length === 0
        )
    );
}
