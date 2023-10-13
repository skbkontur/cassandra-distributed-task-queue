import { RangeSelector } from "../components/DateTimeRangePicker/RangeSelector";

import { RtqMonitoringSearchRequest } from "./Api/RtqMonitoringSearchRequest";
import { RtqMonitoringTaskModel } from "./Api/RtqMonitoringTaskModel";
import { TimeUtils } from "./Utils/TimeUtils";

export interface ICustomRenderer {
    renderDetails: (target: any, path: string[]) => null | JSX.Element;
    getRelatedTasksLocation: (taskDetails: RtqMonitoringTaskModel) => Nullable<RtqMonitoringSearchRequest>;
}

export class NullCustomRenderer implements ICustomRenderer {
    public getRelatedTasksLocation(): Nullable<RtqMonitoringSearchRequest> {
        return null;
    }

    public renderDetails(): null | JSX.Element {
        return null;
    }
}

export class CustomRenderer implements ICustomRenderer {
    public getRelatedTasksLocation(taskDetails: RtqMonitoringTaskModel): Nullable<RtqMonitoringSearchRequest> {
        const documentCirculationId =
            taskDetails.taskData && typeof taskDetails.taskData["DocumentCirculationId"] === "string"
                ? taskDetails.taskData["DocumentCirculationId"]
                : null;
        if (documentCirculationId != null && taskDetails.taskMeta.ticks != null) {
            const rangeSelector = new RangeSelector(TimeUtils.TimeZones.UTC);

            return {
                enqueueTimestampRange: rangeSelector.getMonthOf(TimeUtils.ticksToDate(taskDetails.taskMeta.ticks)),
                queryString: `Data.\\*.DocumentCirculationId:"${documentCirculationId || ""}"`,
                names: [],
                states: [],
            };
        }
        return null;
    }

    public renderDetails(target: any, path: string[]): null | JSX.Element {
        return null;
    }
}
