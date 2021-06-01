import { ArrowBoldDown } from "@skbkontur/react-icons";
import ArrowBoldUpIcon from "@skbkontur/react-icons/ArrowBoldUp";
import ArrowCorner1Icon from "@skbkontur/react-icons/ArrowCorner1";
import ArrowTriangleDownIcon from "@skbkontur/react-icons/ArrowTriangleDown";
import ClearIcon from "@skbkontur/react-icons/Clear";
import ClockIcon from "@skbkontur/react-icons/Clock";
import DeleteIcon from "@skbkontur/react-icons/Delete";
import DownloadIcon from "@skbkontur/react-icons/Download";
import OkIcon from "@skbkontur/react-icons/Ok";
import RefreshIcon from "@skbkontur/react-icons/Refresh";
import { Link, ThemeContext } from "@skbkontur/react-ui";
import { LocationDescriptor } from "history";
import React from "react";

import { RtqMonitoringTaskMeta } from "../../Domain/Api/RtqMonitoringTaskMeta";
import { TaskState } from "../../Domain/Api/TaskState";
import { Ticks } from "../../Domain/DataTypes/Time";
import { AllowCopyToClipboard } from "../AllowCopyToClipboard";
import { DateTimeView } from "../DateTimeView/DateTimeView";
import { RouterLink } from "../RouterLink/RouterLink";
import { getIconColor } from "../TaskChainTree/TaskStateIcon";

import { jsStyles } from "./TaskTimeLine.styles";
import { TimeLine } from "./TimeLine/TimeLine";

const TimeLineEntry = TimeLine.Entry;

const alwaysVisibleTaskIdsCount = 3;

interface TaskTimeLineProps {
    taskMeta: RtqMonitoringTaskMeta;
    childTaskIds: string[];
    getHrefToTask: (id: string) => LocationDescriptor;
}

