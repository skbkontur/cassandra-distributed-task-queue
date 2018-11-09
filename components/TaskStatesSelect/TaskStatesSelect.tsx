import ArrowTriangleDownIcon from "@skbkontur/react-icons/ArrowTriangleDown";
import * as React from "react";
import { Button, Checkbox, Tooltip } from "ui";
import { ColumnStack, Fit } from "ui/layout";
import { TaskState, TaskStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskState";
import { getAllTaskStates } from "Domain/EDI/Api/RemoteTaskQueue/TaskStateExtensions";

import cn from "./TaskStatesSelect.less";

export interface TaskStatesSelectProps {
    value: TaskState[];
    onChange: (selectedTaskStates: TaskState[]) => void;
}

const TaskStateCaptions = {
    [TaskStates.Unknown]: "Unknown",
    [TaskStates.New]: "New",
    [TaskStates.WaitingForRerun]: "Waiting for rerun",
    [TaskStates.WaitingForRerunAfterError]: "Waiting for rerun after error",
    [TaskStates.Finished]: "Finished",
    [TaskStates.InProcess]: "In process",
    [TaskStates.Fatal]: "Fatal",
    [TaskStates.Canceled]: "Canceled",
};

export class TaskStatesSelect extends React.Component<TaskStatesSelectProps> {
    public render(): JSX.Element {
        const { value } = this.props;
        return (
            <Tooltip render={() => this.renderTooltip()} trigger="click" pos="bottom left">
                <Button>
                    <span data-tid="ButtonText" className={cn("button-text")}>
                        {value.length ? `Выбрано состояний: ${value.length}` : "Выбрать состояние"}
                    </span>
                    <ArrowTriangleDownIcon />
                </Button>
            </Tooltip>
        );
    }

    private renderTooltip(): JSX.Element {
        return (
            <ColumnStack block gap={2}>
                {getAllTaskStates().map((item, index) => (
                    <Fit key={index}>
                        <Checkbox
                            data-tid={item}
                            checked={this.isItemSelected(item)}
                            onChange={(e, val) => this.selectItem(val, item)}>
                            {TaskStateCaptions[item]}
                        </Checkbox>
                    </Fit>
                ))}
            </ColumnStack>
        );
    }

    private isItemSelected(item: TaskState): boolean {
        const { value } = this.props;
        return Boolean(value.find(i => i === item));
    }

    private selectItem(val: boolean, taskState: TaskState) {
        const { value, onChange } = this.props;
        const newSelectedArray = value.slice();

        if (val) {
            newSelectedArray.push(taskState);
        } else {
            const index = newSelectedArray.findIndex(i => i === taskState);
            newSelectedArray.splice(index, 1);
        }
        onChange(newSelectedArray);
    }
}
