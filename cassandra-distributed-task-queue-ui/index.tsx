import "./react-selenium-testing";
import React from "react";
import ReactDom from "react-dom";
import { Switch, Redirect, Route } from "react-router";
import { BrowserRouter } from "react-router-dom";

import { RemoteTaskQueueApiFake } from "./RemoteTaskQueueApiFake";
import { RemoteTaskQueueApplication, RemoteTaskQueueApi } from "./src";

const rtqApiPrefix = "/remote-task-queue/";

export const dbViewerApi = process.env.API === "fake" ? new RemoteTaskQueueApiFake() : new DbViewerApi(rtqApiPrefix);

function AdminToolsEntryPoint() {
    return (
        <BrowserRouter>
            <Switch>
                <Route
                    path="/BusinessObjects"
                    render={props => (
                        <RemoteTaskQueueApplication
                            identifierKeywords={["Cql", "StorageElement"]}
                            customRenderer={new NullCustomRenderer()}
                            useErrorHandlingContainer
                            isSuperUser={localStorage.getItem("isSuperUser") === "true"}
                            dbViewerApi={dbViewerApi}
                            {...props}
                        />
                    )}
                />
                <Route exact path="/">
                    <Redirect to="/BusinessObjects" />
                </Route>
                <Route
                    exact
                    path="/Admin"
                    render={() => {
                        localStorage.setItem("isSuperUser", "true");
                        return <Redirect to="/BusinessObjects" />;
                    }}
                />
            </Switch>
        </BrowserRouter>
    );
}

// todo react-hot-loader не дружит с react-selenium-testing
export const AdminTools = AdminToolsEntryPoint;
ReactDom.render(<AdminTools />, document.getElementById("content"));
