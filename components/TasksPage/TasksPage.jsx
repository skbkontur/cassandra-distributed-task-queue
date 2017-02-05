// @flow
import React from 'react';
import {
    Loader,
    Button,
    Modal,
    Input,
} from 'ui';
import { RowStack, ColumnStack } from 'ui/layout';

import TaskQueueFilter from '../../containers/TaskQueueFilterConnected';
import TasksTable from '../../containers/TasksTableConnected';
import TasksPaginator from '../../containers/TasksPaginatorConnected';
import TaskActionResult from '../TaskActionResult/TaskActionResult';

import cn from './TasksPage.less';
import numberToString from '../../Domain/numberToString';

export type TasksPageType = {
    loading: boolean;
    counter: number;
    error?: string;
    allowRerunOrCancel: boolean;
    actionsOnTaskResult?: any;
    onRerunAll: () => any;
    onCancelAll: () => any;
};

type TasksPageState = {
    openedModal: boolean;
    modalType: 'Cancel' | 'Rerun';
    manyTaskConfirm: string;
};

export default class TasksPage extends React.Component {
    props: TasksPageType;
    state: TasksPageState = {
        openedModal: false,
        modalType: 'Cancel',
        manyTaskConfirm: '',
    };

    render(): React.Element<*> {
        const { counter, loading, error, actionsOnTaskResult, allowRerunOrCancel } = this.props;
        const { openedModal } = this.state;
        return (
            <div className={cn('page-wrapper')}>
                <Loader type='big' active={loading} tid={'Loader'}>
                    <header className={cn('header')}>
                        <h1 className={cn('header-title')}>Список задач</h1>
                        <a href='/AdminTools'>вернуться к инструментам администратора</a>
                    </header>
                    <div className={cn('filter-wrapper')}>
                        <TaskQueueFilter />
                    </div>
                    { error && <div className={cn('error-wrapper')}>
                        {this.renderError()}
                    </div>
                    }
                    { actionsOnTaskResult && <div className={cn('action-result-wrapper')}>
                        <TaskActionResult
                            actionResult={actionsOnTaskResult}
                            showTasks={Object.keys(actionsOnTaskResult).length > 1}
                        />
                    </div>
                    }
                    { counter > 0 && (
                        <div tid={'ResultsWrapper'}>
                            <p className={cn('counter')} tid={'ResultCount'}>
                                Всего результатов: {counter}
                            </p>
                            {allowRerunOrCancel && <RowStack
                                gap={2}
                                tid={'ButtonsWrapper'}
                                className={cn('button-wrapper')}>
                                <RowStack.Fit>
                                    <Button
                                        use='danger'
                                        tid={'CancelAllButton'}
                                        onClick={() => this.clickCancelAll()}>Cancel All</Button>
                                </RowStack.Fit>
                                <RowStack.Fit>
                                    <Button
                                        use='success'
                                        tid={'RerunAllButton'}
                                        onClick={() => this.clickRerunAll()}>Rerun All</Button>
                                </RowStack.Fit>
                            </RowStack>}
                            <TasksTable />
                            <div className={cn('paginator-wrapper')}>
                                <TasksPaginator />
                            </div>
                        </div>
                    )
                    }
                    { openedModal && this.renderModal() }
                </Loader>
            </div>
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
        const { onCancelAll, onRerunAll, counter } = this.props;
        const { modalType, manyTaskConfirm } = this.state;
        const confirmedRegExp = /б.*л.*я/i;

        return (
            <Modal onClose={() => this.closeModal()} width={500} tid='Modal'>
                <Modal.Header>
                    Нужно подтверждение
                </Modal.Header>
                <Modal.Body>
                    <ColumnStack gap={2}>
                        <ColumnStack.Fit>
                            <span tid='ModalText'>
                            {modalType === 'Rerun'
                                ? 'Уверен, что все эти таски надо перезапустить?'
                                : 'Уверен, что все эти таски надо остановить?'
                            }
                            </span>
                        </ColumnStack.Fit>
                    {counter > 100 &&
                        [<ColumnStack.Fit key='text'>
                            <span tid='ModalManyTasksText'>
                                Это действие может задеть больше 100 тасок, если это точно надо сделать,
                                то напиши прописью количество тасок (их { counter }):
                            </span>
                        </ColumnStack.Fit>,
                        <ColumnStack.Fit key='input'>
                            <Input
                                tid='ModalInput'
                                value={manyTaskConfirm}
                                onChange={(e, val) => this.setState({ manyTaskConfirm: val })}
                            />
                        </ColumnStack.Fit>]
                    }
                    </ColumnStack>
                </Modal.Body>
                <Modal.Footer>
                    <RowStack gap={2}>
                        <RowStack.Fit>
                            {modalType === 'Rerun'
                                ? <Button
                                    tid='ModalRerunButton'
                                    use='success'
                                    disabled={counter > 100 &&
                                        (!confirmedRegExp.test(manyTaskConfirm) &&
                                         manyTaskConfirm !== numberToString(counter))}
                                    onClick={() => {
                                        onRerunAll();
                                        this.closeModal();
                                    }}>Перезапустить все</Button>
                                : <Button
                                    tid='ModalCancelButton'
                                    use='danger'
                                    disabled={counter > 100 &&
                                        (!confirmedRegExp.test(manyTaskConfirm) &&
                                         manyTaskConfirm !== numberToString(counter))}
                                    onClick={() => {
                                        onCancelAll();
                                        this.closeModal();
                                    }}>Остановить все</Button>
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

    clickRerunAll(): any {
        this.setState({
            openedModal: true,
            modalType: 'Rerun',
        });
    }

    clickCancelAll(): any {
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
