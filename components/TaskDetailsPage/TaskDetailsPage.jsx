// @flow
import React from 'react';
import cn from './TaskDetailsPage.less';
import {
    Button,
    Modal,
    ButtonLink,
    RouterLink,
} from 'ui';
import { RowStack, ColumnStack } from 'ui/layout';
import CommonLayout from '../../../Commons/Layouts';
import { cancelableStates, rerunableStates } from '../../Domain/TaskState';
import TaskDetailsMetaTable from '../TaskDetailsMetaTable/TaskDetailsMetaTable';
import TaskAccordion from '../TaskAccordion/TaskAccordion';
import customRender from '../../Domain/CustomRender';
import TaskTimeLine from '../TaskTimeLine/TaskTimeLine';
import type { RemoteTaskInfoModel } from '../../api/RemoteTaskQueueApi';
import type { RouterLocationDescriptor } from '../../../Commons/DataTypes/Routing';
import { buildSearchQueryForRequest } from '../../containers/TasksPageContainer';
import RangeSelector from '../../../Commons/DateTimeRangePicker/RangeSelector';
import { TimeZones, ticksToDate } from '../../../Commons/DataTypes/Time';

export type TaskDetailsPageProps = {
    parentLocation: RouterLocationDescriptor;
    taskDetails: ?RemoteTaskInfoModel;
    getTaskLocation: (id: string) => RouterLocationDescriptor;
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
        const { allowRerunOrCancel, getTaskLocation, taskDetails, parentLocation } = this.props;
        const { openedModal } = this.state;

        return (
            <div>
                <CommonLayout>
                    {taskDetails && <CommonLayout.GoBack to={parentLocation}>
                        Вернуться к поиску задач
                    </CommonLayout.GoBack>}
                    {taskDetails && (
                        <CommonLayout.GreyLineHeader
                            data-tid='Header'
                            title={`Задача ${taskDetails.taskMeta.name}`}
                            tools={(taskDetails && allowRerunOrCancel) && this.renderButtons()}>
                            <TaskTimeLine
                                getHrefToTask={getTaskLocation}
                                taskMeta={taskDetails.taskMeta}
                            />
                        </CommonLayout.GreyLineHeader>
                    )}
                    <CommonLayout.Content>
                        <ColumnStack block streach gap={2}>
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
                                <ColumnStack.Fit
                                    className={cn('exception-container')}
                                    data-tid='Exceptions'>
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
            </div>
        );
    }

    getRelatedTasksLocation(taskDetails: RemoteTaskInfoModel): ?RouterLocationDescriptor {
        const documentCirculationId =
            (taskDetails.taskData && (typeof taskDetails.taskData.documentCirculationId === 'string'))
                ? taskDetails.taskData.documentCirculationId
                : null;
        if (documentCirculationId != null && taskDetails.taskMeta.ticks != null) {
            const rangeSelector = new RangeSelector(TimeZones.UTC);

            return {
                pathname: '/AdminTools/Tasks/Tree',
                search: buildSearchQueryForRequest({
                    enqueueDateTimeRange: rangeSelector.getMonthOf(ticksToDate(taskDetails.taskMeta.ticks)),
                    queryString:
                        'Data.DocumentCirculationId:' +
                        `"${documentCirculationId || ''}"`,
                    names: [],
                    states: [],
                }),
            };
        }
        return null;
    }

    renderButtons(): React.Element<*> | null {
        const { taskDetails } = this.props;
        if (!taskDetails) {
            return null;
        }
        const isCancelable = cancelableStates.includes(taskDetails.taskMeta.state);
        const isRerunable = rerunableStates.includes(taskDetails.taskMeta.state);
        const relatedTasksLocation = this.getRelatedTasksLocation(taskDetails);
        if (!isCancelable && !isRerunable && relatedTasksLocation == null) {
            return null;
        }

        return (
            <RowStack baseline block gap={2}>
                <RowStack.Fill />
                {relatedTasksLocation &&
                    <RowStack.Fit>
                        <RouterLink
                            icon='list'
                            to={relatedTasksLocation}>
                            View related tasks tree
                        </RouterLink>
                    </RowStack.Fit>}
                {isCancelable &&
                    <RowStack.Fit>
                        <ButtonLink
                            icon='remove'
                            use='danger'
                            data-tid={'CancelButton'}
                            onClick={() => this.cancel()}>
                            Cancel task
                        </ButtonLink>
                    </RowStack.Fit>}
                {isRerunable &&
                    <RowStack.Fit>
                        <ButtonLink
                            icon='refresh'
                            data-tid={'RerunButton'}
                            onClick={() => this.rerun()}>
                            Rerun task
                        </ButtonLink>
                    </RowStack.Fit>}
            </RowStack>
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


