import { Timestamp, AllowCopyToClipboard, Ticks } from "@skbkontur/edi-ui";
import { ArrowADownIcon16Regular } from "@skbkontur/icons/ArrowADownIcon16Regular";
import { ArrowAUpIcon16Regular } from "@skbkontur/icons/ArrowAUpIcon16Regular";
import { ArrowDCornerDownRightIcon16Regular } from "@skbkontur/icons/ArrowDCornerDownRightIcon16Regular";
import { ArrowRoundTimeForwardIcon16Regular } from "@skbkontur/icons/ArrowRoundTimeForwardIcon16Regular";
import { ArrowShapeTriangleADownIcon16Regular } from "@skbkontur/icons/ArrowShapeTriangleADownIcon16Regular";
import { CheckAIcon16Regular } from "@skbkontur/icons/CheckAIcon16Regular";
import { NetDownloadIcon16Regular } from "@skbkontur/icons/NetDownloadIcon16Regular";
import { TimeClockIcon16Regular } from "@skbkontur/icons/TimeClockIcon16Regular";
import { XCircleIcon16Regular } from "@skbkontur/icons/XCircleIcon16Regular";
import { XIcon16Regular } from "@skbkontur/icons/XIcon16Regular";
import { Link, ThemeContext } from "@skbkontur/react-ui";
import { ReactElement, useContext, useState } from "react";
import { Location } from "react-router-dom";

import { RtqMonitoringTaskMeta } from "../../Domain/Api/RtqMonitoringTaskMeta";
import { TaskState } from "../../Domain/Api/TaskState";
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

export function TaskTimeLine({ taskMeta, childTaskIds, getHrefToTask }: TaskTimeLineProps): ReactElement {
    const [showAllErrors, setShowAllErrors] = useState(false);
    const theme = useContext(ThemeContext);

    const createSimpleEntry = (entry: {
        title: string;
        icon: ReactElement;
        date?: Nullable<Ticks>;
        color?: string;
    }): ReactElement => {
        return (
            <TimeLineEntry key={entry.title} icon={entry.icon}>
                <div style={{ color: entry.color }}>{entry.title}</div>
                {entry.date && (
                    <div className={jsStyles.date(theme)}>
                        <Timestamp value={entry.date} />
                    </div>
                )}
            </TimeLineEntry>
        );
    };

    const getStartedEntry = (): null | ReactElement => {
        if (!taskMeta.startExecutingTicks) {
            return null;
        }
        return createSimpleEntry({
            title: "Started",
            icon: <ArrowDCornerDownRightIcon16Regular />,
            date: taskMeta.startExecutingTicks,
        });
    };

    const getExecutionEntries = (): Array<null | ReactElement> => {
        if (taskMeta.attempts === undefined || taskMeta.attempts === null || taskMeta.attempts === 0) {
            return [getShouldStartedEntry(), getStartedEntry()];
        }

        const shouldStartAndStartEntries: Array<null | ReactElement> = [];
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

    const getShouldStartedEntry = (): null | ReactElement => {
        return createSimpleEntry({
            title: "Start scheduled",
            icon: <TimeClockIcon16Regular />,
            date: taskMeta.minimalStartTicks,
        });
    };

    const getCurrentStateEntries = (): Array<null | ReactElement> => {
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

    const getEnqueuedEntry = (): null | ReactElement => {
        return createSimpleEntry({
            title: "Enqueued",
            icon: <NetDownloadIcon16Regular />,
            date: taskMeta.ticks,
        });
    };

    const getChildrenTaskIdsEntry = (): null | ReactElement => {
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

    const getParentTaskIdEntry = (): null | ReactElement => {
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