export function TaskTimeLine({ taskMeta, childTaskIds, getHrefToTask }: TaskTimeLineProps): JSX.Element {
    const [showAllErrors, setShowAllErrors] = React.useState(false);
    const theme = React.useContext(ThemeContext);

    const createSimpleEntry = (entry: {
        title: string;
        icon: JSX.Element;
        date?: Nullable<Ticks>;
        color?: string;
    }): JSX.Element => {
        return (
            <TimeLineEntry key={entry.title} icon={entry.icon}>
                <div style={{ color: entry.color }}>{entry.title}</div>
                {entry.date && (
                    <div className={jsStyles.date(theme)}>
                        <DateTimeView value={entry.date} />
                    </div>
                )}
            </TimeLineEntry>
        );
    };

    const getStartedEntry = (): null | JSX.Element => {
        if (!taskMeta.startExecutingTicks) {
            return null;
        }
        return createSimpleEntry({
            title: "Started",
            icon: <ArrowCorner1Icon />,
            date: taskMeta.startExecutingTicks,
        });
    };

    const getExecutionEntries = (): Array<null | JSX.Element> => {
        if (taskMeta.attempts === undefined || taskMeta.attempts === null || taskMeta.attempts === 0) {
            return [getShouldStartedEntry(), getStartedEntry()];
        }

        const shouldStartAndStartEntries: Array<null | JSX.Element> = [];
        if (taskMeta.state === TaskState.WaitingForRerun) {
            shouldStartAndStartEntries.push(
                getStartedEntry(),
                createSimpleEntry({
                    title: "Finished",
                    icon: <OkIcon />,
                    date: taskMeta.finishExecutingTicks,
                })
            );
        } else if (taskMeta.state === TaskState.WaitingForRerunAfterError) {
            const color = getIconColor(theme, "error");
            shouldStartAndStartEntries.push(
                getStartedEntry(),
                createSimpleEntry({
                    title: "Failed",
                    icon: <ClearIcon color={color} />,
                    color: color,
                    date: taskMeta.finishExecutingTicks,
                })
            );
        } else {
            shouldStartAndStartEntries.push(getShouldStartedEntry(), getStartedEntry());
        }

        if (taskMeta.attempts !== undefined && taskMeta.attempts !== null && taskMeta.attempts > 1) {
            const TimeLineCycled = TimeLine.Cycled;
            return [
                <TimeLineCycled
                    key="FewAttempts"
                    icon={<RefreshIcon />}
                    content={`Restarted for ${taskMeta.attempts} times`}>
                    {shouldStartAndStartEntries}
                </TimeLineCycled>,
            ];
        }
        return shouldStartAndStartEntries;
    };

    const getShouldStartedEntry = (): null | JSX.Element => {
        return createSimpleEntry({
            title: "Start scheduled",
            icon: <ClockIcon />,
            date: taskMeta.minimalStartTicks,
        });
    };

    const getCurrentStateEntries = (): Array<null | JSX.Element> => {
        if (taskMeta.state === TaskState.Finished) {
            const color = getIconColor(theme, "success");
            return [
                createSimpleEntry({
                    title: "Finished",
                    icon: <OkIcon color={color} />,
                    color: color,
                    date: taskMeta.finishExecutingTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskState.Fatal) {
            const color = getIconColor(theme, "error");
            return [
                createSimpleEntry({
                    title: "Failed",
                    icon: <ClearIcon color={color} />,
                    color: color,
                    date: taskMeta.finishExecutingTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskState.Canceled) {
            const color = getIconColor(theme, "error");
            return [
                createSimpleEntry({
                    title: "Canceled",
                    icon: <DeleteIcon color={color} />,
                    color: color,
                    date: taskMeta.finishExecutingTicks || taskMeta.lastModificationTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskState.WaitingForRerun || taskMeta.state === TaskState.WaitingForRerunAfterError) {
            const color = getIconColor(theme, "waiting");
            return [
                getShouldStartedEntry(),
                createSimpleEntry({
                    title: "Waiting for next run",
                    icon: <ClockIcon color={color} />,
                    color: color,
                }),
            ];
        }
        if (taskMeta.state === TaskState.InProcess) {
            const color = getIconColor(theme, "waiting");
            return [
                createSimpleEntry({
                    title: "Waiting for complete",
                    icon: <ClockIcon color={color} />,
                    color: color,
                }),
            ];
        }
        if (taskMeta.state === TaskState.New) {
            const color = getIconColor(theme, "waiting");
            return [
                createSimpleEntry({
                    title: "Waiting for start",
                    icon: <ClockIcon color={color} />,
                    color: color,
                }),
            ];
        }
        if (taskMeta.state === TaskState.Unknown) {
            return [];
        }
        return [];
    };

    const getEnqueuedEntry = (): null | JSX.Element => {
        return createSimpleEntry({
            title: "Enqueued",
            icon: <DownloadIcon />,
            date: taskMeta.ticks,
        });
    };

    const getChildrenTaskIdsEntry = (): null | JSX.Element => {
        if (childTaskIds && childTaskIds.length > 0) {
            const visibleTaskIdsCount = showAllErrors ? childTaskIds.length : alwaysVisibleTaskIdsCount;
            const hiddenTaskIdsCount = childTaskIds.length - visibleTaskIdsCount;
            const color = getIconColor(theme, "waiting");
            return (
                <TimeLineEntry key="Children" icon={<ArrowBoldDown color={color} />}>
                    <div style={{ color: color }} data-tid="EnqueuedTasks">
                        <div>Enqueued tasks:</div>
                        {childTaskIds.slice(0, visibleTaskIdsCount).map(x => (
                            <div key={x} data-tid="TaskLink">
                                <AllowCopyToClipboard>
                                    <RouterLink to={getHrefToTask(x)}>{x}</RouterLink>
                                </AllowCopyToClipboard>
                            </div>
                        ))}

                        {hiddenTaskIdsCount > 0 && (
                            <Link data-tid="ShowAllTasks" onClick={() => setShowAllErrors(true)}>
                                ...and {hiddenTaskIdsCount} more
                                {"\u00A0"}
                                <ArrowTriangleDownIcon />
                            </Link>
                        )}
                    </div>
                </TimeLineEntry>
            );
        }
        return null;
    };

    const getParentTaskIdEntry = (): null | JSX.Element => {
        if (!taskMeta.parentTaskId) {
            return null;
        }
        const color = getIconColor(theme, "waiting");
        return (
            <TimeLineEntry key="Parent" icon={<ArrowBoldUpIcon color={color} />}>
                <div style={{ color: color }}>
                    Parent:{" "}
                    <AllowCopyToClipboard>
                        <RouterLink to={getHrefToTask(taskMeta.parentTaskId)}>{taskMeta.parentTaskId}</RouterLink>
                    </AllowCopyToClipboard>
                </div>
            </TimeLineEntry>
        );
    };

    return (
        <TimeLine>
            {[
                getParentTaskIdEntry(),
                getEnqueuedEntry(),
                ...getExecutionEntries(),
                ...getCurrentStateEntries(),
                getChildrenTaskIdsEntry(),
            ]}
        </TimeLine>
    );
}
