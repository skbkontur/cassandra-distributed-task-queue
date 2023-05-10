import DeleteIcon from "@skbkontur/react-icons/Delete";
import ListRowsIcon from "@skbkontur/react-icons/ListRows";
import RefreshIcon from "@skbkontur/react-icons/Refresh";
import { ColumnStack, Fill, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Link, Modal, ThemeContext } from "@skbkontur/react-ui";
import React from "react";
import { Location } from "react-router-dom";

import { RtqMonitoringTaskModel } from "../../Domain/Api/RtqMonitoringTaskModel";
import { ICustomRenderer } from "../../Domain/CustomRenderer";
import { cancelableStates, rerunableStates } from "../../Domain/TaskStateExtensions";
import { searchRequestMapping } from "../../containers/TasksPageContainer";
import { Accordion } from "../Accordion/Accordion";
import { GoBackLink } from "../GoBack/GoBackLink";
import { CommonLayout } from "../Layouts/CommonLayout";
import { RouterLink } from "../RouterLink/RouterLink";
import { TaskDetailsMetaTable } from "../TaskDetailsMetaTable/TaskDetailsMetaTable";
import { TaskTimeLine } from "../TaskTimeLine/TaskTimeLine";

import { jsStyles } from "./TaskDetailsPage.styles";

export interface TaskDetailsPageProps {
    parentLocation: string;
    taskDetails: Nullable<RtqMonitoringTaskModel>;
    customRenderer: ICustomRenderer;
    getTaskLocation: (id: string) => string | Partial<Location>;
    allowRerunOrCancel: boolean;
    onRerun: (id: string) => void;
    onCancel: (id: string) => void;
}

export function TaskDetailsPage({
    parentLocation,
    taskDetails,
    customRenderer,
    getTaskLocation,
    allowRerunOrCancel,
    onRerun,
    onCancel,
}: TaskDetailsPageProps): JSX.Element {
    const [openedModal, setOpenedModal] = React.useState(false);
    const [modalType, setModalType] = React.useState<"Cancel" | "Rerun">("Cancel");
    const theme = React.useContext(ThemeContext);

    const rerun = () => {
        setOpenedModal(true);
        setModalType("Rerun");
    };

    const cancel = () => {
        setOpenedModal(true);
        setModalType("Cancel");
    };

    const closeModal = () => setOpenedModal(false);

    const renderButtons = (): JSX.Element | null => {
        if (!taskDetails) {
            return null;
        }
        const isCancelable = cancelableStates.includes(taskDetails.taskMeta.state);
        const isRerunable = rerunableStates.includes(taskDetails.taskMeta.state);
        const relatedTasksRequest = customRenderer.getRelatedTasksLocation(taskDetails);
        if (!isCancelable && !isRerunable && relatedTasksRequest == null) {
            return null;
        }

        return (
            <RowStack baseline block gap={2}>
                <Fill />
                {relatedTasksRequest && (
                    <Fit>
                        <RouterLink
                            data-tid={"RelatedTaskTree"}
                            to={`../Tree${searchRequestMapping.stringify(relatedTasksRequest)}`}>
                            <ListRowsIcon />
                            {"\u00A0"}
                            View related tasks tree
                        </RouterLink>
                    </Fit>
                )}
                {isCancelable && (
                    <Fit>
                        <Link icon={<DeleteIcon />} use="danger" data-tid={"CancelButton"} onClick={cancel}>
                            Cancel task
                        </Link>
                    </Fit>
                )}
                {isRerunable && (
                    <Fit>
                        <Link icon={<RefreshIcon />} data-tid={"RerunButton"} onClick={rerun}>
                            Rerun task
                        </Link>
                    </Fit>
                )}
            </RowStack>
        );
    };

    const renderModal = (): JSX.Element | null => {
        if (!taskDetails) {
            return null;
        }
        return (
            <Modal onClose={closeModal} width={500} data-tid="ConfirmOperationModal">
                <Modal.Header>
                    <span className={jsStyles.modalText(theme)}>Нужно подтверждение</span>
                </Modal.Header>
                <Modal.Body>
                    <span data-tid="ModalText" className={jsStyles.modalText(theme)}>
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
                                        onRerun(taskDetails.taskMeta.id);
                                        closeModal();
                                    }}>
                                    Перезапустить
                                </Button>
                            ) : (
                                <Button
                                    data-tid="CancelButton"
                                    use="danger"
                                    onClick={() => {
                                        onCancel(taskDetails.taskMeta.id);
                                        closeModal();
                                    }}>
                                    Остановить
                                </Button>
                            )}
                        </Fit>
                        <Fit>
                            <Button data-tid="CloseButton" onClick={closeModal}>
                                Закрыть
                            </Button>
                        </Fit>
                    </RowStack>
                </Modal.Footer>
            </Modal>
        );
    };

    return (
        <div>
            <CommonLayout>
                {taskDetails && (
                    <CommonLayout.GreyLineHeader
                        data-tid="Header"
                        title={
                            <RowStack gap={3} verticalAlign="bottom">
                                <GoBackLink backUrl={parentLocation} />
                                <span>Задача {taskDetails.taskMeta.name}</span>
                            </RowStack>
                        }
                        tools={taskDetails && allowRerunOrCancel ? renderButtons() : null}>
                        <TaskTimeLine
                            getHrefToTask={getTaskLocation}
                            taskMeta={taskDetails.taskMeta}
                            childTaskIds={taskDetails.childTaskIds}
                        />
                    </CommonLayout.GreyLineHeader>
                )}
                <CommonLayout.Content>
                    <ColumnStack block stretch gap={2}>
                        {taskDetails && (
                            <Fit>
                                <TaskDetailsMetaTable
                                    taskMeta={taskDetails.taskMeta}
                                    childTaskIds={taskDetails.childTaskIds}
                                />
                            </Fit>
                        )}
                        {taskDetails && (
                            <Fit className={jsStyles.taskDataContainer()}>
                                <Accordion
                                    renderCaption={null}
                                    renderValue={customRenderer.renderDetails}
                                    value={taskDetails.taskData}
                                    title="TaskData"
                                />
                            </Fit>
                        )}
                        {taskDetails && taskDetails.exceptionInfos && (
                            <Fit className={jsStyles.exceptionContainer()} data-tid="Exceptions">
                                {taskDetails.exceptionInfos.map((exception, index) => (
                                    <pre data-tid="Exception" key={index} className={jsStyles.exception(theme)}>
                                        {exception}
                                    </pre>
                                ))}
                            </Fit>
                        )}
                    </ColumnStack>
                </CommonLayout.Content>
            </CommonLayout>
            {openedModal && renderModal()}
        </div>
    );
}
