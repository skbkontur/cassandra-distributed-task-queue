// @flow

export const TaskStates = {
    Unknown: 'Unknown',
    New: 'New',
    WaitingForRerun: 'WaitingForRerun',
    WaitingForRerunAfterError: 'WaitingForRerunAfterError',
    Finished: 'Finished',
    Inprocess: 'Inprocess',
    Fatal: 'Fatal',
    Canceled: 'Canceled',
};

export function getAllTaskStates(): TaskState[] {
    return Object.keys(TaskStates);
}

export type TaskState = $Keys<typeof TaskStates>;


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
