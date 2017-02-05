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
                <td>{taskMeta.id}</td>
            </tr>,
            <tr key='TaskState'>
                <td>TaskState</td>
                <td>{taskMeta.state}</td>
            </tr>,
            <tr key='Name'>
                <td>Name</td>
                <td>{taskMeta.name}</td>
            </tr>,
            <tr key='EnqueueTime'>
                <td>EnqueueTime</td>
                <td>{formatterDate(taskMeta.enqueueDateTime)}</td>
            </tr>,
            <tr key='StartExecutedTime'>
                <td>StartExecutedTime</td>
                <td>{formatterDate(taskMeta.startExecutingDateTime)}</td>
            </tr>,
            <tr key='FinishExecutedTime'>
                <td>FinishExecutedTime</td>
                <td>{formatterDate(taskMeta.finishExecutingDateTime)}</td>
            </tr>,
            <tr key='MinimalStartTime'>
                <td>MinimalStartTime</td>
                <td>{formatterDate(taskMeta.minimalStartDateTime)}</td>
            </tr>,
            <tr key='ExpirationTime'>
                <td>ExpirationTime</td>
                <td>{formatterDate(taskMeta.expirationTimestamp)}</td>
            </tr>,
            <tr key='Attempts'>
                <td>Attempts</td>
                <td>{taskMeta.attempts}</td>
            </tr>,
            <tr key='ParentTaskId'>
                <td>ParentTaskId</td>
                <td>
                    {taskMeta.parentTaskId &&
                    <a href={'/AdminTools/Tasks/' + taskMeta.parentTaskId}>{taskMeta.parentTaskId}</a>
                    }
                </td>
            </tr>,
            <tr key='ChildTaskIds'>
                <td>ChildTaskIds</td>
                <td>{taskMeta.childTaskIds && taskMeta.childTaskIds.map(item => {
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
                            .format('YYYY.MM.DD HH:mm:ss');
    const timeStamp = copyDate.getTime();
    return formattedDate + ' (МСК) (' + timeStamp + ')';
}
