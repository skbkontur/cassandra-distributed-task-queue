import { ThemeContext } from "@skbkontur/react-ui";
import React from "react";

import { useCustomSettings } from "../../CustomSettingsContext";
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
}

export const TaskDetailsMetaTable = ({
    taskMeta: {
        attempts,
        executionDurationTicks,
        expirationModificationTicks,
        expirationTimestampTicks,
        finishExecutingTicks,
        id,
        lastModificationTicks,
        minimalStartTicks,
        name,
        parentTaskId,
        startExecutingTicks,
        state,
        ticks,
    },
    childTaskIds,
}: TaskDetailsMetaTableProps): JSX.Element => {
    const theme = React.useContext(ThemeContext);
    const { customStateCaptions } = useCustomSettings();

    const renderDate = (date?: Nullable<Ticks>): JSX.Element => (
        <span>
            <DateTimeView value={date} />{" "}
            {date && (
                <span className={jsStyles.ticks(theme)}>
                    (<AllowCopyToClipboard>{date}</AllowCopyToClipboard>)
                </span>
            )}
        </span>
    );

    const renderMetaInfo = (): JSX.Element[] => {
        const executionTime = ticksToMilliseconds(executionDurationTicks);
        return [
            <tr key="TaskId">
                <td>TaskId</td>
                <td data-tid="TaskId">
                    <AllowCopyToClipboard>{id}</AllowCopyToClipboard>
                </td>
            </tr>,
            <tr key="State">
                <td>State</td>
                <td data-tid="State">{customStateCaptions[state]}</td>
            </tr>,
            <tr key="Name">
                <td>Name</td>
                <td data-tid="Name">{name}</td>
            </tr>,
            <tr key="EnqueueTime">
                <td>EnqueueTime</td>
                <td data-tid="EnqueueTime">{renderDate(ticks)}</td>
            </tr>,
            <tr key="StartExecutingTime">
                <td>StartExecutingTime</td>
                <td data-tid="StartExecutingTime">{renderDate(startExecutingTicks)}</td>
            </tr>,
            <tr key="FinishExecutingTime">
                <td>FinishExecutingTime</td>
                <td data-tid="FinishExecutingTime">{renderDate(finishExecutingTicks)}</td>
            </tr>,
            <tr key="LastExecutionDurationInMs">
                <td>LastExecutionDurationInMs</td>
                <td data-tid="LastExecutionDurationInMs">{executionTime == null ? "unknown" : executionTime}</td>
            </tr>,
            <tr key="MinimalStartTime">
                <td>MinimalStartTime</td>
                <td data-tid="MinimalStartTime">{renderDate(minimalStartTicks)}</td>
            </tr>,
            <tr key="ExpirationTime">
                <td>ExpirationTime</td>
                <td data-tid="ExpirationTime">{renderDate(expirationTimestampTicks)}</td>
            </tr>,
            <tr key="ExpirationModificationTime">
                <td>ExpirationModificationTime</td>
                <td data-tid="ExpirationModificationTime">{renderDate(expirationModificationTicks)}</td>
            </tr>,
            <tr key="LastModificationTime">
                <td>LastModificationTime</td>
                <td data-tid="LastModificationTime">{renderDate(lastModificationTicks)}</td>
            </tr>,
            <tr key="Attempts">
                <td>Attempts</td>
                <td data-tid="Attempts">{attempts}</td>
            </tr>,
            <tr key="ParentTaskId">
                <td>ParentTaskId</td>
                <td data-tid="ParentTaskId">
                    {parentTaskId && <RouterLink to={`../${parentTaskId}`}>{parentTaskId}</RouterLink>}
                </td>
            </tr>,
            <tr key="ChildTaskIds">
                <td>ChildTaskIds</td>
                <td data-tid="ChildTaskIds">
                    {childTaskIds &&
                        childTaskIds.map(item => (
                            <span key={item}>
                                <RouterLink to={`../${item}`}>{item}</RouterLink>
                                <br />
                            </span>
                        ))}
                </td>
            </tr>,
        ];
    };

    return (
        <table className={jsStyles.table(theme)}>
            <tbody>{renderMetaInfo()}</tbody>
        </table>
    );
};
