import moment from "moment";

import { RtqMonitoringTaskMeta } from "../src/Domain/Api/RtqMonitoringTaskMeta";
import { TaskState } from "../src/Domain/Api/TaskState";
import { TimeUtils } from "../src/Domain/Utils/TimeUtils";

export function createTask(override: Partial<RtqMonitoringTaskMeta>): RtqMonitoringTaskMeta {
    const defaultTaskMeta: RtqMonitoringTaskMeta = {
        name: "Task",
        id: "Id",
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
    };
    const result = {
        ...defaultTaskMeta,
        ...override,
    };
    return result;
}
