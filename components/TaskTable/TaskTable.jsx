// @flow
import React from 'react';
import type { TaskMetaInformationModel } from '../../api/RemoteTaskQueueApi';
import type { TaskState } from '../../Domain/TaskState';
import { TaskStates } from '../../Domain/TaskState';
import moment from 'moment';
import {
    Button,
    Modal,
} from 'ui';
import { RowStack, ColumnStack } from 'ui/layout';
import cn from './TaskTable.less';
import { cancelableStates, rerunableStates } from '../../Domain/TaskState';
import { TasksPath } from '../../reducers/RemoteTaskQueueReducer';

export type TaskTableProps = {
    taskInfos: TaskMetaInformationModel[];
    allowRerunOrCancel: boolean;
    currentUrl?: string;
    onRerun: (id: string) => any;
    onCancel: (id: string) => any;
};

type TasksTableState = {
    openedModal: boolean;
    modalType: 'Cancel' | 'Rerun';
    actionTask: string;
};

export default class TasksTable extends React.Component {
    props: TaskTableProps;
    state: TasksTableState;

    componentWillMount() {
        this.state = {
            openedModal: false,
            modalType: 'Cancel',
            actionTask: '',
        };
    }

    render(): React.Element<*> {
        const { taskInfos } = this.props;
        const { openedModal } = this.state;
        return (
            <div>
                <table className={cn('table')}>
                    <thead>
                    <tr>
                        {this.renderHeader()}
                    </tr>
                    </thead>
                    <tbody tid='Rows'>
                    {taskInfos.map(item => this.renderRow(item))}
                    </tbody>
                </table>
                { openedModal && this.renderModal() }
            </div>
        );
    }

    renderHeader(): React.Element<*>[] {
        return [
            <th key='TaskId'>Id</th>,
            <th key='TaskState'>State</th>,
            <th key='TaskName'>Name</th>,
            <th key='EnqueueTime'>EnqueueTime</th>,
            <th key='StartExecutingTime'>StartExecutingTime</th>,
            <th key='FinishExecutingTime'>FinishExecutingTime</th>,
            <th key='MinimalStartTime'>MinimalStartTime</th>,
            <th key='ExpirationTime'>ExpirationTime</th>,
            <th key='Attempts'>Attempts</th>,
            <th key='ParentTaskId'>ParentTaskId</th>,
        ];
    }

    renderRow(item: TaskMetaInformationModel): React.Element<*> {
        return (
            <tr key={item.id} tid={'Row'} className={cn(this.chooseRowColor(item.state))}>
                <td>{this.renderFirstCol(item)}</td>
                <td>{TaskStates[item.state]}</td>
                <td><span className={cn('break-text')}>{item.name}</span></td>
                <td>{dateFormatter(item, 'enqueueDateTime')}</td>
                <td>{dateFormatter(item, 'startExecutingDateTime')}</td>
                <td>{dateFormatter(item, 'finishExecutingDateTime')}</td>
                <td>{dateFormatter(item, 'minimalStartDateTime')}</td>
                <td>{dateFormatter(item, 'expirationTimestamp')}</td>
                <td>{item.attempts ? String(item.attempts) : ''}</td>
                <td>{item.parentTaskId ? String(item.parentTaskId) : ''}</td>
            </tr>
        );
    }

    renderModal(): React.Element<*> {
        const { onCancel, onRerun } = this.props;
        const { modalType, actionTask } = this.state;

        return (
            <Modal onClose={() => this.closeModal()} width={500} tid='Modal'>
                <Modal.Header>
                    Нужно подтверждение
                </Modal.Header>
                <Modal.Body>
                    <span tid='ModalText'>
                    {modalType === 'Rerun'
                        ? 'Уверен, что таску надо перезапустить?'
                        : 'Уверен, что таску надо остановить?'
                    }
                    </span>
                </Modal.Body>
                <Modal.Footer>
                    <RowStack gap={2}>
                        <RowStack.Fit>
                            {modalType === 'Rerun'
                                ? <Button
                                    tid='ModalRerunButton'
                                    use='success'
                                    onClick={() => {
                                        onRerun(actionTask);
                                        this.closeModal();
                                    }}>Перезапустить</Button>
                                : <Button
                                    tid='ModalCancelButton'
                                    use='danger'
                                    onClick={() => {
                                        onCancel(actionTask);
                                        this.closeModal();
                                    }}>Остановить</Button>
                            }
                        </RowStack.Fit>
                        <RowStack.Fit>
                            <Button onClick={() => this.closeModal()}>Закрыть</Button>
                        </RowStack.Fit>
                    </RowStack>
                </Modal.Footer>
            </Modal>
        );
    }

    renderFirstCol(item: TaskMetaInformationModel): React.Element<*> {
        const { allowRerunOrCancel } = this.props;
        return (
            <ColumnStack gap={1}>
                <ColumnStack.Fit>
                    <a
                        href={`${TasksPath}/${item.id}`}
                        tid={'Link'}
                        onClick={() => this.addLinkInLocalStorage(item.id)}>
                        {item.id}
                    </a>
                </ColumnStack.Fit>
                <ColumnStack.Fit>
                    {allowRerunOrCancel && <RowStack gap={1}>
                        {cancelableStates.includes(item.state) && (
                            <RowStack.Fit>
                                <Button
                                    use='danger'
                                    tid={'CancelButton'}
                                    onClick={() => this.cancel(item.id)}>Cancel</Button>
                            </RowStack.Fit>
                        )}
                        {rerunableStates.includes(item.state) && (
                            <RowStack.Fit>
                                <Button
                                    use='success'
                                    tid={'RerunButton'}
                                    onClick={() => this.rerun(item.id)}>Rerun</Button>
                            </RowStack.Fit>
                        )}
                    </RowStack>}
                </ColumnStack.Fit>
            </ColumnStack>);
    }

    chooseRowColor(taskState: TaskState): string {
        switch (taskState) {
            case TaskStates.Finished:
                return 'green';
            case TaskStates.Fatal:
            case TaskStates.Canceled:
                return 'red';
            case TaskStates.WaitingForRerun:
            case TaskStates.WaitingForRerunAfterError:
                return 'gray';
            default:
                return '';
        }
    }

    rerun(id: string): any {
        this.setState({
            openedModal: true,
            modalType: 'Rerun',
            actionTask: id,
        });
    }

    cancel(id: string): any {
        this.setState({
            openedModal: true,
            modalType: 'Cancel',
            actionTask: id,
        });
    }

    closeModal(): any {
        this.setState({
            openedModal: false,
        });
    }

    addLinkInLocalStorage(id: string) {
        const { currentUrl } = this.props;
        if (!currentUrl || !localStorage) {
            return;
        }

        const inStorage = localStorage.getItem('taskQueueLinks');
        let result = inStorage ? JSON.parse(inStorage) : {};
        if (Object.keys(result).length > 850) { //больше 100 килобайт занимает
            result = {};
        }
        result[id] = currentUrl;
        localStorage.setItem('taskQueueLinks', JSON.stringify(result));
    }
}

function dateFormatter(item: TaskMetaInformationModel, column: string): React.Element<*> | string {
    if (typeof item[column] === 'undefined') {
        return '';
    }
    const date = new Date(item[column]);
    const formattedDate = moment(date)
                        .utcOffset('+0300')
                        .locale('ru')
                        .format('YYYY.MM.DD HH:mm:ss.SSS Z');

    return (<span tid={'Date' + column}>{formattedDate}</span>);
}
