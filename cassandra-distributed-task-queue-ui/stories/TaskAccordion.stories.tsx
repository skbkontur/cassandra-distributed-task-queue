import _ from "lodash";
import React from "react";

import { Accordion } from "../src/components/Accordion/Accordion";

export default {
    title: "RemoteTaskQueueMonitoring/TaskAccordion",
    component: Accordion,
};

export const Default = () => <Accordion value={document} title="taskData" />;

export const WithCustomRender = () => <Accordion customRender={customRender} value={document} title="taskData" />;

export const ToggleAllMultipleItemsWithTitle = () => (
    <div>
        {Array(10)
            .fill(document)
            .map((x, index) => (
                <Accordion value={x} title={`taskData[${index}]`} showToggleAll defaultCollapsed />
            ))}
    </div>
);
ToggleAllMultipleItemsWithTitle.story = {
    name: "Toggle all, multiple items with title",
};

export const ToggleAllWithoutTitle = () => <Accordion value={document} showToggleAll defaultCollapsed />;
ToggleAllWithoutTitle.story = {
    name: "Toggle all, without title",
};

export const ToggleAllHiddenNothingToToggle = () => <Accordion value={flatDocument} showToggleAll defaultCollapsed />;
ToggleAllHiddenNothingToToggle.story = {
    name: "Toggle all hidden, nothing to toggle",
};

function customRender(target: { [key: string]: any }, path: string[]): JSX.Element | null {
    const boxId = target.details?.box && typeof target.details.box === "object" && target.details.box.id;
    if (_.isEqual(path, ["details", "box", "id"]) && boxId != null && typeof boxId === "string") {
        return <a href={`/zzzz/${boxId}`}>{boxId}</a>;
    }
    return null;
}

const document = {
    documentType: {
        title: "Orders",
        mainTitle: "Orders",
    },
    details: {
        box: {
            id: "0b7db73e-c968-46cd-892d-7943b960b9ad",
            gln: ["1234512345130", "1234512345130"],
            isTest: false,
            partyId: "649e2565-c34f-4810-a2de-c20b92b51d51",
            inactive: false,
            againNewField: null,
        },
    },
    documentEntityIdentifier: {
        boxId: "0b7db73e-c968-46cd-892d-7943b960b9ad",
        entityId: "57e932f7-a6c5-46e1-9859-70c02513774a",
    },
    documentCirculationId: "a9a25c51-b73a-11e6-94c2-e672d55923d4",
};

const flatDocument = {
    title: "Orders",
    id: "0b7db73e-c968-46cd-892d-7943b960b9ad",
    gln: "1234512345130",
    isTest: false,
    partyId: "649e2565-c34f-4810-a2de-c20b92b51d51",
    inactive: false,
    againNewField: null,
    boxId: "0b7db73e-c968-46cd-892d-7943b960b9ad",
    entityId: "57e932f7-a6c5-46e1-9859-70c02513774a",
    documentCirculationId: "a9a25c51-b73a-11e6-94c2-e672d55923d4",
};
