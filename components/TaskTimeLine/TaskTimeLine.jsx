// @flow
import React from 'react';
import moment from 'moment';
import { RouterLink } from 'ui';
import cn from './TaskTimeLine.less';
import TimeLine from './TimeLine/TimeLine';
import { TaskStates } from '../../Domain/TaskState';
import type { TaskMetaInformationModel } from '../../api/RemoteTaskQueueApi';
import type { RouterLocationDescriptor } from '../../../Commons/DataTypes/Routing';
import AllowCopyToClipboard from '../../../Commons/AllowCopyToClipboard';

const IconColors = {
    red: '#d43517',
    green: '#3F9726',
    grey: '#a0a0a0',
};

type TaskTimeLineProps = {
    taskMeta: TaskMetaInformationModel;
    getHrefToTask: (id: string) => RouterLocationDescriptor;
};

export default class TaskTimeLine extends React.Component {
    props: TaskTimeLineProps;

    getIconColor(severity: string): ?string {
        switch (severity) {
            case 'error':
                return '#d43517';
            case 'success':
                return '#3F9726';
            case 'waiting':
                return '#a0a0a0';
            default:
                return undefined;
        }
    }

    createSimpleEntry(entry: { title: string; severity?: string; icon: string; date?: ?Date | ?string }): any {
        const severity = entry.severity || 'info';

        return (
            <TimeLine.Entry
                key={entry.title}
                icon={entry.icon}
                iconColor={this.getIconColor(severity)}>
                <div className={cn('entry', severity)}>
                    <div className={cn('title')}>{entry.title}</div>
                    {entry.date && <div className={cn('date')}>
                        {moment(entry.date).utc().format('YYYY-MM-DD HH:mm:ss.SSS')} (UTC)
                    </div>}
                </div>
            </TimeLine.Entry>
        );
    }

    getStartedEntry(): any {
        const { taskMeta } = this.props;

        if (!taskMeta.startExecutingDateTime) {
            return null;
        }
        return this.createSimpleEntry({
            title: 'Started',
            icon: 'enter',
            date: taskMeta.startExecutingDateTime,
        });
    }

    getExectionEntries(): (?any)[] {
        const { taskMeta } = this.props;
        if (taskMeta.attempts === undefined || taskMeta.attempts === null || taskMeta.attempts === 0) {
            return [
                this.getShouldStartedEntry(),
                this.getStartedEntry(),
            ];
        }

        const shouldStartAndStartEntries = [];
        if (taskMeta.state === TaskStates.WaitingForRerun) {
            shouldStartAndStartEntries.push(
                this.getStartedEntry(),
                this.createSimpleEntry({
                    title: 'Finished',
                    icon: 'ok',
                    date: taskMeta.finishExecutingDateTime,
                }));
        }
        else if (taskMeta.state === TaskStates.WaitingForRerunAfterError) {
            shouldStartAndStartEntries.push(
                this.getStartedEntry(),
                this.createSimpleEntry({
                    title: 'Failed',
                    icon: 'clear',
                    severity: 'error',
                    date: taskMeta.finishExecutingDateTime,
                }));
        }
        else {
            shouldStartAndStartEntries.push(
                this.getShouldStartedEntry(),
                this.getStartedEntry()
            );
        }

        if (taskMeta.attempts !== undefined && taskMeta.attempts !== null && taskMeta.attempts > 1) {
            return [
                <TimeLine.Cycled
                    key='FewAttempts'
                    icon='refresh'
                    content={
                        `Restarted for ${taskMeta.attempts} times`
                    }>
                    {shouldStartAndStartEntries}
                </TimeLine.Cycled>,
            ];
        }
        return shouldStartAndStartEntries;
    }

    getShouldStartedEntry(): ?any {
        const { taskMeta } = this.props;
        return this.createSimpleEntry({
            title: 'Start scheduled',
            icon: 'wait',
            date: taskMeta.minimalStartDateTime,
        });
    }

