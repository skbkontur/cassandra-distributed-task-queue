import { TaskState } from "./Api/TaskState";

export function getAllTaskStates(): TaskState[] {
    return Object.values(TaskState);
}

export const cancelableStates = [TaskState.New, TaskState.WaitingForRerun, TaskState.WaitingForRerunAfterError];

export const rerunableStates = [
    TaskState.Fatal,
    TaskState.Finished,
    TaskState.WaitingForRerun,
    TaskState.WaitingForRerunAfterError,
];
