// @flow
import * as React from "react";
import cn from "./TaskDetailsPage.less";
import { Button, Modal, ModalHeader, ModalBody, ModalFooter, ButtonLink, RouterLink } from "ui";
import { RowStack, ColumnStack, Fill, Fit } from "ui/layout";
import CommonLayout, {
    CommonLayoutGoBack,
    CommonLayoutGreyLineHeader,
    CommonLayoutContent,
} from "../../../Commons/Layouts";
import { cancelableStates, rerunableStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskStateExtensions";
import TaskDetailsMetaTable from "../TaskDetailsMetaTable/TaskDetailsMetaTable";
import Accordion from "../../../Commons/Accordion/Accordion";
import taskDetailsCustomRender from "../../Domain/TaskDetailsCustomRender";
import TaskTimeLine from "../TaskTimeLine/TaskTimeLine";
import type { RemoteTaskInfoModel } from "../../api/RemoteTaskQueueApi";
import type { RouterLocationDescriptor } from "../../../Commons/DataTypes/Routing";
import { buildSearchQueryForRequest } from "../../containers/TasksPageContainer";
import RangeSelector from "../../../Commons/DateTimeRangePicker/RangeSelector";
import { TimeZones, ticksToDate } from "../../../Commons/DataTypes/Time";

export type TaskDetailsPageProps = {
    parentLocation: RouterLocationDescriptor,
    taskDetails: ?RemoteTaskInfoModel,
    getTaskLocation: (id: string) => RouterLocationDescriptor,
    allowRerunOrCancel: boolean,
    onRerun: (id: string) => void,
    onCancel: (id: string) => void,
};

type TaskDetailsPageState = {
    openedModal: boolean,
    modalType: "Cancel" | "Rerun",
};

export default class TaskDetailsPage extends React.Component<TaskDetailsPageProps, TaskDetailsPageState> {
    componentWillMount() {
        this.setState({
            openedModal: false,
            modalType: "Cancel",
        });
    }

    render(): React.Node {
        const { allowRerunOrCancel, getTaskLocation, taskDetails, parentLocation } = this.props;
        const { openedModal } = this.state;

        return (
            <div>
                <CommonLayout>
                    {taskDetails && (
                        <CommonLayoutGoBack to={parentLocation}>Вернуться к поиску задач</CommonLayoutGoBack>
                    )}
                    {taskDetails && (
                        <CommonLayoutGreyLineHeader
                            data-tid="Header"
                            title={`Задача ${taskDetails.taskMeta.name}`}
                            tools={taskDetails && allowRerunOrCancel ? this.renderButtons() : null}>
                            <TaskTimeLine getHrefToTask={getTaskLocation} taskMeta={taskDetails.taskMeta} />
                        </CommonLayoutGreyLineHeader>
                    )}
                    <CommonLayoutContent>
                        <ColumnStack block streach gap={2}>
                            {taskDetails && (
                                <Fit>
                                    <TaskDetailsMetaTable taskMeta={taskDetails.taskMeta} />
                                </Fit>
                            )}
                            {taskDetails && (
                                <Fit>
                                    <Accordion
                                        customRender={taskDetailsCustomRender}
                                        value={taskDetails.taskData}
                                        title="TaskData"
                                    />
                                </Fit>
                            )}
                            {taskDetails &&
                                taskDetails.exceptionInfos && (
                                    <Fit className={cn("exception-container")} data-tid="Exceptions">
                                        {taskDetails.exceptionInfos.map((exception, index) => {
                                            return (
                                                <pre data-tid="Exception" key={index} className={cn("exception")}>
                                                    {exception.exceptionMessageInfo}
                                                </pre>
                                            );
                                        })}
                                    </Fit>
                                )}
                        </ColumnStack>
                    </CommonLayoutContent>
                </CommonLayout>
                {openedModal && this.renderModal()}
            </div>
        );
    }

    getRelatedTasksLocation(taskDetails: RemoteTaskInfoModel): ?RouterLocationDescriptor {
        const documentCirculationId =
            // @flow-disable-next-line хоршо бы разобраться и затипизировать четко
            taskDetails.taskData && typeof taskDetails.taskData.documentCirculationId === "string"
                ? taskDetails.taskData.documentCirculationId
                : null;
        if (documentCirculationId != null && taskDetails.taskMeta.ticks != null) {
            const rangeSelector = new RangeSelector(TimeZones.UTC);

            return {
                pathname: "/AdminTools/Tasks/Tree",
                search: buildSearchQueryForRequest({
                    enqueueDateTimeRange: rangeSelector.getMonthOf(ticksToDate(taskDetails.taskMeta.ticks)),
                    queryString: `Data.\\*.DocumentCirculationId:"${documentCirculationId || ""}"`,
                    names: [],
                    states: [],
                }),
            };
        }
        return null;
    }

    renderButtons(): React.Node | null {
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
                        <RouterLink icon="list" data-tid={"RelatedTaskTree"} to={relatedTasksLocation}>
                            View related tasks tree
                        </RouterLink>
                    </Fit>
                )}
                {isCancelable && (
                    <Fit>
                        <ButtonLink icon="remove" use="danger" data-tid={"CancelButton"} onClick={() => this.cancel()}>
                            Cancel task
                        </ButtonLink>
                    </Fit>
                )}
                {isRerunable && (
                    <Fit>
                        <ButtonLink icon="refresh" data-tid={"RerunButton"} onClick={() => this.rerun()}>
                            Rerun task
                        </ButtonLink>
                    </Fit>
                )}
            </RowStack>
        );
    }

    renderModal(): React.Node | null {
        const { onCancel, onRerun, taskDetails } = this.props;
        const { modalType } = this.state;
        if (!taskDetails) {
            return null;
        }
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
                </ModalFooter>
            </Modal>
        );
    }

    rerun() {
        this.setState({
            openedModal: true,
            modalType: "Rerun",
        });
    }

    cancel() {
        this.setState({
            openedModal: true,
            modalType: "Cancel",
        });
    }

    closeModal() {
        this.setState({
            openedModal: false,
        });
    }
}
