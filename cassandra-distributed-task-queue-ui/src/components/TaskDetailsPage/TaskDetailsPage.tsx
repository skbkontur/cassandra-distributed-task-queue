import DeleteIcon from "@skbkontur/react-icons/Delete";
import ListRowsIcon from "@skbkontur/react-icons/ListRows";
import RefreshIcon from "@skbkontur/react-icons/Refresh";
import { ColumnStack, Fill, Fit, RowStack } from "@skbkontur/react-stack-layout";
import Button from "@skbkontur/react-ui/Button";
import Link from "@skbkontur/react-ui/Link";
import Modal from "@skbkontur/react-ui/Modal";
import { LocationDescriptor } from "history";
import React from "react";
import { Link as RouterLink } from "react-router-dom";

import { RtqMonitoringTaskModel } from "../../Domain/Api/RtqMonitoringTaskModel";
import { cancelableStates, rerunableStates } from "../../Domain/TaskStateExtensions";
import { TimeUtils } from "../../Domain/Utils/TimeUtils";
import { buildSearchQueryForRequest } from "../../containers/TasksPageContainer";
import { Accordion } from "../Accordion/Accordion";
import { RangeSelector } from "../DateTimeRangePicker/RangeSelector";
import { CommonLayout } from "../Layouts/CommonLayout";
import { TaskDetailsMetaTable } from "../TaskDetailsMetaTable/TaskDetailsMetaTable";
import { TaskTimeLine } from "../TaskTimeLine/TaskTimeLine";

import styles from "./TaskDetailsPage.less";

export interface TaskDetailsPageProps {
    parentLocation: LocationDescriptor;
    taskDetails: Nullable<RtqMonitoringTaskModel>;
    getTaskLocation: (id: string) => LocationDescriptor;
    allowRerunOrCancel: boolean;
    onRerun: (id: string) => void;
    onCancel: (id: string) => void;
}

interface TaskDetailsPageState {
    openedModal: boolean;
    modalType: "Cancel" | "Rerun";
}

export class TaskDetailsPage extends React.Component<TaskDetailsPageProps, TaskDetailsPageState> {
    public componentWillMount() {
        this.setState({
            openedModal: false,
            modalType: "Cancel",
        });
    }

    public render(): JSX.Element {
        const { allowRerunOrCancel, getTaskLocation, taskDetails, parentLocation } = this.props;
        const { openedModal } = this.state;

        return (
            <div>
                <CommonLayout>
                    {taskDetails && (
                        <CommonLayout.GoBack to={parentLocation}>Вернуться к поиску задач</CommonLayout.GoBack>
                    )}
                    {taskDetails && (
                        <CommonLayout.GreyLineHeader
                            data-tid="Header"
                            title={`Задача ${taskDetails.taskMeta.name}`}
                            tools={taskDetails && allowRerunOrCancel ? this.renderButtons() : null}>
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
                                <Fit className={styles.taskDataContainer}>
                                    <Accordion
                                        customRender={() => null}
                                        value={taskDetails.taskData}
                                        title="TaskData"
                                    />
                                </Fit>
                            )}
                            {taskDetails && taskDetails.exceptionInfos && (
                                <Fit className={styles.exceptionContainer} data-tid="Exceptions">
                                    {taskDetails.exceptionInfos.map((exception, index) => (
                                        <pre data-tid="Exception" key={index} className={styles.exception}>
                                            {exception.exceptionMessageInfo}
                                        </pre>
                                    ))}
                                </Fit>
                            )}
                        </ColumnStack>
                    </CommonLayout.Content>
                </CommonLayout>
                {openedModal && this.renderModal()}
            </div>
        );
    }

    public getRelatedTasksLocation(taskDetails: RtqMonitoringTaskModel): Nullable<LocationDescriptor> {
        const documentCirculationId =
            taskDetails.taskData && typeof taskDetails.taskData["documentCirculationId"] === "string"
                ? taskDetails.taskData["documentCirculationId"]
                : null;
        if (documentCirculationId != null && taskDetails.taskMeta.ticks != null) {
            const rangeSelector = new RangeSelector(TimeUtils.TimeZones.UTC);

            return {
                pathname: "/AdminTools/Tasks/Tree",
                search: buildSearchQueryForRequest({
                    enqueueTimestampRange: rangeSelector.getMonthOf(TimeUtils.ticksToDate(taskDetails.taskMeta.ticks)),
                    queryString: `Data.\\*.DocumentCirculationId:"${documentCirculationId || ""}"`,
                    names: [],
                    states: [],
                }),
            };
        }
        return null;
    }

    public renderButtons(): JSX.Element | null {
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
                <Fill />
                {relatedTasksLocation && (
                    <Fit>
                        <RouterLink
                            className={styles.routerLink}
                            data-tid={"RelatedTaskTree"}
                            to={relatedTasksLocation}>
                            <ListRowsIcon />
                            {"\u00A0"}
                            View related tasks tree
                        </RouterLink>
                    </Fit>
                )}
                {isCancelable && (
                    <Fit>
                        <Link
                            icon={<DeleteIcon />}
                            use="danger"
                            data-tid={"CancelButton"}
                            onClick={() => this.cancel()}>
                            Cancel task
                        </Link>
                    </Fit>
                )}
                {isRerunable && (
                    <Fit>
                        <Link icon={<RefreshIcon />} data-tid={"RerunButton"} onClick={() => this.rerun()}>
                            Rerun task
                        </Link>
                    </Fit>
                )}
            </RowStack>
        );
    }

    public renderModal(): JSX.Element | null {
        const { onCancel, onRerun, taskDetails } = this.props;
        const { modalType } = this.state;
        if (!taskDetails) {
            return null;
        }
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
                                        onRerun(taskDetails.taskMeta.id);
                                        this.closeModal();
                                    }}>
                                    Перезапустить
                                </Button>
                            ) : (
                                <Button
                                    data-tid="CancelButton"
                                    use="danger"
                                    onClick={() => {
                                        onCancel(taskDetails.taskMeta.id);
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

    public rerun() {
        this.setState({
            openedModal: true,
            modalType: "Rerun",
        });
    }

    public cancel() {
        this.setState({
            openedModal: true,
            modalType: "Cancel",
        });
    }

    public closeModal() {
        this.setState({
            openedModal: false,
        });
    }
}
