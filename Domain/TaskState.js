// @flow
import { TaskStates } from '../../Domain/EDI/Api/RemoteTaskQueue/TaskState';
export { TaskStates };
import type { TaskState } from '../../Domain/EDI/Api/RemoteTaskQueue/TaskState';
export type { TaskState };

export function getAllTaskStates(): TaskState[] {
    return Object.keys(TaskStates);
}

export const cancelableStates = [
    TaskStates.New,
    TaskStates.WaitingForRerun,
    TaskStates.WaitingForRerunAfterError,
];

export const rerunableStates = [
    TaskStates.Fatal,
    TaskStates.Finished,
    TaskStates.WaitingForRerun,
    TaskStates.WaitingForRerunAfterError,
];
