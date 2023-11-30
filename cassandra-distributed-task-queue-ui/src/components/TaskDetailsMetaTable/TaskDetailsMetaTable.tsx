import { ThemeContext } from "@skbkontur/react-ui";
import React, { ReactNode } from "react";

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
    const { customStateCaptions, hideMissingMeta } = useCustomSettings();

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

    const renderRow = (name: string, value: Nullable<string>, render?: (x: Nullable<string>) => ReactNode) =>
        value || !hideMissingMeta ? (
            <tr key={name}>
                <td>{name}</td>
                <td data-tid={name}>{render ? render(value) : value}</td>
            </tr>
        ) : null;

    const renderMetaInfo = (): ReactNode[] => {
        const executionTime = ticksToMilliseconds(executionDurationTicks);
        console.log(executionTime);
        return [
            renderRow("TaskId", id, id => <AllowCopyToClipboard>{id}</AllowCopyToClipboard>),
            renderRow("State", customStateCaptions[state]),
            renderRow("Name", name),
            renderRow("EnqueueTime", ticks, renderDate),
            renderRow("StartExecutingTime", startExecutingTicks, renderDate),
            renderRow("FinishExecutingTime", finishExecutingTicks, renderDate),
            renderRow("LastExecutionDurationInMs", executionTime, executionTime => executionTime || "unknown"),
            renderRow("MinimalStartTime", minimalStartTicks, renderDate),
            renderRow("ExpirationTime", expirationTimestampTicks, renderDate),
            renderRow("ExpirationModificationTime", expirationModificationTicks, renderDate),
            renderRow("LastModificationTime", lastModificationTicks, renderDate),
            renderRow("Attempts", attempts.toString()),
            renderRow("ParentTaskId", parentTaskId, parentTaskId => (
                <RouterLink to={`../${parentTaskId}`}>{parentTaskId}</RouterLink>
            )),
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
