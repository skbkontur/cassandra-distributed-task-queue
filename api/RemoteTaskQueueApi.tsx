import { DateTimeRange } from "../../Domain/DataTypes/DateTimeRange";
import { TimeZones } from "../../Commons/DataTypes/Time";
import { RangeSelector } from "../../Commons/DateTimeRangePicker/RangeSelector";
import { TaskMetaInformation } from "../../Domain/EDI/Api/RemoteTaskQueue/TaskMetaInformation";
import { RemoteTaskQueueSearchRequest } from "../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueueSearchRequest";
import { RemoteTaskQueueSearchResults } from "../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueueSearchResults";
import { IRemoteTaskQueueApi } from "../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueue";
import { RemoteTaskInfoModel } from "../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskInfoModel";
import { TaskMetaInformationAndTaskMetaInformationChildTasks } from "../../Domain/EDI/Api/RemoteTaskQueue/TaskMetaInformationChildTasks";

export type IRemoteTaskQueueApi = IRemoteTaskQueueApi;
export type TaskMetaInformation = TaskMetaInformation;
export type RemoteTaskQueueSearchRequest = RemoteTaskQueueSearchRequest;
export type RemoteTaskQueueSearchResults = RemoteTaskQueueSearchResults;
export type RemoteTaskInfoModel = RemoteTaskInfoModel;
export type TaskMetaInformationAndTaskMetaInformationChildTasks = TaskMetaInformationAndTaskMetaInformationChildTasks;

function isDateTimeRangeEmpty(range: Nullable<DateTimeRange>): boolean {
    return !(range && (range.lowerBound || range.upperBound));
}

export function createDefaultRemoteTaskQueueSearchRequest(): RemoteTaskQueueSearchRequest {
    const rangeSelector = new RangeSelector(TimeZones.UTC);
    return {
        enqueueDateTimeRange: rangeSelector.getToday(),
        queryString: "",
        states: null,
        names: null,
    };
}

export function isRemoteTaskQueueSearchRequestEmpty(searchRequest: Nullable<RemoteTaskQueueSearchRequest>): boolean {
    if (searchRequest === null || searchRequest === undefined) {
        return true;
    }
    return (
        isDateTimeRangeEmpty(searchRequest.enqueueDateTimeRange) &&
        (searchRequest.queryString === null ||
            searchRequest.queryString === undefined ||
            searchRequest.queryString.trim() === "") &&
        (searchRequest.states === null || searchRequest.states === undefined || searchRequest.states.length === 0) &&
        (searchRequest.names === null || searchRequest.names === undefined || searchRequest.names.length === 0)
    );
}
