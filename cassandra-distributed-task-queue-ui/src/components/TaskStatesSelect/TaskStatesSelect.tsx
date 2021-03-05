import ArrowTriangleDownIcon from "@skbkontur/react-icons/ArrowTriangleDown";
import { ColumnStack, Fit } from "@skbkontur/react-stack-layout";
import { Button, Checkbox, Tooltip } from "@skbkontur/react-ui";
import React from "react";

import { TaskState } from "../../Domain/Api/TaskState";
import { getAllTaskStates } from "../../Domain/TaskStateExtensions";

import styles from "./TaskStatesSelect.less";

export interface TaskStatesSelectProps {
    value: TaskState[];
    onChange: (selectedTaskStates: TaskState[]) => void;
}

const TaskStateCaptions = {
    [TaskState.Unknown]: "Unknown",
    [TaskState.New]: "New",
    [TaskState.WaitingForRerun]: "Waiting for rerun",
    [TaskState.WaitingForRerunAfterError]: "Waiting for rerun after error",
    [TaskState.Finished]: "Finished",
    [TaskState.InProcess]: "In process",
    [TaskState.Fatal]: "Fatal",
    [TaskState.Canceled]: "Canceled",
};

export class TaskStatesSelect extends React.Component<TaskStatesSelectProps> {
    public render(): JSX.Element {
        const { value } = this.props;
        return (
            <span>
                <Tooltip render={this.renderTooltip} trigger="click" pos="bottom left">
                    <Button>
                        <span data-tid="ButtonText" className={styles.buttonText}>
                            {value.length ? `Выбрано состояний: ${value.length}` : "Выбрать состояние"}
                        </span>
                        <ArrowTriangleDownIcon />
                    </Button>
                </Tooltip>
            </span>
        );
    }

    private readonly renderTooltip = (): JSX.Element => (
        <ColumnStack block gap={2}>
            {getAllTaskStates().map((item, index) => (
                <Fit key={index}>
                    <Checkbox
                        data-tid={item}
                        checked={this.isItemSelected(item)}
                        onValueChange={val => this.selectItem(val, item)}>
                        {TaskStateCaptions[item]}
                    </Checkbox>
                </Fit>
            ))}
        </ColumnStack>
    );

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
