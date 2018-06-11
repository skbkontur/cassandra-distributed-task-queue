import * as React from "react";
import { Button, Modal, ModalHeader, ModalBody, ModalFooter } from "ui";
import _ from "lodash";
import { RowStack, Fit } from "ui/layout";

import { TaskMetaInformation } from "../../api/RemoteTaskQueueApi";
import { LocationDescriptor } from "history";

import TaskDetails from "./TaskDetails/TaskDetails";
import cn from "./TaskTable.less";

export type TaskTableProps = {
    taskInfos: TaskMetaInformation[];
    allowRerunOrCancel: boolean;
    onRerun: (x0: string) => void;
    onCancel: (x0: string) => void;
    getTaskLocation: (x0: string) => LocationDescriptor;
};

type TasksTableState = {
    openedModal: boolean;
    modalType: "Cancel" | "Rerun";
    actionTask: string;
};

export default class TasksTable extends React.Component<TaskTableProps, TasksTableState> {
    state: TasksTableState = {
        openedModal: false,
        modalType: "Cancel",
        actionTask: "",
    };

    shouldComponentUpdate(nextProps: TaskTableProps, nextState: TasksTableState): boolean {
        return (
            !_.isEqual(this.props.taskInfos, nextProps.taskInfos) ||
            !_.isEqual(this.props.allowRerunOrCancel, nextProps.allowRerunOrCancel) ||
            !_.isEqual(this.state.openedModal, nextState.openedModal) ||
            !_.isEqual(this.state.modalType, nextState.modalType) ||
            !_.isEqual(this.state.actionTask, nextState.actionTask)
        );
    }

    render(): JSX.Element {
        const { taskInfos } = this.props;
        const { openedModal } = this.state;
        return (
            <div>
                <div data-tid="Tasks">{taskInfos.map(item => this.renderRow(item))}</div>
                {openedModal && this.renderModal()}
            </div>
        );
    }

    renderRow(item: TaskMetaInformation): JSX.Element {
        const { allowRerunOrCancel, getTaskLocation } = this.props;
        return (
            <div key={item.id} className={cn("task-details-row")}>
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

    renderModal(): JSX.Element {
        const { onCancel, onRerun } = this.props;
        const { modalType, actionTask } = this.state;

        return (
            <Modal onClose={() => this.closeModal()} width={500} data-tid="ConfirmOperationModal">
                <ModalHeader>Нужно подтверждение</ModalHeader>
                <ModalBody>
                    <span data-tid="ModalText">
                        {modalType === "Rerun"
                            ? "Уверен, что таску надо перезапустить?"
                            : "Уверен, что таску надо остановить?"}
                    </span>
                </ModalBody>
                <ModalFooter>
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
                </ModalFooter>
            </Modal>
        );
    }

    rerun(id: string) {
        this.setState({
            openedModal: true,
            modalType: "Rerun",
            actionTask: id,
        });
    }

    cancel(id: string) {
        this.setState({
            openedModal: true,
            modalType: "Cancel",
            actionTask: id,
        });
    }

    closeModal() {
        this.setState({
            openedModal: false,
        });
    }
}
