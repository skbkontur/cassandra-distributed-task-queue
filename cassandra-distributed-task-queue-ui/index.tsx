import "./react-selenium-testing";
import React from "react";
import ReactDom from "react-dom";
import { Switch, Redirect, Route } from "react-router";
import { BrowserRouter } from "react-router-dom";

import { RtqMonitoringApiFake } from "./RtqMonitoringApiFake";
import { RemoteTaskQueueApplication, RtqMonitoringApi } from "./src";

const rtqApiPrefix = "/remote-task-queue/";

export const rtqMonitoringApi =
    process.env.API === "fake" ? new RtqMonitoringApiFake() : new RtqMonitoringApi(rtqApiPrefix);

function AdminToolsEntryPoint() {
    return (
        <BrowserRouter>
            <Switch>
                <Route
                    path="/AdminTools/Tasks"
                    render={props => (
                        <RemoteTaskQueueApplication
                            // identifierKeywords={["Cql", "StorageElement"]}
                            // customRenderer={new NullCustomRenderer()}
                            rtqMonitoringApi={rtqMonitoringApi}
                            useErrorHandlingContainer
                            isSuperUser={localStorage.getItem("isSuperUser") === "true"}
                            // dbViewerApi={dbViewerApi}
                            {...props}
                        />
                    )}
                />
                <Route exact path="/">
                    <Redirect to="/AdminTools/Tasks" />
                </Route>
                <Route
                    exact
                    path="/Admin"
                    render={() => {
                        localStorage.setItem("isSuperUser", "true");
                        return <Redirect to="/AdminTools/Tasks" />;
                    }}
                />
            </Switch>
        </BrowserRouter>
    );
}

// todo react-hot-loader не дружит с react-selenium-testing
export const AdminTools = AdminToolsEntryPoint;
ReactDom.render(<AdminTools />, document.getElementById("content"));
