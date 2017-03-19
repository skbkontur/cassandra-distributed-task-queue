// @flow
import React from 'react';
import { Link, RouterLink, Checkbox } from 'ui';
import { RowStack, ColumnStack } from 'ui/layout';
import { TaskStates } from '../../../Domain/TaskState';
import { cancelableStates, rerunableStates } from '../../../Domain/TaskState';
import AllowCopyToClipboard from '../../../../Commons/AllowCopyToClipboard';
import DateTimeView from '../../DateTimeView/DateTimeView';
import type { RouterLocationDescriptor } from '../../../../Commons/DataTypes/Routing';
import type { TaskMetaInformationModel } from '../../../api/RemoteTaskQueueApi';
import type { TaskState } from '../../../Domain/TaskState';

import cn from './TaskDetails.less';

type TaskDetailsProps = {
    taskInfo: TaskMetaInformationModel;
    allowRerunOrCancel: boolean;
    onRerun: () => any;
    onCancel: () => any;
    getTaskLocation: (id: string) => RouterLocationDescriptor;
};

function dateFormatter(item: TaskMetaInformationModel, column: string): React.Element<*> {
    return <DateTimeView value={item[column]} />;
}

function taskDate(taskInfo: TaskMetaInformationModel, caption: string, path: string): React.Element<*> {
    return (
        <div className={cn('date')}>
            <span className={cn('caption')}>
                {caption}
            </span>
            <span className={cn('value')}>
                {dateFormatter(taskInfo, path)}
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
    Inprocess: 'state-in-process',
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
            <RowStack.Fit className={cn('checkbox')}>
                <Checkbox disabled checked={false} />
            </RowStack.Fit>
            <RowStack.Fit>
                <ColumnStack block gap={1}>
                    <ColumnStack.Fit className={cn('name')}>
                        <RouterLink
                            data-tid='Name'
                            to={getTaskLocation(taskInfo.id)}>
                            {taskInfo.name}
                        </RouterLink>
                    </ColumnStack.Fit>
                    <ColumnStack.Fit>
                        <RowStack verticalAlign='stretch' block gap={2}>
                            <RowStack.Fit tag={ColumnStack} className={cn('info-block-1')}>
                                <ColumnStack.Fit className={cn('id')}>
                                    <AllowCopyToClipboard>
                                        <span data-tid='TaskId'>{taskInfo.id}</span>
                                    </AllowCopyToClipboard>
                                </ColumnStack.Fit>
                                <ColumnStack.Fit className={cn('state')}>
                                    <span className={cn('state-name')} data-tid='State'>
                                        {TaskStates[taskInfo.state]}
                                    </span>
                                    <span className={cn('attempts')}>
                                        Attempts: <span data-tid='Attempts'>{taskInfo.attempts}</span>
                                    </span>
                                </ColumnStack.Fit>
                                <ColumnStack.Fill className={cn('parent-task')}>
                                    <div>Parent: {taskInfo.parentTaskId
                                        ? <AllowCopyToClipboard>{taskInfo.parentTaskId}</AllowCopyToClipboard>
                                        : '-'}
                                    </div>
                                </ColumnStack.Fill>
                                {allowRerunOrCancel && <ColumnStack.Fit className={cn('actions')}>
                                    <RowStack baseline block gap={2}>
                                        <RowStack.Fit>
                                            <Link
                                                data-tid='Cancel'
                                                disabled={!cancelableStates.includes(taskInfo.state)}
                                                onClick={onCancel}
                                                icon='remove'>
                                                Cancel
                                            </Link>
                                        </RowStack.Fit>
                                        <RowStack.Fit>
                                            <Link
                                                data-tid='Rerun'
                                                disabled={!rerunableStates.includes(taskInfo.state)}
                                                onClick={onRerun}
                                                icon='refresh'>
                                                Rerun
                                            </Link>
                                        </RowStack.Fit>
                                    </RowStack>
                                </ColumnStack.Fit>}
                            </RowStack.Fit>
                            <RowStack.Fit className={cn('dates')}>
                                {taskDate(taskInfo, 'Enqueued', 'enqueueDateTime')}
                                {taskDate(taskInfo, 'Started', 'startExecutingDateTime')}
                                {taskDate(taskInfo, 'Finished', 'finishExecutingDateTime')}
                                {taskDate(taskInfo, 'StateTime', 'minimalStartDateTime')}
                                {taskDate(taskInfo, 'Expiration', 'expirationTimestamp')}
                            </RowStack.Fit>
                        </RowStack>
                    </ColumnStack.Fit>
                </ColumnStack>
            </RowStack.Fit>
        </RowStack>
    );
}
