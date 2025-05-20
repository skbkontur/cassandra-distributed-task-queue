import { TimeUtils } from "@skbkontur/edi-ui";
import type { ReactElement } from "react";

import { RangeSelector } from "../components/DateTimeRangePicker/RangeSelector";

import { RtqMonitoringSearchRequest } from "./Api/RtqMonitoringSearchRequest";
import { RtqMonitoringTaskModel } from "./Api/RtqMonitoringTaskModel";

interface TaskDetails extends RtqMonitoringTaskModel {
    taskData: { DocumentCirculationId?: any };
}

export interface ICustomRenderer {
    renderDetails: (target: any, path: string[]) => null | ReactElement;
    getRelatedTasksLocation: (taskDetails: RtqMonitoringTaskModel) => Nullable<RtqMonitoringSearchRequest>;
}

export class NullCustomRenderer implements ICustomRenderer {
    public getRelatedTasksLocation(): Nullable<RtqMonitoringSearchRequest> {
        return null;
    }

    public renderDetails(): null | ReactElement {
        return null;
    }
}

export class CustomRenderer implements ICustomRenderer {
    public getRelatedTasksLocation(taskDetails: TaskDetails): Nullable<RtqMonitoringSearchRequest> {
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

    public renderDetails(target: any, path: string[]): null | ReactElement {
        return null;
    }
}
