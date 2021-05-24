import { ThemeContext } from "@skbkontur/react-ui";
import React from "react";

import { RtqMonitoringTaskMeta } from "../../Domain/Api/RtqMonitoringTaskMeta";
import { Ticks } from "../../Domain/DataTypes/Time";
import { ticksToMilliseconds } from "../../Domain/Utils/ConvertTimeUtil";
import { AllowCopyToClipboard } from "../AllowCopyToClipboard";
import { DateTimeView } from "../DateTimeView/DateTimeView";
import { RouterLink } from "../RouterLink/RouterLink";

import { jsStyles } from "./TaskDetailsMetaTable.styles";

export interface TaskDetailsMetaTableProps {
    taskMeta: RtqMonitoringTaskMeta;
    childTaskIds: string[];
    path: string;
}

export function TaskDetailsMetaTable({ taskMeta, childTaskIds, path }: TaskDetailsMetaTableProps): JSX.Element {
    const theme = React.useContext(ThemeContext);

    const renderMetaInfo = (): JSX.Element[] => {
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
                        <RouterLink to={`${path}/${taskMeta.parentTaskId}`}>{taskMeta.parentTaskId}</RouterLink>
                    )}
                </td>
            </tr>,
            <tr key="ChildTaskIds">
                <td>ChildTaskIds</td>
                <td data-tid="ChildTaskIds">
                    {childTaskIds &&
                        childTaskIds.map(item => (
                            <span key={item}>
                                <RouterLink to={`${path}/${item}`}>{item}</RouterLink>
                                <br />
                            </span>
                        ))}
                </td>
            </tr>,
        ];
    };

    const renderDate = (date?: Nullable<Ticks>): JSX.Element => {
        return (
            <span>
                <DateTimeView value={date} />{" "}
                {date && (
                    <span className={jsStyles.ticks(theme)}>
                        (<AllowCopyToClipboard>{date}</AllowCopyToClipboard>)
                    </span>
                )}
            </span>
        );
    };

    return (
        <table className={jsStyles.table(theme)}>
            <tbody>{renderMetaInfo()}</tbody>
        </table>
    );
}
