import { LocationDescriptor } from "history";
import * as React from "react";
import { IconName, RouterLink } from "ui";
import { TaskMetaInformationAndTaskMetaInformationChildTasks } from "Domain/EDI/Api/RemoteTaskQueue/TaskMetaInformationChildTasks";
import { TaskStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskState";

import { AllowCopyToClipboard } from "../../../Commons/AllowCopyToClipboard";
import { Ticks, ticksToDate } from "../../../Commons/DataTypes/Time";
import DateTimeView from "../../../Commons/DateTimeView/DateTimeView";

import cn from "./TaskTimeLine.less";
import { TimeLine } from "./TimeLine/TimeLine";

const TimeLineEntry = TimeLine.Entry;

const IconColors = {
    red: "#d43517",
    green: "#3F9726",
    grey: "#a0a0a0",
};

interface TaskTimeLineProps {
    taskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks;
    getHrefToTask: (id: string) => LocationDescriptor;
}

export class TaskTimeLine extends React.Component<TaskTimeLineProps, $FlowFixMeState> {
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
        icon: IconName;
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
            icon: "ArrowCorner1",
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
                    icon: "Ok",
                    date: taskMeta.finishExecutingTicks,
                })
            );
        } else if (taskMeta.state === TaskStates.WaitingForRerunAfterError) {
            shouldStartAndStartEntries.push(
                this.getStartedEntry(),
                this.createSimpleEntry({
                    title: "Failed",
                    icon: "Clear",
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
                <TimeLineCycled key="FewAttempts" icon="Refresh" content={`Restarted for ${taskMeta.attempts} times`}>
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
            icon: "Clock",
            date: taskMeta.minimalStartTicks,
        });
    }

    public getCurrentStateEntries(): Array<null | JSX.Element> {
        const { taskMeta } = this.props;

        if (taskMeta.state === TaskStates.Finished) {
            return [
                this.createSimpleEntry({
                    title: "Finished",
                    icon: "Ok",
                    severity: "success",
                    date: taskMeta.finishExecutingTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskStates.Fatal) {
            return [
                this.createSimpleEntry({
                    title: "Failed",
                    icon: "Clear",
                    severity: "error",
                    date: taskMeta.finishExecutingTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskStates.Canceled) {
            return [
                this.createSimpleEntry({
                    title: "Canceled",
                    icon: "Delete",
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
                    icon: "Clock",
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskStates.InProcess) {
            return [
                this.createSimpleEntry({
                    title: "Waiting for complete",
                    icon: "Clock",
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskStates.New) {
            return [
                this.createSimpleEntry({
                    title: "Waiting for start",
                    icon: "Clock",
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
            icon: "Download",
            date: taskMeta.ticks,
        });
    }

    public getChildrenTaskIdsEntry(): null | JSX.Element {
        const { taskMeta, getHrefToTask } = this.props;
        if (taskMeta.childTaskIds && taskMeta.childTaskIds.length > 0) {
            return (
                <TimeLineEntry key="Children" icon="ArrowBoldDown" iconColor={IconColors.grey}>
                    <div className={cn("entry", "waiting")}>
                        <div>Enqueued tasks:</div>
                        {taskMeta.childTaskIds.slice(0, 3).map(x => (
                            <div key={x}>
                                <AllowCopyToClipboard>
                                    <RouterLink to={getHrefToTask(x)}>{x}</RouterLink>
                                </AllowCopyToClipboard>
                            </div>
                        ))}
                        {taskMeta.childTaskIds &&
                            taskMeta.childTaskIds.length > 3 && (
                                <div>...and {taskMeta.childTaskIds.length - 3} more</div>
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
            <TimeLineEntry key="Parent" icon="ArrowBoldUp" iconColor={IconColors.grey}>
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
