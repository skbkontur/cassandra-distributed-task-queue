// @flow
import React from 'react';
import cn from './TaskDetailsPage.less';
import {
    Loader,
    Button,
    Modal,
} from 'ui';
import { RowStack, ColumnStack } from 'ui/layout';
import { cancelableStates, rerunableStates } from '../../Domain/TaskState';
import TaskDetailsMetaTable from '../TaskDetailsMetaTable/TaskDetailsMetaTable';
import TaskAccordion from '../TaskAccordion/TaskAccordion';
import TaskActionResult from '../TaskActionResult/TaskActionResult';
import { TasksPath } from '../../reducers/RemoteTaskQueueReducer';
import customRender from '../../Domain/CustomRender';
import type { RemoteTaskInfoModel } from '../../api/RemoteTaskQueueApi';

export type TaskDetailsPageProps = {
    loading: boolean;
    taskDetails: RemoteTaskInfoModel;
    error?: string;
    actionsOnTaskResult?: any;
    allowRerunOrCancel: boolean;
    onRerun: (id: string) => any;
    onCancel: (id: string) => any;
};

type TaskDetailsPageState = {
    openedModal: boolean;
    modalType: 'Cancel' | 'Rerun';
};

export default class TaskDetailsPage extends React.Component {
    props: TaskDetailsPageProps;
    state: TaskDetailsPageState;

    componentWillMount() {
        this.state = {
            openedModal: false,
            modalType: 'Cancel',
        };
    }

    render(): React.Element<*> {
        const { loading, taskDetails, error, actionsOnTaskResult } = this.props;
        const { openedModal } = this.state;

        return (
            <div className={cn('page-wrapper')}>
                <Loader type='big' active={loading} tid={'Loader'}>
                    {taskDetails && this.renderHeader()}
                    {error && this.renderError()}
                    {actionsOnTaskResult &&
                        <div className={cn('action-result-wrapper')}>
                            <TaskActionResult actionResult={actionsOnTaskResult} showTasks={false} />
                        </div>
                    }
                    {taskDetails && <TaskDetailsMetaTable taskMeta={taskDetails.taskMeta} />}
                    {taskDetails &&
                        <div className={cn('accordion-wrapper')}>
                            <TaskAccordion
                                customRender={customRender}
                                value={taskDetails.taskData}
                                title='TaskData'
                            />
                        </div>
                    }
                    {taskDetails && taskDetails.exceptionInfos &&
                        <div className={cn('exceptions-wrapper')}>
                            {taskDetails.exceptionInfos.map((exception, index) => {
                                return (
                                    <pre key={index} className={cn('exception')}>{exception.exceptionMessageInfo}</pre>
                                );
                            })}
                        </div>}
                    { openedModal && this.renderModal() }
                </Loader>
            </div>
        );
    }

    renderHeader(): React.Element<*> {
        const { taskDetails, allowRerunOrCancel } = this.props;
        return (
            <ColumnStack gap={3} className={cn('header')}>
                <ColumnStack.Fit>
                    <h1 data-tid='Header' className={cn('header-title')}>Задача {taskDetails.taskMeta.name}</h1>
                    <a href={this.createBackUrl()}>вернуться к поиску</a>
                </ColumnStack.Fit>
                <ColumnStack.Fit>
                    <p className={cn('header-id')}>Id: {taskDetails.taskMeta.id}</p>
                </ColumnStack.Fit>
                {allowRerunOrCancel && this.renderButton()}
            </ColumnStack>
        );
    }

    renderButton(): React.Element<*> | null {
        const { taskDetails } = this.props;
        const isCancelable = cancelableStates.includes(taskDetails.taskMeta.state);
        const isRerunable = rerunableStates.includes(taskDetails.taskMeta.state);

        if (!isCancelable && !isRerunable) {
            return null;
        }

        return (
            <ColumnStack.Fit>
                <RowStack
                    gap={2}
                    className={cn('button-wrapper')}>
                    {isCancelable &&
                        <RowStack.Fit>
                            <Button
                                use='danger'
                                tid={'CancelButton'}
                                onClick={() => this.cancel()}>Cancel</Button>
                        </RowStack.Fit>}
                    {isRerunable &&
                        <RowStack.Fit>
                            <Button
                                use='success'
                                tid={'RerunButton'}
                                onClick={() => this.rerun()}>Rerun</Button>
                        </RowStack.Fit>}
                </RowStack>
            </ColumnStack.Fit>
        );
    }

    renderError(): React.Element<*> {
        const { error } = this.props;

        return (
            <div className={cn('error')} tid='Error'>
                {error}
            </div>
        );
    }

    renderModal(): React.Element<*> {
        const { onCancel, onRerun, taskDetails } = this.props;
        const { modalType } = this.state;

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
                                        onRerun(taskDetails.taskMeta.id);
                                        this.closeModal();
                                    }}>Перезапустить</Button>
                                : <Button
                                    tid='ModalCancelButton'
                                    use='danger'
                                    onClick={() => {
                                        onCancel(taskDetails.taskMeta.id);
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

    createBackUrl(): string {
        const { taskDetails } = this.props;
        const inStorage = window.localStorage && window.localStorage.getItem('taskQueueLinks');
        if (!inStorage) {
            return TasksPath;
        }
        const storageLinksObj = JSON.parse(inStorage);
        return storageLinksObj[taskDetails.taskMeta.id] || TasksPath;
    }

    rerun(): any {
        this.setState({
            openedModal: true,
            modalType: 'Rerun',
        });
    }

    cancel(): any {
        this.setState({
            openedModal: true,
            modalType: 'Cancel',
        });
    }

    closeModal(): any {
        this.setState({
            openedModal: false,
        });
    }
}


