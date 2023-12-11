import { ColumnStack, Fill, Fit, Fixed, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Input, Link } from "@skbkontur/react-ui";
import React from "react";
import type { JSX } from "react";

import { RtqMonitoringSearchRequest } from "../../Domain/Api/RtqMonitoringSearchRequest";
import { DateTimeRangePicker } from "../DateTimeRangePicker/DateTimeRangePicker";
import { TaskStatesSelect } from "../TaskStatesSelect/TaskStatesSelect";
import { TaskTypesSelect } from "../TaskTypesSelect/TaskTypesSelect";

import { jsStyles } from "./TaskQueueFilter.styles";
import { TaskQueueFilterHelpModal } from "./TaskQueueFilterHelpModal";

export interface TaskQueueFilterProps {
    value: RtqMonitoringSearchRequest;
    availableTaskTypes: string[] | null;
    onChange: (filterParams: Partial<RtqMonitoringSearchRequest>) => void;
    onSearchButtonClick: () => void;
}

export function TaskQueueFilter({
    value,
    availableTaskTypes,
    onChange,
    onSearchButtonClick,
}: TaskQueueFilterProps): JSX.Element {
    const [openedModal, setOpenedModal] = React.useState(false);

    const openModal = () => {
        setOpenedModal(true);
    };

    const closeModal = () => {
        setOpenedModal(false);
    };

    const { enqueueTimestampRange, queryString, states, names, count } = value;
    const defaultEnqueueDateTimeRange = {
        lowerBound: null,
        upperBound: null,
    };
    return (
        <RowStack block gap={1}>
            <Fill>
                <ColumnStack stretch block gap={1}>
                    <Fit>
                        <Input
                            width="100%"
                            data-tid={"SearchStringInput"}
                            value={queryString || ""}
                            onValueChange={value => onChange({ queryString: value })}
                            onKeyDown={e => {
                                if (e.key === "Enter") {
                                    onSearchButtonClick();
                                }
                            }}
                        />
                    </Fit>
                    <Fit className={jsStyles.searchLink()}>
                        <Link onClick={openModal} data-tid="OpenModalButton">
                            Что можно ввести в строку поиска
                        </Link>
                        {openedModal && <TaskQueueFilterHelpModal onClose={closeModal} />}
                    </Fit>
                </ColumnStack>
            </Fill>
            <Fixed width={72}>
                <Input
                    width="100%"
                    data-tid="MaxInput"
                    placeholder="Max"
                    value={String(count)}
                    onValueChange={value => onChange({ count: Number(value) })}
                />
            </Fixed>
            <Fit>
                <DateTimeRangePicker
                    data-tid="DateTimeRangePicker"
                    hideTime
                    value={enqueueTimestampRange || defaultEnqueueDateTimeRange}
                    onChange={value => onChange({ enqueueTimestampRange: value })}
                />
            </Fit>
            <Fit>
                <TaskTypesSelect
                    data-tid="TaskTypesSelect"
                    value={names || []}
                    disabled={availableTaskTypes === null}
                    availableTaskTypes={availableTaskTypes || []}
                    onChange={selectedTypes => onChange({ names: selectedTypes })}
                />
            </Fit>
            <Fit>
                <TaskStatesSelect
                    data-tid={"TaskStatesSelect"}
                    value={states || []}
                    onChange={selectedStates => onChange({ states: selectedStates })}
                />
            </Fit>
            <Fit>
                <Button data-tid={"SearchButton"} onClick={onSearchButtonClick} use="primary">
                    Найти
                </Button>
            </Fit>
        </RowStack>
    );
}
