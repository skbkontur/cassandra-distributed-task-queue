// @flow
import type { RemoteTaskQueueSearchResults } from "./../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueueSearchResults";
import type { RemoteTaskQueueSearchRequest } from "./../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueueSearchRequest";
import type { TaskMetaInformationAndTaskMetaInformationChildTasks } from "./../../Domain/EDI/Api/RemoteTaskQueue/TaskMetaInformationChildTasks";
import type { RemoteTaskInfoModel } from "./../../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskInfoModel";
import type { IRemoteTaskQueueApi } from "./RemoteTaskQueueApi";
import type { TaskManipulationResult } from "./../../Domain/EDI/Api/RemoteTaskQueue/TaskManipulationResult";
import moment from "moment";
import { dateToTicks } from "../../Commons/DataTypes/Time";
import { delay } from "utils";
import { TaskStates } from "../../Domain/EDI/Api/RemoteTaskQueue/TaskState";

let requestCount = 1;
function emulateErrors() {
    requestCount++;
    if (requestCount % 3 === 0) {
        throw new Error();
    }
}

export function createTask(
    override: $Shape<TaskMetaInformationAndTaskMetaInformationChildTasks>
): TaskMetaInformationAndTaskMetaInformationChildTasks {
    const defaultTaskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks = {
        name: "Task",
        id: "Id",
        taskDataId: null,
        taskExceptionInfoIds: null,
        ticks: dateToTicks(moment().toDate()),
        minimalStartTicks: dateToTicks(moment().toDate()),
        startExecutingTicks: dateToTicks(moment().toDate()),
        finishExecutingTicks: dateToTicks(moment().toDate()),
        lastModificationTicks: dateToTicks(moment().toDate()),
        expirationTimestampTicks: dateToTicks(moment().toDate()),
        expirationModificationTicks: dateToTicks(moment().toDate()),
        state: TaskStates.Finished,
        attempts: 1,
        parentTaskId: "ParentTaskId",
        childTaskIds: [],
        taskGroupLock: "",
        traceId: "",
        traceIsActive: false,
    };
    const result = {
        ...defaultTaskMeta,
        ...override,
    };
    return result;
}

export class RemoteTaskQueueApi implements IRemoteTaskQueueApi {
    async getAllTaskNames(): Promise<string[]> {
        await delay(1300);
        emulateErrors();
        return ["Name1", "Name2", "Name3", "Name4", "Name5"];
    }

    async search(
        _searchRequest: RemoteTaskQueueSearchRequest,
        _from: number,
        _size: number
    ): Promise<RemoteTaskQueueSearchResults> {
        emulateErrors();
        await delay(1000);
        return {
            totalCount: 100500,
            taskMetas: [
                {
                    name: "SynchronizeUserPartiesToPortalTaskData",
                    id: "1e813176-a672-11e6-8c67-1218c2e5c7a2",
                    ticks: "636275120594815095",
                    minimalStartTicks: "636275120594815095",
                    startExecutingTicks: "636275120594815095",
                    finishExecutingTicks: "636275120594815095",
                    state: "Finished",
                    attempts: 1,
                },
                {
                    name: "SynchronizeUserPartiesToPortalTaskData",
                    id: "1e813176-a672-11e6-8c67-1218c2e5c7a3",
                    ticks: "636275120594815095",
                    minimalStartTicks: "636275120594815095",
                    startExecutingTicks: "636275120594815095",
                    finishExecutingTicks: "636275120594815095",
                    state: "Finished",
                    attempts: 1,
                },
                {
                    name: "SynchronizeUserPartiesToPortalTaskData",
                    id: "1e813176-a672-11e6-8c67-1218c2e5c7a4",
                    ticks: "636275120594815095",
                    minimalStartTicks: "636275120594815095",
                    startExecutingTicks: "636275120594815095",
                    finishExecutingTicks: "636275120594815095",
                    state: "Finished",
                    attempts: 1,
                },
                {
                    name: "SynchronizeUserPartiesToPortalTaskData",
                    id: "1e813176-a672-11e6-8c67-1218c2e5c7a5",
                    ticks: "636275120594815095",
                    minimalStartTicks: "636275120594815095",
                    startExecutingTicks: "636275120594815095",
                    finishExecutingTicks: "636275120594815095",
                    state: "Finished",
                    attempts: 1,
                },
                {
                    name: "SynchronizeUserPartiesToPortalTaskData",
                    id: "1e813176-a672-11e6-8c67-1218c2e5c7a6",
                    ticks: "636275120594815095",
                    minimalStartTicks: "636275120594815095",
                    startExecutingTicks: "636275120594815095",
                    finishExecutingTicks: "636275120594815095",
                    state: "Finished",
                    attempts: 1,
                },
                {
                    name: "SynchronizeUserPartiesToPortalTaskData",
                    id: "1e813176-a672-11e6-8c67-1218c2e5c7az",
                    ticks: "636275120594815095",
                    minimalStartTicks: "636275120594815095",
                    startExecutingTicks: "636275120594815095",
                    finishExecutingTicks: "636275120594815095",
                    state: "Finished",
                    attempts: 1,
                },
            ].map(x => createTask(x)),
        };
    }

