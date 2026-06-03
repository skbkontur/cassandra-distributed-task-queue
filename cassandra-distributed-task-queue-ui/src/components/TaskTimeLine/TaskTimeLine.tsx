import { Timestamp, AllowCopyToClipboard, Ticks } from "@skbkontur/edi-ui";
import { IconArrowADownRegular16 } from "@skbkontur/icons/IconArrowADownRegular16";
import { IconArrowAUpRegular16 } from "@skbkontur/icons/IconArrowAUpRegular16";
import { IconArrowDCornerDownRightRegular16 } from "@skbkontur/icons/IconArrowDCornerDownRightRegular16";
import { IconArrowRoundTimeForwardRegular16 } from "@skbkontur/icons/IconArrowRoundTimeForwardRegular16";
import { IconArrowShapeTriangleADownRegular16 } from "@skbkontur/icons/IconArrowShapeTriangleADownRegular16";
import { IconCheckARegular16 } from "@skbkontur/icons/IconCheckARegular16";
import { IconNetDownloadRegular16 } from "@skbkontur/icons/IconNetDownloadRegular16";
import { IconTimeClockRegular16 } from "@skbkontur/icons/IconTimeClockRegular16";
import { IconXCircleRegular16 } from "@skbkontur/icons/IconXCircleRegular16";
import { IconXRegular16 } from "@skbkontur/icons/IconXRegular16";
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
            icon: <IconArrowDCornerDownRightRegular16 />,
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
                    icon: <IconCheckARegular16 />,
                    date: taskMeta.finishExecutingTicks,
                })
            );
        } else if (taskMeta.state === TaskState.WaitingForRerunAfterError) {
            const color = getIconColor(theme, "error");
            shouldStartAndStartEntries.push(
                getStartedEntry(),
                createSimpleEntry({
                    title: "Failed",
                    icon: <IconXCircleRegular16 color={color} />,
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
                    icon={<IconArrowRoundTimeForwardRegular16 />}
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
            icon: <IconTimeClockRegular16 />,
            date: taskMeta.minimalStartTicks,
        });
    };

    const getCurrentStateEntries = (): Array<null | ReactElement> => {
        if (taskMeta.state === TaskState.Finished) {
            const color = getIconColor(theme, "success");
            return [
                createSimpleEntry({
                    title: "Finished",
                    icon: <IconCheckARegular16 color={color} />,
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
                    icon: <IconXCircleRegular16 color={color} />,
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
                    icon: <IconXRegular16 color={color} />,
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
                    icon: <IconTimeClockRegular16 color={color} />,
                    color: color,
                }),
            ];
        }
        if (taskMeta.state === TaskState.InProcess) {
            const color = getIconColor(theme, "waiting");
            return [
                createSimpleEntry({
                    title: "Waiting for complete",
                    icon: <IconTimeClockRegular16 color={color} />,
                    color: color,
                }),
            ];
        }
        if (taskMeta.state === TaskState.New) {
            const color = getIconColor(theme, "waiting");
            return [
                createSimpleEntry({
                    title: "Waiting for start",
                    icon: <IconTimeClockRegular16 color={color} />,
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
            icon: <IconNetDownloadRegular16 />,
            date: taskMeta.ticks,
        });
    };

    const getChildrenTaskIdsEntry = (): null | ReactElement => {
        if (childTaskIds && childTaskIds.length > 0) {
            const visibleTaskIdsCount = showAllErrors ? childTaskIds.length : alwaysVisibleTaskIdsCount;
            const hiddenTaskIdsCount = childTaskIds.length - visibleTaskIdsCount;
            const color = getIconColor(theme, "waiting");
            return (
                <TimeLineEntry key="Children" icon={<IconArrowADownRegular16 color={color} />}>
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
                                <IconArrowShapeTriangleADownRegular16 />
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
            <TimeLineEntry key="Parent" icon={<IconArrowAUpRegular16 color={color} />}>
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
