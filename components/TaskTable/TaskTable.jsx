// @flow
import React from 'react';
import { Button, Modal } from 'ui';
import { RowStack } from 'ui/layout';
import TaskDetails from './TaskDetails/TaskDetails';
import type { TaskMetaInformationModel } from '../../api/RemoteTaskQueueApi';
import type { RouterLocationDescriptor } from '../../../Commons/DataTypes/Routing';
import cn from './TaskTable.less';

export type TaskTableProps = {
    taskInfos: TaskMetaInformationModel[];
    allowRerunOrCancel: boolean;
    currentUrl?: string;
    onRerun: (id: string) => any;
    onCancel: (id: string) => any;
    getTaskLocation: (id: string) => RouterLocationDescriptor;
};

type TasksTableState = {
    openedModal: boolean;
    modalType: 'Cancel' | 'Rerun';
    actionTask: string;
};

export default class TasksTable extends React.Component {
    props: TaskTableProps;
    state: TasksTableState = {
        openedModal: false,
        modalType: 'Cancel',
        actionTask: '',
    };

    render(): React.Element<*> {
        const { taskInfos } = this.props;
        const { openedModal } = this.state;
        return (
            <div>
                <div data-tid='Tasks'>
                    {taskInfos.map(item => this.renderRow(item))}
                </div>
                {openedModal && this.renderModal()}
            </div>
        );
    }

    renderRow(item: TaskMetaInformationModel): React.Element<*> {
        const { allowRerunOrCancel, getTaskLocation } = this.props;
        return (
            <div key={item.id} className={cn('task-details-row')}>
                <TaskDetails
                    getTaskLocation={getTaskLocation}
                    data-tid='Task'
                    onCancel={() => this.cancel(item.id)}
                    onRerun={() => this.rerun(item.id)}
                    taskInfo={item}
                    allowRerunOrCancel={allowRerunOrCancel}
                />
            </div>
        );
    }

    renderModal(): React.Element<*> {
        const { onCancel, onRerun } = this.props;
        const { modalType, actionTask } = this.state;

        return (
            <Modal onClose={() => this.closeModal()} width={500} data-tid='ConfirmOperationModal'>
                <Modal.Header>
                    Нужно подтверждение
                </Modal.Header>
                <Modal.Body>
                    <span data-tid='ModalText'>
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
                                    data-tid='RerunButton'
                                    use='success'
                                    onClick={() => {
                                        onRerun(actionTask);
                                        this.closeModal();
                                    }}>Перезапустить</Button>
                                : <Button
                                    data-tid='CancelButton'
                                    use='danger'
                                    onClick={() => {
                                        onCancel(actionTask);
                                        this.closeModal();
                                    }}>Остановить</Button>
                            }
                        </RowStack.Fit>
                        <RowStack.Fit>
                            <Button data-tid='CloseButton' onClick={() => this.closeModal()}>Закрыть</Button>
                        </RowStack.Fit>
                    </RowStack>
                </Modal.Footer>
            </Modal>
        );
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
}