    async getTaskDetails(taskId: string): Promise<RemoteTaskInfoModel> {
        await delay(1000);
        emulateErrors();
        return {
            taskMeta: createTask({
                name: "SynchronizeUserPartiesToPortalTaskData",
                id: taskId,
                ticks: "636275120594815095",
                minimalStartTicks: "636275120594815095",
                startExecutingTicks: "636275120594815095",
                finishExecutingTicks: "636275120594815095",
                state: TaskStates.Finished,
                attempts: 1,
                childTaskIds: ["1e813176-a672-11e6-8c67-1218c2e5c7a5", "1e813176-a672-11e6-8c67-1218c2e5cwew"],
            }),
            taskData: {
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
                transportBox: {
                    id: "3743fa7e-fe2d-4d3a-92d6-14ca4d232dd9",
                    primaryKey: {
                        forBoxId: "8d4d57ae-e939-4e3e-a727-b13b1f6e6c2c",
                    },
                    partyId: "61066fb8-a243-439b-92b3-a75f2ed0bd58",
                },
                documentCirculationId: "a9a25c51-b73a-11e6-94c2-e672d55923d4",
                computedConnectorInteractionId: "3d6bf8d7-2fa7-41d1-bec5-76ed44ea38b7",
                computedConnectorBoxId: "98c827e1-d146-4c5b-a88f-6af4180cbfe8",
            },
            exceptionInfos: [],
        };
    }

    async cancelTasks(_ids: string[]): Promise<{ [key: string]: TaskManipulationResult }> {
        await delay(1000);
        emulateErrors();
        return {
            "1e813176-a672-11e6-8c67-1218c2e5c7a2": "Success",
        };
    }

    async rerunTasks(_ids: string[]): Promise<{ [key: string]: TaskManipulationResult }> {
        await delay(1000);
        emulateErrors();
        return {
            "1e813176-a672-11e6-8c67-1218c2e5c7a2": "Success",
        };
    }

    async rerunTasksBySearchQuery(
        _searchRequest: RemoteTaskQueueSearchRequest
    ): Promise<{ [key: string]: TaskManipulationResult }> {
        await delay(1000);
        emulateErrors();
        return {
            "1e813176-a672-11e6-8c67-1218c2e5c7a2": "Success",
        };
    }

    async cancelTasksBySearchQuery(
        _searchRequest: RemoteTaskQueueSearchRequest
    ): Promise<{ [key: string]: TaskManipulationResult }> {
        await delay(1000);
        emulateErrors();
        return {
            "1e813176-a672-11e6-8c67-1218c2e5c7a2": "Success",
        };
    }
}
