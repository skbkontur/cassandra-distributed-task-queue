import { Accordion } from "@skbkontur/edi-ui";
import { ArrowRoundTimeForwardIcon16Regular } from "@skbkontur/icons/ArrowRoundTimeForwardIcon16Regular";
import { TextAlignCenterJustifyIcon16Regular } from "@skbkontur/icons/TextAlignCenterJustifyIcon16Regular";
import { XIcon16Regular } from "@skbkontur/icons/XIcon16Regular";
import { ColumnStack, Fill, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Link, Modal, ThemeContext } from "@skbkontur/react-ui";
import { useContext, useState, ReactElement } from "react";
import { Location } from "react-router-dom";

import { useCustomSettings } from "../../CustomSettingsContext";
import { RtqMonitoringTaskModel } from "../../Domain/Api/RtqMonitoringTaskModel";
import { cancelableStates, rerunableStates } from "../../Domain/TaskStateExtensions";
import { searchRequestMapping } from "../../containers/TasksPageContainer";
import { CommonLayout } from "../Layouts/CommonLayout";
import { RouterLink } from "../RouterLink/RouterLink";
import { TaskDetailsMetaTable } from "../TaskDetailsMetaTable/TaskDetailsMetaTable";
import { TaskTimeLine } from "../TaskTimeLine/TaskTimeLine";

import { jsStyles } from "./TaskDetailsPage.styles";

export interface TaskDetailsPageProps {
    parentLocation: string;
    taskDetails: Nullable<RtqMonitoringTaskModel>;
    getTaskLocation: (id: string) => string | Partial<Location>;
    allowRerunOrCancel: boolean;
    onRerun: (id: string) => void;
    onCancel: (id: string) => void;
}

export function TaskDetailsPage({
    parentLocation,
    taskDetails,
    getTaskLocation,
    allowRerunOrCancel,
    onRerun,
    onCancel,
}: TaskDetailsPageProps): ReactElement {
    const [openedModal, setOpenedModal] = useState(false);
    const [modalType, setModalType] = useState<"Cancel" | "Rerun">("Cancel");
    const theme = useContext(ThemeContext);
    const { customDetailRenderer } = useCustomSettings();

    const rerun = () => {
        setOpenedModal(true);
        setModalType("Rerun");
    };

    const cancel = () => {
        setOpenedModal(true);
        setModalType("Cancel");
    };

    const closeModal = () => setOpenedModal(false);

    const renderButtons = (): ReactElement | null => {
        if (!taskDetails) {
            return null;
        }

        const canCancel = taskDetails.taskMeta.taskActions
            ? taskDetails.taskMeta.taskActions.canCancel
            : allowRerunOrCancel && cancelableStates.includes(taskDetails.taskMeta.state);

        const canRerun = taskDetails.taskMeta.taskActions
            ? taskDetails.taskMeta.taskActions.canRerun
            : allowRerunOrCancel && rerunableStates.includes(taskDetails.taskMeta.state);

        const relatedTasksRequest = customDetailRenderer.getRelatedTasksLocation(taskDetails);
        if (!canCancel && !canRerun && relatedTasksRequest == null) {
            return null;
        }

        return (
            <RowStack baseline block gap={2}>
                <Fill />
                {relatedTasksRequest && (
                    <Fit>
                        <RouterLink
                            data-tid="RelatedTaskTree"
                            to={`../Tree${searchRequestMapping.stringify(relatedTasksRequest)}`}>
                            <TextAlignCenterJustifyIcon16Regular />
                            {"\u00A0"}
                            View related tasks tree
                        </RouterLink>
                    </Fit>
                )}
                {canCancel && (
                    <Fit>
                        <Link icon={<XIcon16Regular />} use="danger" data-tid="CancelButton" onClick={cancel}>
                            Cancel task
                        </Link>
                    </Fit>
                )}
                {canRerun && (
                    <Fit>
                        <Button
                            use="link"
                            icon={<ArrowRoundTimeForwardIcon16Regular />}
                            data-tid="RerunButton"
                            onClick={rerun}>
                            Rerun task
                        </Button>
                    </Fit>
                )}
            </RowStack>
        );
    };

    const renderModal = (): ReactElement | null => {
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
            <CommonLayout withArrow>
                <CommonLayout.GoBack to={parentLocation} />
                {taskDetails && (
                    <CommonLayout.Header
                        borderBottom
                        data-tid="Header"
                        title={`Задача ${taskDetails.taskMeta.name}`}
                        tools={renderButtons()}>
                        <TaskTimeLine
                            getHrefToTask={getTaskLocation}
                            taskMeta={taskDetails.taskMeta}
                            childTaskIds={taskDetails.childTaskIds}
                        />
                    </CommonLayout.Header>
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
                                    renderValue={customDetailRenderer.renderDetails}
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