    getCurrentStateEntries(): (?any)[] {
        const { taskMeta } = this.props;

        if (taskMeta.state === TaskStates.Finished) {
            return [this.createSimpleEntry({
                title: 'Finished',
                icon: 'ok',
                severity: 'success',
                date: taskMeta.finishExecutingDateTime,
            })];
        }
        if (taskMeta.state === TaskStates.Fatal) {
            return [this.createSimpleEntry({
                title: 'Failed',
                icon: 'clear',
                severity: 'error',
                date: taskMeta.finishExecutingDateTime,
            })];
        }
        if (taskMeta.state === TaskStates.Canceled) {
            return [this.createSimpleEntry({
                title: 'Canceled',
                icon: 'remove',
                severity: 'error',
                date: taskMeta.finishExecutingDateTime || taskMeta.lastModificationDateTime,
            })];
        }
        if (taskMeta.state === TaskStates.WaitingForRerun || taskMeta.state === TaskStates.WaitingForRerunAfterError) {
            return [
                this.getShouldStartedEntry(),
                this.createSimpleEntry({
                    title: 'Waiting for next run',
                    icon: 'wait',
                    severity: 'waiting',
                }),
            ];
        }
        if (taskMeta.state === TaskStates.Inprocess) {
            return [this.createSimpleEntry({
                title: 'Waiting for complete',
                icon: 'wait',
                severity: 'waiting',
            })];
        }
        if (taskMeta.state === TaskStates.New) {
            return [this.createSimpleEntry({
                title: 'Waiting for start',
                icon: 'wait',
                severity: 'waiting',
            })];
        }
        if (taskMeta.state === TaskStates.Unknown) {
            return [];
        }
        return [];
    }

    getEnqueuedEntry(): ?any {
        const { taskMeta } = this.props;
        return this.createSimpleEntry({
            title: 'Enqueued',
            icon: 'download',
            date: taskMeta.enqueueDateTime,
        });
    }

    getChildrenTaskIdsEntry(): ?any {
        const { taskMeta, getHrefToTask } = this.props;
        if (taskMeta.childTaskIds && taskMeta.childTaskIds.length > 0) {
            return (
                <TimeLine.Entry
                    key='Children'
                    icon='arrow-bottom'
                    iconColor={IconColors.grey}>
                    <div className={cn('entry', 'waiting')}>
                        <div>Enqueued tasks:</div>
                        {taskMeta.childTaskIds.slice(0, 3).map(x => (
                            <div key={x}>
                                <AllowCopyToClipboard>
                                    <RouterLink to={getHrefToTask(x)}>{x}</RouterLink>
                                </AllowCopyToClipboard>
                            </div>
                        ))}
                        {(taskMeta.childTaskIds && taskMeta.childTaskIds.length > 3) &&
                            <div>...and {taskMeta.childTaskIds.length - 3} more</div>}
                    </div>
                </TimeLine.Entry>
            );
        }
        return null;
    }

    getParentTaskIdEntry(): ?any {
        const { taskMeta, getHrefToTask } = this.props;
        if (!taskMeta.parentTaskId) {
            return null;
        }
        return (
            <TimeLine.Entry
                key='Parent'
                icon='arrow-top'
                iconColor={IconColors.grey}>
                <div className={cn('entry', 'waiting')}>
                    Parent:{' '}
                    <AllowCopyToClipboard>
                        <RouterLink to={getHrefToTask(taskMeta.parentTaskId)}>{taskMeta.parentTaskId}</RouterLink>
                    </AllowCopyToClipboard>
                </div>
            </TimeLine.Entry>
        );
    }

    getTaskTimeLineEntries(): any[] {
        return (
            [
                this.getParentTaskIdEntry(),
                this.getEnqueuedEntry(),
                ...this.getExectionEntries(),
                ...this.getCurrentStateEntries(),
                this.getChildrenTaskIdsEntry(),
            ].filter(x => x)
        );
    }

    render(): React.Element<*> {
        return (
            <TimeLine>
                {this.getTaskTimeLineEntries()}
            </TimeLine>
        );
    }
}
