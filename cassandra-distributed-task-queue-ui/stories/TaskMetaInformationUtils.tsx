import moment from "moment";

import { TaskMetaInformation } from "../src/Domain/Api/TaskMetaInformation";
import { TaskState } from "../src/Domain/Api/TaskState";
import { TimeUtils } from "../src/Domain/Utils/TimeUtils";

export function createTask(override: Partial<TaskMetaInformation>): TaskMetaInformation {
    const defaultTaskMeta: TaskMetaInformation = {
        name: "Task",
        id: "Id",
        taskDataId: null,
        taskExceptionInfoIds: null,
        ticks: TimeUtils.dateToTicks(moment().toDate()),
        minimalStartTicks: TimeUtils.dateToTicks(moment().toDate()),
        startExecutingTicks: TimeUtils.dateToTicks(moment().toDate()),
        finishExecutingTicks: TimeUtils.dateToTicks(moment().toDate()),
        executionDurationTicks: null,
        lastModificationTicks: TimeUtils.dateToTicks(moment().toDate()),
        expirationTimestampTicks: TimeUtils.dateToTicks(moment().toDate()),
        expirationModificationTicks: TimeUtils.dateToTicks(moment().toDate()),
        state: TaskState.Finished,
        attempts: 1,
        parentTaskId: "ParentTaskId",
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
