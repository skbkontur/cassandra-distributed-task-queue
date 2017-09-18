// @flow
import React from "react";
import { RouterLink } from "ui";
import cn from "./TaskTimeLine.less";
import TimeLine from "./TimeLine/TimeLine";
import { TaskStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskState";
import DateTimeView from "../../../Commons/DateTimeView/DateTimeView";
import { ticksToDate } from "../../../Commons/DataTypes/Time";
import type { Ticks } from "../../../Commons/DataTypes/Time";
import type { TaskMetaInformationAndTaskMetaInformationChildTasks } from "../../api/RemoteTaskQueueApi";
import type { RouterLocationDescriptor } from "../../../Commons/DataTypes/Routing";
import AllowCopyToClipboard from "../../../Commons/AllowCopyToClipboard";

const TimeLineEntry = TimeLine.Entry;

const IconColors = {
    red: "#d43517",
    green: "#3F9726",
    grey: "#a0a0a0",
};

type TaskTimeLineProps = {
    taskMeta: TaskMetaInformationAndTaskMetaInformationChildTasks,
    getHrefToTask: (id: string) => RouterLocationDescriptor,
};

export default class TaskTimeLine extends React.Component {
    props: TaskTimeLineProps;

    ticksToDate(ticks: ?Ticks): ?Date {
        if (!ticks) {
            return null;
        }
        return ticksToDate(ticks);
    }

    getIconColor(severity: string): ?string {
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

    createSimpleEntry(entry: { title: string, severity?: string, icon: string, date?: ?Ticks }): React.Element<any> {
        const severity = entry.severity || "info";

        return (
            <TimeLineEntry key={entry.title} icon={entry.icon} iconColor={this.getIconColor(severity)}>
                <div className={cn("entry", severity)}>
                    <div className={cn("title")}>
                        {entry.title}
                    </div>
                    {entry.date &&
                        <div className={cn("date")}>
                            <DateTimeView value={entry.date} />
                        </div>}
                </div>
            </TimeLineEntry>
        );
    }

    getStartedEntry(): null | React.Element<any> {
        const { taskMeta } = this.props;

        if (!taskMeta.startExecutingTicks) {
            return null;
        }
        return this.createSimpleEntry({
            title: "Started",
            icon: "enter",
            date: taskMeta.startExecutingTicks,
        });
    }

    getExectionEntries(): (null | React.Element<any>)[] {
        const { taskMeta } = this.props;
        if (taskMeta.attempts === undefined || taskMeta.attempts === null || taskMeta.attempts === 0) {
            return [this.getShouldStartedEntry(), this.getStartedEntry()];
        }

        const shouldStartAndStartEntries = [];
        if (taskMeta.state === TaskStates.WaitingForRerun) {
            shouldStartAndStartEntries.push(
                this.getStartedEntry(),
                this.createSimpleEntry({
                    title: "Finished",
                    icon: "ok",
                    date: taskMeta.finishExecutingTicks,
                })
            );
        } else if (taskMeta.state === TaskStates.WaitingForRerunAfterError) {
            shouldStartAndStartEntries.push(
                this.getStartedEntry(),
                this.createSimpleEntry({
                    title: "Failed",
                    icon: "clear",
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
                <TimeLineCycled key="FewAttempts" icon="refresh" content={`Restarted for ${taskMeta.attempts} times`}>
                    {shouldStartAndStartEntries}
                </TimeLineCycled>,
            ];
        }
        return shouldStartAndStartEntries;
    }

    getShouldStartedEntry(): null | React.Element<any> {
        const { taskMeta } = this.props;
        return this.createSimpleEntry({
            title: "Start scheduled",
            icon: "wait",
            date: taskMeta.minimalStartTicks,
        });
    }

    getCurrentStateEntries(): (null | React.Element<any>)[] {
        const { taskMeta } = this.props;

        if (taskMeta.state === TaskStates.Finished) {
            return [
                this.createSimpleEntry({
                    title: "Finished",
                    icon: "ok",
                    severity: "success",
                    date: taskMeta.finishExecutingTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskStates.Fatal) {
            return [
                this.createSimpleEntry({
                    title: "Failed",
                    icon: "clear",
                    severity: "error",
                    date: taskMeta.finishExecutingTicks,
                }),
            ];
        }
        if (taskMeta.state === TaskStates.Canceled) {
            return [
                this.createSimpleEntry({
                    title: "Canceled",
                    icon: "remove",
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
                    icon: "wait",
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskStates.InProcess) {
            return [
                this.createSimpleEntry({
                    title: "Waiting for complete",
                    icon: "wait",
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskStates.New) {
            return [
                this.createSimpleEntry({
                    title: "Waiting for start",
                    icon: "wait",
                    severity: "waiting",
                }),
            ];
        }
        if (taskMeta.state === TaskStates.Unknown) {
            return [];
        }
        return [];
    }

    getEnqueuedEntry(): null | React.Element<any> {
        const { taskMeta } = this.props;
        return this.createSimpleEntry({
            title: "Enqueued",
            icon: "download",
            date: taskMeta.ticks,
        });
    }

    getChildrenTaskIdsEntry(): null | React.Element<any> {
        const { taskMeta, getHrefToTask } = this.props;
        if (taskMeta.childTaskIds && taskMeta.childTaskIds.length > 0) {
            return (
                <TimeLineEntry key="Children" icon="arrow-bottom" iconColor={IconColors.grey}>
                    <div className={cn("entry", "waiting")}>
                        <div>Enqueued tasks:</div>
                        {taskMeta.childTaskIds.slice(0, 3).map(x =>
                            <div key={x}>
                                <AllowCopyToClipboard>
                                    <RouterLink to={getHrefToTask(x)}>
                                        {x}
                                    </RouterLink>
                                </AllowCopyToClipboard>
                            </div>
                        )}
                        {taskMeta.childTaskIds &&
                            taskMeta.childTaskIds.length > 3 &&
                            <div>
                                ...and {taskMeta.childTaskIds.length - 3} more
                            </div>}
                    </div>
                </TimeLineEntry>
            );
        }
        return null;
    }

    getParentTaskIdEntry(): null | React.Element<any> {
        const { taskMeta, getHrefToTask } = this.props;
        if (!taskMeta.parentTaskId) {
            return null;
        }
        return (
            <TimeLineEntry key="Parent" icon="arrow-top" iconColor={IconColors.grey}>
                <div className={cn("entry", "waiting")}>
                    Parent:{" "}
                    <AllowCopyToClipboard>
                        <RouterLink to={getHrefToTask(taskMeta.parentTaskId)}>
                            {taskMeta.parentTaskId}
                        </RouterLink>
                    </AllowCopyToClipboard>
                </div>
            </TimeLineEntry>
        );
    }

    getTaskTimeLineEntries(): React.Element<any>[] {
        return [
            this.getParentTaskIdEntry(),
            this.getEnqueuedEntry(),
            ...this.getExectionEntries(),
            ...this.getCurrentStateEntries(),
            this.getChildrenTaskIdsEntry(),
        ].filter(Boolean);
    }

    render(): React.Element<*> {
        return (
            <TimeLine>
                {this.getTaskTimeLineEntries()}
            </TimeLine>
        );
    }
}
