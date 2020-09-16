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
import Link from "@skbkontur/react-ui/Link";
import { LocationDescriptor } from "history";
import React from "react";
import { Link as RouterLink } from "react-router-dom";

import { RtqMonitoringTaskMeta } from "../../Domain/Api/RtqMonitoringTaskMeta";
import { TaskState } from "../../Domain/Api/TaskState";
import { Ticks } from "../../Domain/DataTypes/Time";
import { TimeUtils } from "../../Domain/Utils/TimeUtils";
import { AllowCopyToClipboard } from "../AllowCopyToClipboard";
import { DateTimeView } from "../DateTimeView/DateTimeView";

import styles from "./TaskTimeLine.less";
import { TimeLine } from "./TimeLine/TimeLine";

const TimeLineEntry = TimeLine.Entry;

const IconColors = {
    red: "#d43517",
    green: "#3F9726",
    grey: "#a0a0a0",
};

const alwaysVisibleTaskIdsCount = 3;

interface TaskTimeLineProps {
    taskMeta: RtqMonitoringTaskMeta;
    childTaskIds: string[];
    getHrefToTask: (id: string) => LocationDescriptor;
}

interface TaskTimeLineState {
    showAllErrors: boolean;
}

export class TaskTimeLine extends React.Component<TaskTimeLineProps, TaskTimeLineState> {
    public state: TaskTimeLineState = { showAllErrors: false };

    public ticksToDate(ticks: Nullable<Ticks>): Nullable<Date> {
        if (!ticks) {
            return null;
        }
        return TimeUtils.ticksToDate(ticks);
    }

    public getIconColor(severity: string): string | undefined {
        switch (severity) {
            case "error":
                return "#d43517";
            case "success":
                return "#3F9726";
            case "waiting":
                return "#a0a0a0";
            default:
                return undefined;
        }
    }

    public createSimpleEntry(entry: {
        title: string;
        severity?: string;
        icon: JSX.Element;
        date?: Nullable<Ticks>;
    }): JSX.Element {
        const severity = entry.severity || "info";

        return (
            <TimeLineEntry key={entry.title} icon={entry.icon} iconColor={this.getIconColor(severity)}>
                <div className={`${styles.entry} ${styles[severity]}`}>
                    <div className={styles.title}>{entry.title}</div>
                    {entry.date && (
                        <div className={styles.date}>
                            <DateTimeView value={entry.date} />
                        </div>
                    )}
                </div>
            </TimeLineEntry>
        );
    }

    public getStartedEntry(): null | JSX.Element {
        const { taskMeta } = this.props;

        if (!taskMeta.startExecutingTicks) {
            return null;
        }
        return this.createSimpleEntry({
            title: "Started",
            icon: <ArrowCorner1Icon />,
            date: taskMeta.startExecutingTicks,
        });
    }

