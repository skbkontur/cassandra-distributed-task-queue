import { action } from "@storybook/addon-actions";
import React from "react";
import { withRouter } from "storybook-addon-react-router-v6";

import { NullCustomRenderer } from "../src";
import { RtqMonitoringTaskModel } from "../src/Domain/Api/RtqMonitoringTaskModel";
import { TaskState } from "../src/Domain/Api/TaskState";
import { TaskDetailsPage } from "../src/components/TaskDetailsPage/TaskDetailsPage";

import { createTask } from "./TaskMetaInformationUtils";

export default {
    title: "RemoteTaskQueueMonitoring/TaskDetailsPage",
    decorators: [withRouter, (story: any) => <div style={{ maxWidth: 1000 }}>{story()}</div>],
    component: TaskDetailsPage,
};

export const Default = () => (
    <TaskDetailsPage
        customRenderer={new NullCustomRenderer()}
        parentLocation="/"
        allowRerunOrCancel
        taskDetails={taskDetails}
        getTaskLocation={id => id}
        onRerun={action("onRerun")}
        onCancel={action("onCancel")}
    />
);

const taskDetails: RtqMonitoringTaskModel = {
    taskMeta: createTask({
        name: "SynchronizeUserPartiesToPortalTaskData",
        id: "1231312312312312",
        ticks: "636275120594815095",
        minimalStartTicks: "636275120594815095",
        startExecutingTicks: "636275120594815095",
        finishExecutingTicks: "636275120594815095",
        state: TaskState.Finished,
        attempts: 1,
    }),
    childTaskIds: ["1e813176-a672-11e6-8c67-1218c2e5c7a5", "1e813176-a672-11e6-8c67-1218c2e5cwew"],
    exceptionInfos: [
        "SKBKontur.Catalogue.EDI.Domain.New.Documents.Mutators.Validation.DocumentValidationException: Документ содержит ошибки\r\nЗначение «GLN грузополучателя» (C082_E3039 в NAD+CN) обязательно\r\nЗначение «Номер ТД» '10113100140317ОБ001710000' (C506_E1154 в RFF+ABT для LIN+10) не соответствует формату 'Число(2-8)/ДДММГГ(ДДММГГГГ)/Строка(=7)/Число(0-3)'\r\n   at SKBKontur.Catalogue.EDI.Domain.EdiHandling.BoxTasks.OutboxValidatingTaskHandler.HandleInternal[TDocument](TDocument document, OutboxValidatingTaskData taskData) in c:\\BuildAgent\\work\\EDIDeploymentCheckout\\EDI\\Domain\\EdiHandling\\BoxTasks\\OutboxValidatingTaskHandler.cs:line 42\r\n   at HandleInternal_04007e42-d51f-45d0-bc41-5a461a2e735c(Object , Object[] )\r\n   at SKBKontur.Catalogue.EDI.Domain.EdiHandling.BoxTasks.OutboxValidatingTaskHandler.Handle(OutboxValidatingTaskData taskData) in c:\\BuildAgent\\work\\EDIDeploymentCheckout\\EDI\\Domain\\EdiHandling\\BoxTasks\\OutboxValidatingTaskHandler.cs:line 32\r\n   at SKBKontur.Catalogue.EDI.Domain.RemoteTaskQueue.Handling.DampedRerunPeriodicityTaskHandler`1.HandleTask(TTaskData taskData) in c:\\BuildAgent\\work\\EDIDeploymentCheckout\\EDI\\Domain\\RemoteTaskQueue.Handling\\DampedRerunPeriodicityTaskHandler.cs:line 24",
    ],
    taskData: {
        deliveryContext: {
            documentType: "Partin",
            outbox: {
                id: "f1e56be8-1137-4aa5-9755-44709ca84845",
                gln: "4680013579999",
                isTest: false,
                partyId: "e80ed688-7536-48b8-a0be-d7b6c8bea401",
                inactive: false,
            },
            inbox: {
                id: "a7a67acc-7c14-473b-a157-5b7c9d63a5a1",
                gln: "4607162712428",
                isTest: false,
                partyId: "9e3e8ad8-2c33-462c-95c0-acf7c043fb8a",
                inactive: false,
            },
            deliveryBox: {
                id: "5af1fd8b-deda-4ab1-85a3-adfaa3691554",
                partyId: "9e3e8ad8-2c33-462c-95c0-acf7c043fb8a",
                relatedBoxId: "a7a67acc-7c14-473b-a157-5b7c9d63a5a1",
                inactive: false,
                primaryKey: {
                    documentType: "Partin",
                    forBoxId: "a7a67acc-7c14-473b-a157-5b7c9d63a5a1",
                },
                settings: {
                    customMessageFormat: null,
                    transportBoxEndpoints: ["e9106d1c-1ed1-4d33-95b5-859583026900"],
                },
            },
            deliverySettings: {
                messageFormat: "EdiXml",
                xmlEncoding: "utf-8",
                ftpEdifactFilesExtension: "txt",
            },
            providerId: null,
            markMessageRoaming: false,
            webTransportOnly: false,
        },
        box: {
            id: "a7a67acc-7c14-473b-a157-5b7c9d63a5a1",
            gln: "4607162712428",
            isTest: false,
            partyId: "9e3e8ad8-2c33-462c-95c0-acf7c043fb8a",
            inactive: false,
        },
        documentEntityIdentifier: {
            boxId: "a7a67acc-7c14-473b-a157-5b7c9d63a5a1",
            entityId: "922954a7-15c2-4c3e-9e71-d5ee879ba059",
        },
        documentCirculationId: "46b44852-1a22-11e7-8e0d-c355c3d667fd",
    },
};
