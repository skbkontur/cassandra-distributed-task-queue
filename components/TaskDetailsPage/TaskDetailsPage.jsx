// @flow
import React from 'react';
import cn from './TaskDetailsPage.less';
import {
    Loader,
    Button,
    Modal,
} from 'ui';
import { RowStack, ColumnStack } from 'ui/layout';
import CommonLayout from '../../../Commons/Layouts';
import { cancelableStates, rerunableStates } from '../../Domain/TaskState';
import TaskDetailsMetaTable from '../TaskDetailsMetaTable/TaskDetailsMetaTable';
import TaskAccordion from '../TaskAccordion/TaskAccordion';
import TaskActionResult from '../TaskActionResult/TaskActionResult';
import customRender from '../../Domain/CustomRender';
import type { RemoteTaskInfoModel } from '../../api/RemoteTaskQueueApi';
import type { RouterLocationDescriptor } from '../../../Commons/DataTypes/Routing';

export type TaskDetailsPageProps = {
    parentLocation: RouterLocationDescriptor;
    taskDetails: ?RemoteTaskInfoModel;
    loading: boolean;
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
        const { allowRerunOrCancel, loading, taskDetails, error, actionsOnTaskResult, parentLocation } = this.props;
        const { openedModal } = this.state;

        return (
            <Loader type='big' active={loading} data-tid={'Loader'}>
                <CommonLayout>
                    {taskDetails && <CommonLayout.GoBack to={parentLocation}>
                        Вернуться к поиску задач
                    </CommonLayout.GoBack>}
                    {taskDetails && (
                        <CommonLayout.GreyLineHeader
                            data-tid='Header'
                            title={`Задача ${taskDetails.taskMeta.name}`}
                        />
                    )}
                    <CommonLayout.Content>
                        <ColumnStack block streach gap={2}>
                            {taskDetails && allowRerunOrCancel && (
                                <ColumnStack.Fit>
                                    {this.renderButton()}
                                </ColumnStack.Fit>
                            )}
                            {error && <ColumnStack.Fit>{this.renderError()}</ColumnStack.Fit>}
                            {actionsOnTaskResult &&
                                <ColumnStack.Fit>
                                    <TaskActionResult actionResult={actionsOnTaskResult} showTasks={false} />
                                </ColumnStack.Fit>}
                            {taskDetails && (
                                <ColumnStack.Fit>
                                    <TaskDetailsMetaTable taskMeta={taskDetails.taskMeta} />
                                </ColumnStack.Fit>
                            )}
                            {taskDetails &&
                                <ColumnStack.Fit>
                                    <TaskAccordion
                                        customRender={customRender}
                                        value={taskDetails.taskData}
                                        title='TaskData'
                                    />
                                </ColumnStack.Fit>
                            }
                            {taskDetails && taskDetails.exceptionInfos && (
                                <ColumnStack.Fit data-tid='Exceptions'>
                                    {taskDetails.exceptionInfos.map((exception, index) => {
                                        return (
                                            <pre
                                                data-tid='Exception'
                                                key={index}
                                                className={cn('exception')}>
                                                {exception.exceptionMessageInfo}
                                            </pre>
                                        );
                                    })}
                                </ColumnStack.Fit>
                            )}
                        </ColumnStack>
                    </CommonLayout.Content>
                </CommonLayout>
                { openedModal && this.renderModal() }
            </Loader>
        );
    }

    renderButton(): React.Element<*> | null {
        const { taskDetails } = this.props;
        if (!taskDetails) {
            return null;
        }
        const isCancelable = cancelableStates.includes(taskDetails.taskMeta.state);
        const isRerunable = rerunableStates.includes(taskDetails.taskMeta.state);

        if (!isCancelable && !isRerunable) {
            return null;
        }

        return (
            <RowStack gap={2}>
                {isCancelable &&
                    <RowStack.Fit>
                        <Button
                            use='danger'
                            data-tid={'CancelButton'}
                            onClick={() => this.cancel()}>Cancel</Button>
                    </RowStack.Fit>}
                {isRerunable &&
                    <RowStack.Fit>
                        <Button
                            use='success'
                            data-tid={'RerunButton'}
                            onClick={() => this.rerun()}>Rerun</Button>
                    </RowStack.Fit>}
            </RowStack>
        );
    }

    renderError(): React.Element<*> {
        const { error } = this.props;

        return (
            <div className={cn('error')} data-tid='Error'>
                {error}
            </div>
        );
    }

    renderModal(): React.Element<*> | null {
        const { onCancel, onRerun, taskDetails } = this.props;
        const { modalType } = this.state;
        if (!taskDetails) {
            return null;
        }
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
                                        onRerun(taskDetails.taskMeta.id);
                                        this.closeModal();
                                    }}>Перезапустить</Button>
                                : <Button
                                    data-tid='CancelButton'
                                    use='danger'
                                    onClick={() => {
                                        onCancel(taskDetails.taskMeta.id);
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


