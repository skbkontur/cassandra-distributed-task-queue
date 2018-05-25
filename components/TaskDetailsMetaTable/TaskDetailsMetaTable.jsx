// @flow
import * as React from "react";

import AllowCopyToClipboard from "../../../Commons/AllowCopyToClipboard";
import { type TaskMetaInformationAndTaskMetaInformationChildTasks } from "../../api/RemoteTaskQueueApi";
import DateTimeView from "../../../Commons/DateTimeView/DateTimeView";
import { type Ticks } from "../../../Commons/DataTypes/Time";

import cn from "./TaskDetailsMetaTable.less";

export type TaskDetailsMetaTableProps = {
    taskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks,
};

export default class TaskDetailsMetaTable extends React.Component<TaskDetailsMetaTableProps, $FlowFixMeState> {
    render(): React.Node {
        return (
            <table className={cn("table")}>
                <tbody>{this.renderMetaInfo()}</tbody>
            </table>
        );
    }

    renderMetaInfo(): React.Node[] {
        const { taskMeta } = this.props;
        return [
            <tr key="TaskId">
                <td>TaskId</td>
                <td data-tid="TaskId">
                    <AllowCopyToClipboard>{taskMeta.id}</AllowCopyToClipboard>
                </td>
            </tr>,
            <tr key="TaskState">
                <td>TaskState</td>
                <td data-tid="TaskState">{taskMeta.state}</td>
            </tr>,
            <tr key="Name">
                <td>Name</td>
                <td data-tid="Name">{taskMeta.name}</td>
            </tr>,
            <tr key="EnqueueTime">
                <td>EnqueueTime</td>
                <td data-tid="EnqueueTime">{renderDate(taskMeta.ticks)}</td>
            </tr>,
            <tr key="StartExecutedTime">
                <td>StartExecutedTime</td>
                <td data-tid="StartExecutedTime">{renderDate(taskMeta.startExecutingTicks)}</td>
            </tr>,
            <tr key="FinishExecutedTime">
                <td>FinishExecutedTime</td>
                <td data-tid="FinishExecutedTime">{renderDate(taskMeta.finishExecutingTicks)}</td>
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
                    {taskMeta.childTaskIds &&
                        taskMeta.childTaskIds.map(item => {
                            return (
                                <span key={item}>
                                    <a href={"/AdminTools/Tasks/" + item}>{item}</a>
                                    <br />
                                </span>
                            );
                        })}
                </td>
            </tr>,
        ];
    }
}

function renderDate(date?: ?Ticks): React.Node {
    return (
        <span>
            <DateTimeView value={date} />{" "}
            {date && (
                <span className={cn("ticks")}>
                    (<AllowCopyToClipboard>{date}</AllowCopyToClipboard>)
                </span>
            )}
        </span>
    );
}
