// @flow
import React from 'react';
import moment from 'moment';
import cn from './TaskDetailsMetaTable.less';
import type { TaskMetaInformationModel } from '../../api/RemoteTaskQueueApi';

export type TaskDetailsMetaTableProps = {
    taskMeta: TaskMetaInformationModel;
};

export default class TaskDetailsMetaTable extends React.Component {
    props: TaskDetailsMetaTableProps;

    render(): React.Element<*> {
        return (
            <table className={cn('table')}>
                <tbody>
                {this.renderMetaInfo()}
                </tbody>
            </table>
        );
    }

    renderMetaInfo(): React.Element<*>[] {
        const { taskMeta } = this.props;
        return [
            <tr key='TaskId'>
                <td>TaskId</td>
                <td data-tid='TaskId'>{taskMeta.id}</td>
            </tr>,
            <tr key='TaskState'>
                <td>TaskState</td>
                <td data-tid='TaskState'>{taskMeta.state}</td>
            </tr>,
            <tr key='Name'>
                <td>Name</td>
                <td data-tid='Name'>{taskMeta.name}</td>
            </tr>,
            <tr key='EnqueueTime'>
                <td>EnqueueTime</td>
                <td data-tid='EnqueueTime'>{formatterDate(taskMeta.enqueueDateTime)}</td>
            </tr>,
            <tr key='StartExecutedTime'>
                <td>StartExecutedTime</td>
                <td data-tid='StartExecutedTime'>{formatterDate(taskMeta.startExecutingDateTime)}</td>
            </tr>,
            <tr key='FinishExecutedTime'>
                <td>FinishExecutedTime</td>
                <td data-tid='FinishExecutedTime'>{formatterDate(taskMeta.finishExecutingDateTime)}</td>
            </tr>,
            <tr key='MinimalStartTime'>
                <td>MinimalStartTime</td>
                <td data-tid='MinimalStartTime'>{formatterDate(taskMeta.minimalStartDateTime)}</td>
            </tr>,
            <tr key='ExpirationTime'>
                <td>ExpirationTime</td>
                <td data-tid='ExpirationTime'>{formatterDate(taskMeta.expirationTimestamp)}</td>
            </tr>,
            <tr key='ExpirationModificationTime'>
                <td>ExpirationModificationTime</td>
                <td data-tid='ExpirationModificationTime'>{formatterDate(taskMeta.expirationModificationDateTime)}</td>
            </tr>,
            <tr key='LastModificationTime'>
                <td>LastModificationTime</td>
                <td data-tid='LastModificationTime'>{formatterDate(taskMeta.lastModificationDateTime)}</td>
            </tr>,
            <tr key='Attempts'>
                <td>Attempts</td>
                <td data-tid='Attempts'>{taskMeta.attempts}</td>
            </tr>,
            <tr key='ParentTaskId'>
                <td>ParentTaskId</td>
                <td data-tid='ParentTaskId'>
                    {taskMeta.parentTaskId &&
                    <a href={'/AdminTools/Tasks/' + taskMeta.parentTaskId}>{taskMeta.parentTaskId}</a>
                    }
                </td>
            </tr>,
            <tr key='ChildTaskIds'>
                <td>ChildTaskIds</td>
                <td data-tid='ChildTaskIds'>{taskMeta.childTaskIds && taskMeta.childTaskIds.map(item => {
                    return (
                        <span key={item}>
                            <a href={'/AdminTools/Tasks/' + item}>{item}</a><br/>
                        </span>
                    );
                })}</td>
            </tr>,
        ];
    }
}

function formatterDate(date?: ?string): string {
    if (!date) {
        return '';
    }

    const copyDate = new Date(date);
    const formattedDate = moment(copyDate)
                            .utcOffset('+0300')
                            .locale('ru')
                            .format('YYYY.MM.DD HH:mm:ss.SSS Z');
    return formattedDate + ' (' + date + ')';
}
