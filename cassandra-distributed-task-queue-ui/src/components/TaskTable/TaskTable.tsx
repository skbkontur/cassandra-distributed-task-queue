import { Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Modal } from "@skbkontur/react-ui";
import { LocationDescriptor } from "history";
import _ from "lodash";
import React from "react";

import { RtqMonitoringTaskMeta } from "../../Domain/Api/RtqMonitoringTaskMeta";

import { TaskDetails } from "./TaskDetails/TaskDetails";
import { jsStyles } from "./TaskTable.styles";

export interface TaskTableProps {
    taskInfos: RtqMonitoringTaskMeta[];
    allowRerunOrCancel: boolean;
    onRerun: (x0: string) => void;
    onCancel: (x0: string) => void;
    getTaskLocation: (x0: string) => LocationDescriptor;
}

interface TasksTableState {
    openedModal: boolean;
    modalType: "Cancel" | "Rerun";
    actionTask: string;
}

export class TasksTable extends React.Component<TaskTableProps, TasksTableState> {
    public state: TasksTableState = {
        openedModal: false,
        modalType: "Cancel",
        actionTask: "",
    };

    public shouldComponentUpdate(nextProps: TaskTableProps, nextState: TasksTableState): boolean {
        return (
            !_.isEqual(this.props.taskInfos, nextProps.taskInfos) ||
            !_.isEqual(this.props.allowRerunOrCancel, nextProps.allowRerunOrCancel) ||
            !_.isEqual(this.state.openedModal, nextState.openedModal) ||
            !_.isEqual(this.state.modalType, nextState.modalType) ||
            !_.isEqual(this.state.actionTask, nextState.actionTask)
        );
    }

    public render(): JSX.Element {
        const { taskInfos } = this.props;
        const { openedModal } = this.state;
        return (
            <div>
                <div data-tid="Tasks">{taskInfos.map(item => this.renderRow(item))}</div>
                {openedModal && this.renderModal()}
            </div>
        );
    }

    public renderRow(item: RtqMonitoringTaskMeta): JSX.Element {
        const { allowRerunOrCancel, getTaskLocation } = this.props;
        return (
            <div key={item.id} className={jsStyles.taskDetailsRow()}>
                <TaskDetails
                    getTaskLocation={getTaskLocation}
                    data-tid="Task"
                    onCancel={() => this.cancel(item.id)}
                    onRerun={() => this.rerun(item.id)}
                    taskInfo={item}
                    allowRerunOrCancel={allowRerunOrCancel}
                />
            </div>
        );
    }

    public renderModal(): JSX.Element {
        const { onCancel, onRerun } = this.props;
        const { modalType, actionTask } = this.state;

        return (
            <Modal onClose={() => this.closeModal()} width={500} data-tid="ConfirmOperationModal">
                <Modal.Header>Нужно подтверждение</Modal.Header>
                <Modal.Body>
                    <span data-tid="ModalText">
                        {modalType === "Rerun"
                            ? "Уверен, что таску надо перезапустить?"
                            : "Уверен, что таску надо остановить?"}
                    </span>
                </Modal.Body>
                <Modal.Footer>
                    <RowStack gap={2}>
                        <Fit>
                            {modalType === "Rerun" ? (
                                <Button
                                    data-tid="RerunButton"
                                    use="success"
                                    onClick={() => {
                                        onRerun(actionTask);
                                        this.closeModal();
                                    }}>
                                    Перезапустить
                                </Button>
                            ) : (
                                <Button
                                    data-tid="CancelButton"
                                    use="danger"
                                    onClick={() => {
                                        onCancel(actionTask);
                                        this.closeModal();
                                    }}>
                                    Остановить
                                </Button>
                            )}
                        </Fit>
                        <Fit>
                            <Button data-tid="CloseButton" onClick={() => this.closeModal()}>
                                Закрыть
                            </Button>
                        </Fit>
                    </RowStack>
                </Modal.Footer>
            </Modal>
        );
    }

    public rerun(id: string) {
        this.setState({
            openedModal: true,
            modalType: "Rerun",
            actionTask: id,
        });
    }

    public cancel(id: string) {
        this.setState({
            openedModal: true,
            modalType: "Cancel",
            actionTask: id,
        });
    }

    public closeModal() {
        this.setState({
            openedModal: false,
        });
    }
}
