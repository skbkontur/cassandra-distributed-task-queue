import { Fit, RowStack } from "@skbkontur/react-stack-layout";
import { ThemeContext, Button, Modal } from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import isEqual from "lodash/isEqual";
import { Component, ReactElement } from "react";
import { Location } from "react-router-dom";

import { RtqMonitoringTaskMeta } from "../../Domain/Api/RtqMonitoringTaskMeta";

import { TaskDetails } from "./TaskDetails/TaskDetails";
import { jsStyles } from "./TaskTable.styles";

export interface TaskTableProps {
    taskInfos: RtqMonitoringTaskMeta[];
    allowRerunOrCancel: boolean;
    chosenTasks: Set<string>;
    onRerun: (x0: string) => void;
    onCancel: (x0: string) => void;
    onCheck: (taskId: string) => void;
    getTaskLocation: (x0: string) => string | Partial<Location>;
}

interface TasksTableState {
    openedModal: boolean;
    modalType: "Cancel" | "Rerun";
    actionTask: string;
}

export class TasksTable extends Component<TaskTableProps, TasksTableState> {
    public state: TasksTableState = {
        openedModal: false,
        modalType: "Cancel",
        actionTask: "",
    };

    private theme!: Theme;

    public shouldComponentUpdate(nextProps: TaskTableProps, nextState: TasksTableState): boolean {
        return (
            this.props.chosenTasks.size !== nextProps.chosenTasks.size ||
            !isEqual(this.props.taskInfos, nextProps.taskInfos) ||
            !isEqual(this.props.allowRerunOrCancel, nextProps.allowRerunOrCancel) ||
            !isEqual(this.state.openedModal, nextState.openedModal) ||
            !isEqual(this.state.modalType, nextState.modalType) ||
            !isEqual(this.state.actionTask, nextState.actionTask)
        );
    }

    public render(): ReactElement {
        const { taskInfos } = this.props;
        const { openedModal } = this.state;
        return (
            <ThemeContext.Consumer>
                {theme => {
                    this.theme = theme;
                    return (
                        <div>
                            <div data-tid="Tasks">{taskInfos.map(item => this.renderRow(item))}</div>
                            {openedModal && this.renderModal()}
                        </div>
                    );
                }}
            </ThemeContext.Consumer>
        );
    }

    public renderRow(item: RtqMonitoringTaskMeta): ReactElement {
        const { allowRerunOrCancel, chosenTasks, onCheck, getTaskLocation } = this.props;
        return (
            <div key={item.id} className={jsStyles.taskDetailsRow()}>
                <TaskDetails
                    getTaskLocation={getTaskLocation}
                    data-tid="Task"
                    onCancel={() => this.cancel(item.id)}
                    onRerun={() => this.rerun(item.id)}
                    taskInfo={item}
                    allowRerunOrCancel={allowRerunOrCancel}
                    isChecked={chosenTasks.has(item.id)}
                    onCheck={() => onCheck(item.id)}
                />
            </div>
        );
    }

    public renderModal(): ReactElement {
        const { onCancel, onRerun } = this.props;
        const { modalType, actionTask } = this.state;

        return (
            <Modal onClose={() => this.closeModal()} width={500} data-tid="ConfirmOperationModal">
                <Modal.Header>
                    <span className={jsStyles.modalText(this.theme)}>Нужно подтверждение</span>
                </Modal.Header>
                <Modal.Body>
                    <span data-tid="ModalText" className={jsStyles.modalText(this.theme)}>
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

    public rerun(id: string): void {
        this.setState({
            openedModal: true,
            modalType: "Rerun",
            actionTask: id,
        });
    }

    public cancel(id: string): void {
        this.setState({
            openedModal: true,
            modalType: "Cancel",
            actionTask: id,
        });
    }

    public closeModal(): void {
        this.setState({
            openedModal: false,
        });
    }
}
