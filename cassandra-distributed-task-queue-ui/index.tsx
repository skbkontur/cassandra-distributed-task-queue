import "./react-selenium-testing";
import React from "react";
import ReactDom from "react-dom";
import { Route, Navigate, Routes, BrowserRouter } from "react-router-dom";

import { RemoteTaskQueueApplication, RtqMonitoringApi, ICustomRenderer } from "./src";
import { RtqMonitoringSearchRequest } from "./src/Domain/Api/RtqMonitoringSearchRequest";
import { RtqMonitoringTaskModel } from "./src/Domain/Api/RtqMonitoringTaskModel";
import { TimeUtils } from "./src/Domain/Utils/TimeUtils";
import { RangeSelector } from "./src/components/DateTimeRangePicker/RangeSelector";
import { RtqMonitoringApiFake } from "./stories/Api/RtqMonitoringApiFake";

const rtqApiPrefix = "/remote-task-queue/";

export const rtqMonitoringApi =
    process.env.API === "fake" ? new RtqMonitoringApiFake() : new RtqMonitoringApi(rtqApiPrefix);

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

const AdminRedirect = (): JSX.Element => {
    localStorage.setItem("isSuperUser", "true");
    return <Navigate to="../Tasks" replace />;
};

const AdminToolsEntryPoint = () => (
    <BrowserRouter>
        <Routes>
            <Route
                path="Tasks/*"
                element={
                    <RemoteTaskQueueApplication
                        rtqMonitoringApi={rtqMonitoringApi}
                        customRenderer={new CustomRenderer()}
                        useErrorHandlingContainer
                        isSuperUser={localStorage.getItem("isSuperUser") === "true"}
                    />
                }
            />
            <Route path="/" element={<Navigate to="../Tasks" />} />
            <Route path="Admin" element={<AdminRedirect />} />
        </Routes>
    </BrowserRouter>
);

ReactDom.render(<AdminToolsEntryPoint />, document.getElementById("content"));
