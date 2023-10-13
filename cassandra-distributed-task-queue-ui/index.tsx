import "./react-selenium-testing";
import ReactDom from "react-dom";
import { Route, Navigate, Routes, BrowserRouter } from "react-router-dom";

import { RemoteTaskQueueApplication, RtqMonitoringApi } from "./src";
import { CustomRenderer } from "./src/Domain/CustomRenderer";
import { RtqMonitoringApiFake } from "./stories/Api/RtqMonitoringApiFake";

const rtqApiPrefix = "/remote-task-queue/";

export const rtqMonitoringApi =
    process.env.API === "fake" ? new RtqMonitoringApiFake() : new RtqMonitoringApi(rtqApiPrefix);

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
