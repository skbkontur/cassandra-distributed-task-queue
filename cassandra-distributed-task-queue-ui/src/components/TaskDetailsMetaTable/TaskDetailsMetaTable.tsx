import { DateTimeView } from "Commons/DateTimeView/DateTimeView";
import React from "react";

import { TaskMetaInformation } from "../../Domain/Api/TaskMetaInformation";
import { Ticks } from "../../Domain/DataTypes/Time";
import { ticksToMilliseconds } from "../../Domain/Utils/ConvertTimeUtil";
import { AllowCopyToClipboard } from "../AllowCopyToClipboard";

import styles from "./TaskDetailsMetaTable.less";

export interface TaskDetailsMetaTableProps {
    taskMeta: TaskMetaInformation;
    childTaskIds: string[];
}

export class TaskDetailsMetaTable extends React.Component<TaskDetailsMetaTableProps> {
    public render(): JSX.Element {
        return (
            <table className={styles.table}>
                <tbody>{this.renderMetaInfo()}</tbody>
            </table>
        );
    }

    public renderMetaInfo(): JSX.Element[] {
        const { taskMeta, childTaskIds } = this.props;
        const executionTime = ticksToMilliseconds(taskMeta.executionDurationTicks);
        return [
            <tr key="TaskId">
                <td>TaskId</td>
                <td data-tid="TaskId">
                    <AllowCopyToClipboard>{taskMeta.id}</AllowCopyToClipboard>
                </td>
            </tr>,
            <tr key="State">
                <td>State</td>
                <td data-tid="State">{taskMeta.state}</td>
            </tr>,
            <tr key="Name">
                <td>Name</td>
                <td data-tid="Name">{taskMeta.name}</td>
            </tr>,
            <tr key="EnqueueTime">
                <td>EnqueueTime</td>
                <td data-tid="EnqueueTime">{renderDate(taskMeta.ticks)}</td>
            </tr>,
            <tr key="StartExecutingTime">
                <td>StartExecutingTime</td>
                <td data-tid="StartExecutingTime">{renderDate(taskMeta.startExecutingTicks)}</td>
            </tr>,
            <tr key="FinishExecutingTime">
                <td>FinishExecutingTime</td>
                <td data-tid="FinishExecutingTime">{renderDate(taskMeta.finishExecutingTicks)}</td>
            </tr>,
            <tr key="LastExecutionDurationInMs">
                <td>LastExecutionDurationInMs</td>
                <td data-tid="LastExecutionDurationInMs">{executionTime == null ? "unknown" : executionTime}</td>
            </tr>,
            <tr key="MinimalStartTime">
                <td>MinimalStartTime</td>
                <td data-tid="MinimalStartTime">{renderDate(taskMeta.minimalStartTicks)}</td>
            </tr>,
            <tr key="ExpirationTime">
                <td>ExpirationTime</td>
                <td data-tid="ExpirationTime">{renderDate(taskMeta.expirationTimestampTicks)}</td>
            </tr>,
            <tr key="ExpirationModificationTime">
                <td>ExpirationModificationTime</td>
                <td data-tid="ExpirationModificationTime">{renderDate(taskMeta.expirationModificationTicks)}</td>
            </tr>,
            <tr key="LastModificationTime">
                <td>LastModificationTime</td>
                <td data-tid="LastModificationTime">{renderDate(taskMeta.lastModificationTicks)}</td>
            </tr>,
            <tr key="Attempts">
                <td>Attempts</td>
                <td data-tid="Attempts">{taskMeta.attempts}</td>
            </tr>,
            <tr key="ParentTaskId">
                <td>ParentTaskId</td>
                <td data-tid="ParentTaskId">
                    {taskMeta.parentTaskId && (
                        <a href={"/AdminTools/Tasks/" + taskMeta.parentTaskId}>{taskMeta.parentTaskId}</a>
                    )}
                </td>
            </tr>,
            <tr key="ChildTaskIds">
                <td>ChildTaskIds</td>
                <td data-tid="ChildTaskIds">
                    {childTaskIds &&
                        childTaskIds.map(item => (
                            <span key={item}>
                                <a href={"/AdminTools/Tasks/" + item}>{item}</a>
                                <br />
                            </span>
                        ))}
                </td>
            </tr>,
        ];
    }
}

function renderDate(date?: Nullable<Ticks>): JSX.Element {
    return (
        <span>
            <DateTimeView value={date} />{" "}
            {date && (
                <span className={styles.ticks}>
                    (<AllowCopyToClipboard>{date}</AllowCopyToClipboard>)
                </span>
            )}
        </span>
    );
}
