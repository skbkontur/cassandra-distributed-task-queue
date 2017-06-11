// @flow
import React from 'react';
import { Link, RouterLink, Checkbox } from 'ui';
import { RowStack, ColumnStack, Fit, Fill } from 'ui/layout';
import { TaskStates } from '../../../Domain/TaskState';
import { cancelableStates, rerunableStates } from '../../../Domain/TaskState';
import AllowCopyToClipboard from '../../../../Commons/AllowCopyToClipboard';
import DateTimeView from '../../DateTimeView/DateTimeView';
import type { RouterLocationDescriptor } from '../../../../Commons/DataTypes/Routing';
import type { TaskMetaInformation } from '../../../api/RemoteTaskQueueApi';
import type { TaskState } from '../../../Domain/TaskState';
import type { Ticks } from '../../../../Commons/DataTypes/Time';

import cn from './TaskDetails.less';

type TaskDetailsProps = {
    taskInfo: TaskMetaInformation;
    allowRerunOrCancel: boolean;
    onRerun: () => any;
    onCancel: () => any;
    getTaskLocation: (id: string) => RouterLocationDescriptor;
};

function dateFormatter(item: TaskMetaInformation, selector: (obj: TaskMetaInformation) => ?Ticks): React.Element<*> {
    return <DateTimeView value={selector(item)} />;
}

function taskDate(
    taskInfo: TaskMetaInformation,
    caption: string,
    selector: (obj: TaskMetaInformation) => ?Ticks
): React.Element<*> {
    return (
        <div className={cn('date')}>
            <span className={cn('caption')}>
                {caption}
            </span>
            <span className={cn('value')}>
                {dateFormatter(taskInfo, selector)}
            </span>
        </div>
    );
}

const stateClassNames = {
    Unknown: 'state-unknown',
    New: 'state-new',
    WaitingForRerun: 'state-waiting-for-rerun',
    WaitingForRerunAfterError: 'state-waiting-for-rerun-after-error',
    Finished: 'state-finished',
    InProcess: 'state-in-process',
    Fatal: 'state-fatal',
    Canceled: 'state-canceled',
};

function getStateClassName(taskState: TaskState): string {
    return stateClassNames[taskState];
}

export default function TaskDetails(props: TaskDetailsProps): React.Element<*> {
    const { allowRerunOrCancel, taskInfo, onCancel, onRerun, getTaskLocation } = props;
    return (
        <RowStack baseline block gap={1} className={cn('task-details', getStateClassName(taskInfo.state))}>
            <Fit className={cn('checkbox')}>
                <Checkbox disabled checked={false} />
            </Fit>
            <Fit>
                <ColumnStack block gap={1}>
                    <Fit className={cn('name')}>
                        <RouterLink data-tid='Name' to={getTaskLocation(taskInfo.id)}>
                            {taskInfo.name}
                        </RouterLink>
                    </Fit>
                    <Fit>
                        <RowStack verticalAlign='stretch' block gap={2}>
                            <Fit tag={ColumnStack} className={cn('info-block-1')}>
                                <Fit className={cn('id')}>
                                    <AllowCopyToClipboard>
                                        <span data-tid='TaskId'>{taskInfo.id}</span>
                                    </AllowCopyToClipboard>
                                </Fit>
                                <Fit className={cn('state')}>
                                    <span className={cn('state-name')} data-tid='State'>
                                        {TaskStates[taskInfo.state]}
                                    </span>
                                    <span className={cn('attempts')}>
                                        Attempts: <span data-tid='Attempts'>{taskInfo.attempts}</span>
                                    </span>
                                </Fit>
                                <Fill className={cn('parent-task')}>
                                    <div>
                                        Parent:
                                        {' '}{taskInfo.parentTaskId
                                            ? <AllowCopyToClipboard>{taskInfo.parentTaskId}</AllowCopyToClipboard>
                                            : '-'}
                                    </div>
                                </Fill>
                                {allowRerunOrCancel &&
                                    <Fit className={cn('actions')}>
                                        <RowStack baseline block gap={2}>
                                            <Fit>
                                                <Link
                                                    data-tid='Cancel'
                                                    disabled={!cancelableStates.includes(taskInfo.state)}
                                                    onClick={onCancel}
                                                    icon='remove'>
                                                    Cancel
                                                </Link>
                                            </Fit>
                                            <Fit>
                                                <Link
                                                    data-tid='Rerun'
                                                    disabled={!rerunableStates.includes(taskInfo.state)}
                                                    onClick={onRerun}
                                                    icon='refresh'>
                                                    Rerun
                                                </Link>
                                            </Fit>
                                        </RowStack>
                                    </Fit>}
                            </Fit>
                            <Fit className={cn('dates')}>
                                {taskDate(taskInfo, 'Enqueued', x => x.ticks)}
                                {taskDate(taskInfo, 'Started', x => x.startExecutingTicks)}
                                {taskDate(taskInfo, 'Finished', x => x.finishExecutingTicks)}
                                {taskDate(taskInfo, 'StateTime', x => x.minimalStartTicks)}
                                {taskDate(taskInfo, 'Expiration', x => x.expirationTimestampTicks)}
                            </Fit>
                        </RowStack>
                    </Fit>
                </ColumnStack>
            </Fit>
        </RowStack>
    );
}
