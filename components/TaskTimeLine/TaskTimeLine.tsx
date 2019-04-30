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
import { LocationDescriptor } from "history";
import * as React from "react";
import { ButtonLink, RouterLink } from "ui";
import { AllowCopyToClipboard } from "Commons/AllowCopyToClipboard";
import { Ticks, ticksToDate } from "Commons/DataHelpers/Time";
import { DateTimeView } from "Commons/DateTimeView/DateTimeView";
import { TaskMetaInformationAndTaskMetaInformationChildTasks } from "Domain/EDI/Api/RemoteTaskQueue/TaskMetaInformationChildTasks";
import { TaskStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskState";

import cn from "./TaskTimeLine.less";
import { TimeLine } from "./TimeLine/TimeLine";

const TimeLineEntry = TimeLine.Entry;

const IconColors = {
    red: "#d43517",
    green: "#3F9726",
    grey: "#a0a0a0",
};

const alwaysVisibleTaskIdsCount = 3;

interface TaskTimeLineProps {
    taskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks;
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
        return ticksToDate(ticks);
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
                <div className={cn("entry", severity)}>
                    <div className={cn("title")}>{entry.title}</div>
                    {entry.date && (
                        <div className={cn("date")}>
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
        if (taskMeta.state === TaskStates.WaitingForRerun) {
            shouldStartAndStartEntries.push(
                this.getStartedEntry(),
                this.createSimpleEntry({
                    title: "Finished",
                    icon: <OkIcon />,
                    date: taskMeta.finishExecutingTicks,
                })
            );
        } else if (taskMeta.state === TaskStates.WaitingForRerunAfterError) {
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

        if (taskMeta.state === TaskStates.Finished) {
            return [
                this.createSimpleEntry({
                    title: "Finished",
                    icon: <OkIcon />,
                    severity: "success",
                    date: taskMeta.finishExecutingTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskStates.Fatal) {
            return [
                this.createSimpleEntry({
                    title: "Failed",
                    icon: <ClearIcon />,
                    severity: "error",
                    date: taskMeta.finishExecutingTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskStates.Canceled) {
            return [
                this.createSimpleEntry({
                    title: "Canceled",
                    icon: <DeleteIcon />,
                    severity: "error",
                    date: taskMeta.finishExecutingTicks || taskMeta.lastModificationTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskStates.WaitingForRerun || taskMeta.state === TaskStates.WaitingForRerunAfterError) {
            return [
                this.getShouldStartedEntry(),
                this.createSimpleEntry({
                    title: "Waiting for next run",
                    icon: <ClockIcon />,
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskStates.InProcess) {
            return [
                this.createSimpleEntry({
                    title: "Waiting for complete",
                    icon: <ClockIcon />,
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskStates.New) {
            return [
                this.createSimpleEntry({
                    title: "Waiting for start",
                    icon: <ClockIcon />,
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskStates.Unknown) {
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
        const { taskMeta, getHrefToTask } = this.props;
        if (taskMeta.childTaskIds && taskMeta.childTaskIds.length > 0) {
            const visibleTaskIdsCount = this.state.showAllErrors
                ? taskMeta.childTaskIds.length
                : alwaysVisibleTaskIdsCount;
            const hiddenTaskIdsCount = taskMeta.childTaskIds.length - visibleTaskIdsCount;

            return (
                <TimeLineEntry key="Children" icon={<ArrowBoldDown />} iconColor={IconColors.grey}>
                    <div className={cn("entry", "waiting")} data-tid="EnqueuedTasks">
                        <div>Enqueued tasks:</div>
                        {taskMeta.childTaskIds.slice(0, visibleTaskIdsCount).map(x => (
                            <div key={x} data-tid="TaskLink">
                                <AllowCopyToClipboard>
                                    <RouterLink to={getHrefToTask(x)}>{x}</RouterLink>
                                </AllowCopyToClipboard>
                            </div>
                        ))}

                        {hiddenTaskIdsCount > 0 && (
                            <ButtonLink
                                data-tid={"ShowAllTasks"}
                                rightIcon={<ArrowTriangleDownIcon />}
                                onClick={this.showAllMessages}>
                                ...and {hiddenTaskIdsCount} more
                            </ButtonLink>
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
                <div className={cn("entry", "waiting")}>
                    Parent:{" "}
                    <AllowCopyToClipboard>
                        <RouterLink to={getHrefToTask(taskMeta.parentTaskId)}>{taskMeta.parentTaskId}</RouterLink>
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
