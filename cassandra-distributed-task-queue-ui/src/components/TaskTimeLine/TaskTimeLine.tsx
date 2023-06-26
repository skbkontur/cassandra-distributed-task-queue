import {
    ArrowADownIcon16Regular,
    ArrowAUpIcon16Regular,
    ArrowDCornerDownRightIcon16Regular,
    ArrowRoundTimeForwardIcon16Regular,
    ArrowShapeTriangleADownIcon16Regular,
    CheckAIcon16Regular,
    NetDownloadIcon16Regular,
    TimeClockIcon16Regular,
    XCircleIcon16Regular,
    XIcon16Regular,
} from "@skbkontur/icons";
import { Link, ThemeContext } from "@skbkontur/react-ui";
import React from "react";
import { Location } from "react-router-dom";

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
    getHrefToTask: (id: string) => string | Partial<Location>;
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
            icon: <ArrowDCornerDownRightIcon16Regular />,
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
                    icon: <CheckAIcon16Regular />,
                    date: taskMeta.finishExecutingTicks,
                })
            );
        } else if (taskMeta.state === TaskState.WaitingForRerunAfterError) {
            const color = getIconColor(theme, "error");
            shouldStartAndStartEntries.push(
                getStartedEntry(),
                createSimpleEntry({
                    title: "Failed",
                    icon: <XCircleIcon16Regular color={color} />,
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
                    icon={<ArrowRoundTimeForwardIcon16Regular />}
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
            icon: <TimeClockIcon16Regular />,
            date: taskMeta.minimalStartTicks,
        });
    };

    const getCurrentStateEntries = (): Array<null | JSX.Element> => {
        if (taskMeta.state === TaskState.Finished) {
            const color = getIconColor(theme, "success");
            return [
                createSimpleEntry({
                    title: "Finished",
                    icon: <CheckAIcon16Regular color={color} />,
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
                    icon: <XCircleIcon16Regular color={color} />,
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
                    icon: <XIcon16Regular color={color} />,
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
                    icon: <TimeClockIcon16Regular color={color} />,
                    color: color,
                }),
            ];
        }
        if (taskMeta.state === TaskState.InProcess) {
            const color = getIconColor(theme, "waiting");
            return [
                createSimpleEntry({
                    title: "Waiting for complete",
                    icon: <TimeClockIcon16Regular color={color} />,
                    color: color,
                }),
            ];
        }
        if (taskMeta.state === TaskState.New) {
            const color = getIconColor(theme, "waiting");
            return [
                createSimpleEntry({
                    title: "Waiting for start",
                    icon: <TimeClockIcon16Regular color={color} />,
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
            icon: <NetDownloadIcon16Regular />,
            date: taskMeta.ticks,
        });
    };

    const getChildrenTaskIdsEntry = (): null | JSX.Element => {
        if (childTaskIds && childTaskIds.length > 0) {
            const visibleTaskIdsCount = showAllErrors ? childTaskIds.length : alwaysVisibleTaskIdsCount;
            const hiddenTaskIdsCount = childTaskIds.length - visibleTaskIdsCount;
            const color = getIconColor(theme, "waiting");
            return (
                <TimeLineEntry key="Children" icon={<ArrowADownIcon16Regular color={color} />}>
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
                                <ArrowShapeTriangleADownIcon16Regular />
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
            <TimeLineEntry key="Parent" icon={<ArrowAUpIcon16Regular color={color} />}>
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
