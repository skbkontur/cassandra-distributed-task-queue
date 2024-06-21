import { TimeUtils } from "@skbkontur/edi-ui";

import { RtqMonitoringTaskMeta } from "../src/Domain/Api/RtqMonitoringTaskMeta";
import { TaskState } from "../src/Domain/Api/TaskState";

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
