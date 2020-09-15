import "./react-selenium-testing";
import React from "react";
import ReactDom from "react-dom";
import { Switch, Redirect, Route } from "react-router";
import { BrowserRouter } from "react-router-dom";

import { RtqMonitoringApiFake } from "./RtqMonitoringApiFake";
import { RemoteTaskQueueApplication, RtqMonitoringApi, ICustomRenderer } from "./src";
import { RtqMonitoringSearchRequest } from "./src/Domain/Api/RtqMonitoringSearchRequest";
import { RtqMonitoringTaskModel } from "./src/Domain/Api/RtqMonitoringTaskModel";
import { TimeUtils } from "./src/Domain/Utils/TimeUtils";
import { RangeSelector } from "./src/components/DateTimeRangePicker/RangeSelector";

const rtqApiPrefix = "/remote-task-queue/";

export const rtqMonitoringApi =
    process.env.API === "fake" ? new RtqMonitoringApiFake() : new RtqMonitoringApi(rtqApiPrefix);

class CustomRenderer implements ICustomRenderer {
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

function AdminToolsEntryPoint() {
    return (
        <BrowserRouter>
            <Switch>
                <Route
                    path="/Tasks"
                    render={props => (
                        <RemoteTaskQueueApplication
                            rtqMonitoringApi={rtqMonitoringApi}
                            customRenderer={new CustomRenderer()}
                            useErrorHandlingContainer
                            isSuperUser={localStorage.getItem("isSuperUser") === "true"}
                            {...props}
                        />
                    )}
                />
                <Route exact path="/">
                    <Redirect to="/Tasks" />
                </Route>
                <Route
                    exact
                    path="/Admin"
                    render={() => {
                        localStorage.setItem("isSuperUser", "true");
                        return <Redirect to="/Tasks" />;
                    }}
                />
            </Switch>
        </BrowserRouter>
    );
}

ReactDom.render(<AdminToolsEntryPoint />, document.getElementById("content"));
