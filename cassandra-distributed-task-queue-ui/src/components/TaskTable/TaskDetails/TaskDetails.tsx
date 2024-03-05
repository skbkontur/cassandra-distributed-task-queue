import { ArrowRoundTimeForwardIcon16Regular } from "@skbkontur/icons/ArrowRoundTimeForwardIcon16Regular";
import { XIcon16Regular } from "@skbkontur/icons/XIcon16Regular";
import { ColumnStack, Fill, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Checkbox, Link, ThemeContext } from "@skbkontur/react-ui";
import React from "react";
import { Location } from "react-router-dom";

import { useCustomSettings } from "../../../CustomSettingsContext";
import { RtqMonitoringTaskMeta } from "../../../Domain/Api/RtqMonitoringTaskMeta";
import { Ticks } from "../../../Domain/DataTypes/Time";
import { cancelableStates, rerunableStates } from "../../../Domain/TaskStateExtensions";
import { AllowCopyToClipboard } from "../../AllowCopyToClipboard";
import { DateTimeView } from "../../DateTimeView/DateTimeView";
import { RouterLink } from "../../RouterLink/RouterLink";

import { jsStyles } from "./TaskDetails.styles";

interface TaskDetailsProps {
    taskInfo: RtqMonitoringTaskMeta;
    allowRerunOrCancel: boolean;
    isChecked: boolean;
    onRerun: () => void;
    onCancel: () => void;
    onCheck: () => void;
    getTaskLocation: (id: string) => string | Partial<Location>;
}

function dateFormatter(
    item: RtqMonitoringTaskMeta,
    selector: (obj: RtqMonitoringTaskMeta) => Nullable<Ticks>
): JSX.Element {
    return <DateTimeView value={selector(item)} />;
}

export function TaskDetails(props: TaskDetailsProps): JSX.Element {
    const { allowRerunOrCancel, taskInfo, isChecked, onCancel, onRerun, onCheck, getTaskLocation } = props;
    const theme = React.useContext(ThemeContext);
    const { customStateCaptions } = useCustomSettings();

    const canCancel = taskInfo.taskActions ? taskInfo.taskActions.canCancel : cancelableStates.includes(taskInfo.state);
    const canRerun = taskInfo.taskActions ? taskInfo.taskActions.canRerun : rerunableStates.includes(taskInfo.state);

    const renderTaskDate = (
        taskInfo: RtqMonitoringTaskMeta,
        caption: string,
        selector: (obj: RtqMonitoringTaskMeta) => Nullable<Ticks>
    ): JSX.Element => {
        return (
            <div>
                <span className={jsStyles.dateCaption(theme)}>{caption}</span>
                <span>{dateFormatter(taskInfo, selector)}</span>
            </div>
        );
    };

    return (
        <RowStack block className={`${jsStyles.taskDetails()} ${jsStyles.state(theme, taskInfo.state)}`} gap={2}>
            <Fit>
                <Checkbox className={jsStyles.checkbox()} onValueChange={onCheck} checked={isChecked} />
            </Fit>
            <ColumnStack block gap={1}>
                <Fit className={jsStyles.name()}>
                    <RouterLink data-tid="Name" to={getTaskLocation(taskInfo.id)}>
                        {taskInfo.name}
                    </RouterLink>
                </Fit>
                <Fit>
                    <RowStack verticalAlign="stretch" block gap={2}>
                        <Fit tag={ColumnStack} className={jsStyles.infoBlock1()}>
                            <Fit className={jsStyles.id()}>
                                <AllowCopyToClipboard>
                                    <span data-tid="TaskId">{taskInfo.id}</span>
                                </AllowCopyToClipboard>
                            </Fit>
                            <Fit>
                                <span className={jsStyles.stateName()} data-tid="State">
                                    {customStateCaptions[taskInfo.state]}
                                </span>
                                <span className={jsStyles.attempts()}>
                                    Attempts: <span data-tid="Attempts">{taskInfo.attempts}</span>
                                </span>
                            </Fit>
                            <Fill className={jsStyles.parentTask()}>
                                <div>
                                    Parent:{" "}
                                    {taskInfo.parentTaskId ? (
                                        <AllowCopyToClipboard>{taskInfo.parentTaskId}</AllowCopyToClipboard>
                                    ) : (
                                        "-"
                                    )}
                                </div>
                            </Fill>
                            {allowRerunOrCancel && (
                                <Fit>
                                    <RowStack baseline block gap={2}>
                                        <Fit>
                                            <Link
                                                data-tid="Cancel"
                                                disabled={!canCancel}
                                                onClick={onCancel}
                                                icon={<XIcon16Regular />}>
                                                Cancel
                                            </Link>
                                        </Fit>
                                        <Fit>
                                            <Link
                                                data-tid="Rerun"
                                                disabled={!canRerun}
                                                onClick={onRerun}
                                                icon={<ArrowRoundTimeForwardIcon16Regular />}>
                                                Rerun
                                            </Link>
                                        </Fit>
                                    </RowStack>
                                </Fit>
                            )}
                        </Fit>
                        <Fit className={jsStyles.dates()}>
                            {renderTaskDate(taskInfo, "Enqueued", x => x.ticks)}
                            {renderTaskDate(taskInfo, "Started", x => x.startExecutingTicks)}
                            {renderTaskDate(taskInfo, "Finished", x => x.finishExecutingTicks)}
                            {renderTaskDate(taskInfo, "StateTime", x => x.minimalStartTicks)}
                            {renderTaskDate(taskInfo, "Expiration", x => x.expirationTimestampTicks)}
                        </Fit>
                    </RowStack>
                </Fit>
            </ColumnStack>
        </RowStack>
    );
}