    public getExectionEntries(): Array<null | JSX.Element> {
        const { taskMeta } = this.props;
        if (taskMeta.attempts === undefined || taskMeta.attempts === null || taskMeta.attempts === 0) {
            return [this.getShouldStartedEntry(), this.getStartedEntry()];
        }

        const shouldStartAndStartEntries: Array<null | JSX.Element> = [];
        if (taskMeta.state === TaskState.WaitingForRerun) {
            shouldStartAndStartEntries.push(
                this.getStartedEntry(),
                this.createSimpleEntry({
                    title: "Finished",
                    icon: <OkIcon />,
                    date: taskMeta.finishExecutingTicks,
                })
            );
        } else if (taskMeta.state === TaskState.WaitingForRerunAfterError) {
            shouldStartAndStartEntries.push(
                this.getStartedEntry(),
                this.createSimpleEntry({
                    title: "Failed",
                    icon: <ClearIcon />,
                    severity: "error",
                    date: taskMeta.finishExecutingTicks,
                })
            );
        } else {
            shouldStartAndStartEntries.push(this.getShouldStartedEntry(), this.getStartedEntry());
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
    }

    public getShouldStartedEntry(): null | JSX.Element {
        const { taskMeta } = this.props;
        return this.createSimpleEntry({
            title: "Start scheduled",
            icon: <ClockIcon />,
            date: taskMeta.minimalStartTicks,
        });
    }

    public getCurrentStateEntries(): Array<null | JSX.Element> {
        const { taskMeta } = this.props;

        if (taskMeta.state === TaskState.Finished) {
            return [
                this.createSimpleEntry({
                    title: "Finished",
                    icon: <OkIcon />,
                    severity: "success",
                    date: taskMeta.finishExecutingTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskState.Fatal) {
            return [
                this.createSimpleEntry({
                    title: "Failed",
                    icon: <ClearIcon />,
                    severity: "error",
                    date: taskMeta.finishExecutingTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskState.Canceled) {
            return [
                this.createSimpleEntry({
                    title: "Canceled",
                    icon: <DeleteIcon />,
                    severity: "error",
                    date: taskMeta.finishExecutingTicks || taskMeta.lastModificationTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskState.WaitingForRerun || taskMeta.state === TaskState.WaitingForRerunAfterError) {
            return [
                this.getShouldStartedEntry(),
                this.createSimpleEntry({
                    title: "Waiting for next run",
                    icon: <ClockIcon />,
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskState.InProcess) {
            return [
                this.createSimpleEntry({
                    title: "Waiting for complete",
                    icon: <ClockIcon />,
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskState.New) {
            return [
                this.createSimpleEntry({
                    title: "Waiting for start",
                    icon: <ClockIcon />,
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskState.Unknown) {
            return [];
        }
        return [];
    }

    public getEnqueuedEntry(): null | JSX.Element {
        const { taskMeta } = this.props;
        return this.createSimpleEntry({
            title: "Enqueued",
            icon: <DownloadIcon />,
            date: taskMeta.ticks,
        });
    }

    public showAllMessages = (): void => {
        this.setState({
            showAllErrors: true,
        });
    };

    public getChildrenTaskIdsEntry(): null | JSX.Element {
        const { childTaskIds, getHrefToTask } = this.props;
        if (childTaskIds && childTaskIds.length > 0) {
            const visibleTaskIdsCount = this.state.showAllErrors ? childTaskIds.length : alwaysVisibleTaskIdsCount;
            const hiddenTaskIdsCount = childTaskIds.length - visibleTaskIdsCount;

            return (
                <TimeLineEntry key="Children" icon={<ArrowBoldDown />} iconColor={IconColors.grey}>
                    <div className={`${styles.entry} ${styles.waiting}`} data-tid="EnqueuedTasks">
                        <div>Enqueued tasks:</div>
                        {childTaskIds.slice(0, visibleTaskIdsCount).map(x => (
                            <div key={x} data-tid="TaskLink">
                                <AllowCopyToClipboard>
                                    <RouterLink className={styles.routerLink} to={getHrefToTask(x)}>
                                        {x}
                                    </RouterLink>
                                </AllowCopyToClipboard>
                            </div>
                        ))}

                        {hiddenTaskIdsCount > 0 && (
                            <Link data-tid="ShowAllTasks" onClick={this.showAllMessages}>
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
    }

    public getParentTaskIdEntry(): null | JSX.Element {
        const { taskMeta, getHrefToTask } = this.props;
        if (!taskMeta.parentTaskId) {
            return null;
        }
        return (
            <TimeLineEntry key="Parent" icon={<ArrowBoldUpIcon />} iconColor={IconColors.grey}>
                <div className={`${styles.entry} ${styles.waiting}`}>
                    Parent:{" "}
                    <AllowCopyToClipboard>
                        <RouterLink className={styles.routerLink} to={getHrefToTask(taskMeta.parentTaskId)}>
                            {taskMeta.parentTaskId}
                        </RouterLink>
                    </AllowCopyToClipboard>
                </div>
            </TimeLineEntry>
        );
    }

    public getTaskTimeLineEntries(): JSX.Element[] {
        return [
            this.getParentTaskIdEntry(),
            this.getEnqueuedEntry(),
            ...this.getExectionEntries(),
            ...this.getCurrentStateEntries(),
            this.getChildrenTaskIdsEntry(),
        ]
            .filter(x => x != null)
            .map(x => x as JSX.Element);
    }

    public render(): JSX.Element {
        return <TimeLine>{this.getTaskTimeLineEntries()}</TimeLine>;
    }
}
