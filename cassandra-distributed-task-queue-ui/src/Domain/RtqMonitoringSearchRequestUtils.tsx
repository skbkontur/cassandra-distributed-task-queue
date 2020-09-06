import { RangeSelector } from "../components/DateTimeRangePicker/RangeSelector";

import { RtqMonitoringSearchRequest } from "./Api/RtqMonitoringSearchRequest";
import { DateTimeRange } from "./DataTypes/DateTimeRange";
import { TimeUtils } from "./Utils/TimeUtils";

function isDateTimeRangeEmpty(range: Nullable<DateTimeRange>): boolean {
    return !(range && (range.lowerBound || range.upperBound));
}

export function createDefaultRemoteTaskQueueSearchRequest(): RtqMonitoringSearchRequest {
    const rangeSelector = new RangeSelector(TimeUtils.TimeZones.UTC);
    return {
        enqueueTimestampRange: rangeSelector.getToday(),
        queryString: "",
        states: null,
        names: null,
    };
}

export function isRemoteTaskQueueSearchRequestEmpty(searchRequest: Nullable<RtqMonitoringSearchRequest>): boolean {
    if (searchRequest === null || searchRequest === undefined) {
        return true;
    }
    return (
        isDateTimeRangeEmpty(searchRequest.enqueueTimestampRange) &&
        (searchRequest.queryString === null ||
            searchRequest.queryString === undefined ||
            searchRequest.queryString.trim() === "") &&
        (searchRequest.states === null || searchRequest.states === undefined || searchRequest.states.length === 0) &&
        (searchRequest.names === null || searchRequest.names === undefined || searchRequest.names.length === 0)
    );
}
