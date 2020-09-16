import DeleteIcon from "@skbkontur/react-icons/Delete";
import RefreshIcon from "@skbkontur/react-icons/Refresh";
import { ColumnStack, Fill, Fit, RowStack } from "@skbkontur/react-stack-layout";
import Link from "@skbkontur/react-ui/Link";
import { LocationDescriptor } from "history";
import React from "react";
import { Link as RouterLink } from "react-router-dom";

import { RtqMonitoringTaskMeta } from "../../../Domain/Api/RtqMonitoringTaskMeta";
import { TaskState } from "../../../Domain/Api/TaskState";
import { Ticks } from "../../../Domain/DataTypes/Time";
import { cancelableStates, rerunableStates } from "../../../Domain/TaskStateExtensions";
import { AllowCopyToClipboard } from "../../AllowCopyToClipboard";
import { DateTimeView } from "../../DateTimeView/DateTimeView";

import styles from "./TaskDetails.less";

interface TaskDetailsProps {
    taskInfo: RtqMonitoringTaskMeta;
    allowRerunOrCancel: boolean;
    onRerun: () => any;
    onCancel: () => any;
    getTaskLocation: (id: string) => LocationDescriptor;
}

function dateFormatter(
    item: RtqMonitoringTaskMeta,
    selector: (obj: RtqMonitoringTaskMeta) => Nullable<Ticks>
): JSX.Element {
    return <DateTimeView value={selector(item)} />;
}

function taskDate(
    taskInfo: RtqMonitoringTaskMeta,
    caption: string,
    selector: (obj: RtqMonitoringTaskMeta) => Nullable<Ticks>
): JSX.Element {
    return (
        <div className={styles.date}>
            <span className={styles.caption}>{caption}</span>
            <span className={styles.value}>{dateFormatter(taskInfo, selector)}</span>
        </div>
    );
}

const stateClassNames = {
    Unknown: "stateUnknown",
    New: "stateNew",
    WaitingForRerun: "stateWaitingForRerun",
    WaitingForRerunAfterError: "stateWaitingForRerunAfterError",
    Finished: "stateFinished",
    InProcess: "stateInProcess",
    Fatal: "stateFatal",
    Canceled: "stateCanceled",
};

function getStateClassName(taskState: TaskState): string {
    return stateClassNames[taskState];
}

export function TaskDetails(props: TaskDetailsProps): JSX.Element {
    const { allowRerunOrCancel, taskInfo, onCancel, onRerun, getTaskLocation } = props;
    return (
        <ColumnStack block gap={1} className={`${styles.taskDetails} ${styles[getStateClassName(taskInfo.state)]}`}>
            <Fit className={styles.name}>
                <RouterLink className={styles.routerLink} data-tid="Name" to={getTaskLocation(taskInfo.id)}>
                    {taskInfo.name}
                </RouterLink>
            </Fit>
            <Fit>
                <RowStack verticalAlign="stretch" block gap={2}>
                    <Fit tag={ColumnStack} className={styles.infoBlock1}>
                        <Fit className={styles.id}>
                            <AllowCopyToClipboard>
                                <span data-tid="TaskId">{taskInfo.id}</span>
                            </AllowCopyToClipboard>
                        </Fit>
                        <Fit className={styles.state}>
                            <span className={styles.stateName} data-tid="State">
                                {TaskState[taskInfo.state]}
                            </span>
                            <span className={styles.attempts}>
                                Attempts: <span data-tid="Attempts">{taskInfo.attempts}</span>
                            </span>
                        </Fit>
                        <Fill className={styles.parentTask}>
                            <div>
                                Parent:{" "}
                                {taskInfo.parentTaskId ? (
                                    <AllowCopyToClipboard>{taskInfo.parentTaskId}</AllowCopyToClipboard>
                                ) : (
                                    "-"
                                )}
                            </div>
                        </Fill>
                        {allowRerunOrCancel && (
                            <Fit className={styles.actions}>
                                <RowStack baseline block gap={2}>
                                    <Fit>
                                        <Link
                                            data-tid="Cancel"
                                            disabled={!cancelableStates.includes(taskInfo.state)}
                                            onClick={onCancel}
                                            icon={<DeleteIcon />}>
                                            Cancel
                                        </Link>
                                    </Fit>
                                    <Fit>
                                        <Link
                                            data-tid="Rerun"
                                            disabled={!rerunableStates.includes(taskInfo.state)}
                                            onClick={onRerun}
                                            icon={<RefreshIcon />}>
                                            Rerun
                                        </Link>
                                    </Fit>
                                </RowStack>
                            </Fit>
                        )}
                    </Fit>
                    <Fit className={styles.dates}>
                        {taskDate(taskInfo, "Enqueued", x => x.ticks)}
                        {taskDate(taskInfo, "Started", x => x.startExecutingTicks)}
                        {taskDate(taskInfo, "Finished", x => x.finishExecutingTicks)}
                        {taskDate(taskInfo, "StateTime", x => x.minimalStartTicks)}
                        {taskDate(taskInfo, "Expiration", x => x.expirationTimestampTicks)}
                    </Fit>
                </RowStack>
            </Fit>
        </ColumnStack>
    );
}
