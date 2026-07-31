import { Fit, RowStack } from "@skbkontur/react-stack-layout";
import { ThemeContext, Button, Modal } from "@skbkontur/react-ui";
import { useStyles } from "@skbkontur/react-ui/lib/renderEnvironment";
import { memo, useContext, useState } from "react";
import { Location } from "react-router-dom";

import { RtqMonitoringTaskMeta } from "../../Domain/Api/RtqMonitoringTaskMeta";

import { TaskDetails } from "./TaskDetails/TaskDetails";
import { getStyles } from "./TaskTable.styles";

export interface TaskTableProps {
    taskInfos: RtqMonitoringTaskMeta[];
    allowRerunOrCancel: boolean;
    chosenTasks: Set<string>;
    onRerun: (x0: string) => void;
    onCancel: (x0: string) => void;
    onCheck: (taskId: string) => void;
    getTaskLocation: (x0: string) => string | Partial<Location>;
}

export const TasksTable = memo(
    ({ taskInfos, allowRerunOrCancel, chosenTasks, onRerun, onCancel, onCheck, getTaskLocation }: TaskTableProps) => {
        const [openedModal, setOpenedModal] = useState(false);
        const [modalType, setModalType] = useState<"Cancel" | "Rerun">("Cancel");
        const [actionTask, setActionTask] = useState("");
        const theme = useContext(ThemeContext);
        const styles = useStyles(getStyles);

        const openRerunModal = (id: string) => {
            setOpenedModal(true);
            setModalType("Rerun");
            setActionTask(id);
        };

        const openCancelModal = (id: string) => {
            setOpenedModal(true);
            setModalType("Cancel");
            setActionTask(id);
        };

        const closeModal = () => {
            setOpenedModal(false);
        };

        return (
            <div>
                <div data-tid="Tasks">
                    {taskInfos.map(item => (
                        <div key={item.id} className={styles.taskDetailsRow()}>
                            <TaskDetails
                                getTaskLocation={getTaskLocation}
                                data-tid="Task"
                                onCancel={() => openCancelModal(item.id)}
                                onRerun={() => openRerunModal(item.id)}
                                taskInfo={item}
                                allowRerunOrCancel={allowRerunOrCancel}
                                isChecked={chosenTasks.has(item.id)}
                                onCheck={() => onCheck(item.id)}
                            />
                        </div>
                    ))}
                </div>
                {openedModal && (
                    <Modal onClose={closeModal} width={500} data-tid="ConfirmOperationModal">
                        <Modal.Header>
                            <span className={styles.modalText(theme)}>Нужно подтверждение</span>
                        </Modal.Header>
                        <Modal.Body>
                            <span data-tid="ModalText" className={styles.modalText(theme)}>
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
                                                closeModal();
                                            }}>
                                            Перезапустить
                                        </Button>
                                    ) : (
                                        <Button
                                            data-tid="CancelButton"
                                            use="danger"
                                            onClick={() => {
                                                onCancel(actionTask);
                                                closeModal();
                                            }}>
                                            Остановить
                                        </Button>
                                    )}
                                </Fit>
                                <Fit>
                                    <Button data-tid="CloseButton" use="outline" onClick={closeModal}>
                                        Закрыть
                                    </Button>
                                </Fit>
                            </RowStack>
                        </Modal.Footer>
                    </Modal>
                )}
            </div>
        );
    }
);
