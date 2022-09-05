import { IRtqMonitoringApi } from "../../src/Domain/Api/RtqMonitoringApi";
import { RtqMonitoringSearchRequest } from "../../src/Domain/Api/RtqMonitoringSearchRequest";
import { RtqMonitoringSearchResults } from "../../src/Domain/Api/RtqMonitoringSearchResults";
import { RtqMonitoringTaskMeta } from "../../src/Domain/Api/RtqMonitoringTaskMeta";
import { RtqMonitoringTaskModel } from "../../src/Domain/Api/RtqMonitoringTaskModel";
import { TaskManipulationResult } from "../../src/Domain/Api/TaskManipulationResult";
import { TaskState } from "../../src/Domain/Api/TaskState";
import { delay } from "../../src/Domain/Utils/PromiseUtils";
import { TimeUtils } from "../../src/Domain/Utils/TimeUtils";

import names from "./names.json";
import task from "./task.json";
import tasks from "./tasks.json";
import treeTaskDetails from "./treeTaskDetails.json";
import treeTasks from "./treeTasks.json";

// let requestCount = 1;
function emulateErrors() {
    // requestCount++;
    // if (requestCount % 3 === 0) {
    //     throw new Error();
    // }
}

export function createTask(override: Partial<RtqMonitoringTaskMeta>): RtqMonitoringTaskMeta {
    const defaultTaskMeta: RtqMonitoringTaskMeta = {
        name: "Task",
        id: "Id",
        ticks: TimeUtils.dateToTicks(new Date()),
        minimalStartTicks: TimeUtils.dateToTicks(new Date()),
        startExecutingTicks: TimeUtils.dateToTicks(new Date()),
        finishExecutingTicks: TimeUtils.dateToTicks(new Date()),
        executionDurationTicks: null,
        lastModificationTicks: TimeUtils.dateToTicks(new Date()),
        expirationTimestampTicks: TimeUtils.dateToTicks(new Date()),
        expirationModificationTicks: TimeUtils.dateToTicks(new Date()),
        state: TaskState.Finished,
        attempts: 1,
        parentTaskId: "ParentTaskId",
    };
    const result = {
        ...defaultTaskMeta,
        ...override,
    };
    return result;
}

export class RtqMonitoringApiFake implements IRtqMonitoringApi {
    public async getAllTaskNames(): Promise<string[]> {
        return names;
    }

    public async search(searchRequest: RtqMonitoringSearchRequest): Promise<RtqMonitoringSearchResults> {
        if (searchRequest.queryString === "AllTasks") {
            return tasks;
        }

        if (searchRequest.queryString?.includes("DocumentCirculationId")) {
            return treeTasks;
        }

        emulateErrors();
        await delay(1000);
        const taskMetaSources: Array<Partial<RtqMonitoringTaskMeta>> = [
            {
                name: "SynchronizeUserPartiesToPortalTaskData",
                id: "1e813176-a672-11e6-8c67-1218c2e5c7a2",
                ticks: "636275120594815095",
                minimalStartTicks: "636275120594815095",
                startExecutingTicks: "636275120594815095",
                finishExecutingTicks: "636275120594815095",
                state: TaskState.Finished,
                attempts: 1,
            },
            {
                name: "SynchronizeUserPartiesToPortalTaskData",
                id: "1e813176-a672-11e6-8c67-1218c2e5c7a3",
                ticks: "636275120594815095",
                minimalStartTicks: "636275120594815095",
                startExecutingTicks: "636275120594815095",
                finishExecutingTicks: "636275120594815095",
                state: TaskState.Finished,
                attempts: 1,
            },
            {
                name: "SynchronizeUserPartiesToPortalTaskData",
                id: "1e813176-a672-11e6-8c67-1218c2e5c7a4",
                ticks: "636275120594815095",
                minimalStartTicks: "636275120594815095",
                startExecutingTicks: "636275120594815095",
                finishExecutingTicks: "636275120594815095",
                state: TaskState.Finished,
                attempts: 1,
            },
            {
                name: "SynchronizeUserPartiesToPortalTaskData",
                id: "1e813176-a672-11e6-8c67-1218c2e5c7a5",
                ticks: "636275120594815095",
                minimalStartTicks: "636275120594815095",
                startExecutingTicks: "636275120594815095",
                finishExecutingTicks: "636275120594815095",
                state: TaskState.Finished,
                attempts: 1,
            },
            {
                name: "SynchronizeUserPartiesToPortalTaskData",
                id: "1e813176-a672-11e6-8c67-1218c2e5c7a6",
                ticks: "636275120594815095",
                minimalStartTicks: "636275120594815095",
                startExecutingTicks: "636275120594815095",
                finishExecutingTicks: "636275120594815095",
                state: TaskState.Finished,
                attempts: 1,
            },
            {
                name: "SynchronizeUserPartiesToPortalTaskData",
                id: "1e813176-a672-11e6-8c67-1218c2e5c7az",
                ticks: "636275120594815095",
                minimalStartTicks: "636275120594815095",
                startExecutingTicks: "636275120594815095",
                finishExecutingTicks: "636275120594815095",
                state: TaskState.Finished,
                attempts: 1,
            },
        ];
        return {
            totalCount: "100500",
            taskMetas: taskMetaSources.map(createTask),
        };
    }

    public async getTaskDetails(taskId: string): Promise<RtqMonitoringTaskModel> {
        if (taskId === "Current") {
            return task;
        }

        if (taskId === "Error") {
            await delay(100);
            throw new Error("Api Error");
        }

        if (taskId === "NotFound") {
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore
            return {};
        }

        if (treeTaskDetails[taskId]) {
            return treeTaskDetails[taskId];
        }

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
                state: TaskState.Finished,
                attempts: 1,
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
            childTaskIds: ["1e813176-a672-11e6-8c67-1218c2e5c7a5", "1e813176-a672-11e6-8c67-1218c2e5cwew"],
            exceptionInfos: [],
        };
    }

    public async cancelTasks(_ids: string[]): Promise<{ [key: string]: TaskManipulationResult }> {
        await delay(1000);
        emulateErrors();
        return {
            "1e813176-a672-11e6-8c67-1218c2e5c7a2": TaskManipulationResult.Success,
        };
    }

    public async rerunTasks(_ids: string[]): Promise<{ [key: string]: TaskManipulationResult }> {
        await delay(1000);
        emulateErrors();
        return {
            "1e813176-a672-11e6-8c67-1218c2e5c7a2": TaskManipulationResult.Success,
        };
    }

    public async rerunTasksBySearchQuery(
        _searchRequest: RtqMonitoringSearchRequest
    ): Promise<{ [key: string]: TaskManipulationResult }> {
        await delay(1000);
        emulateErrors();
        return {
            "1e813176-a672-11e6-8c67-1218c2e5c7a2": TaskManipulationResult.Success,
        };
    }

    public async cancelTasksBySearchQuery(
        _searchRequest: RtqMonitoringSearchRequest
    ): Promise<{ [key: string]: TaskManipulationResult }> {
        await delay(1000);
        emulateErrors();
        return {
            "1e813176-a672-11e6-8c67-1218c2e5c7a2": TaskManipulationResult.Success,
        };
    }
}
