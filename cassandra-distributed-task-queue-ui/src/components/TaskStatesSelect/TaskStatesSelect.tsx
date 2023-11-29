import { ArrowShapeTriangleADownIcon16Regular } from "@skbkontur/icons/ArrowShapeTriangleADownIcon16Regular";
import { ColumnStack, Fit } from "@skbkontur/react-stack-layout";
import { Button, Checkbox, Tooltip } from "@skbkontur/react-ui";
import type { JSX } from "react";

import { useCustomSettings } from "../../CustomSettingsContext";
import { TaskState } from "../../Domain/Api/TaskState";
import { getAllTaskStates } from "../../Domain/TaskStateExtensions";

import { jsStyles } from "./TaskStatesSelect.styles";

export interface TaskStatesSelectProps {
    value: TaskState[];
    onChange: (selectedTaskStates: TaskState[]) => void;
}

export const TaskStatesSelect = ({ value, onChange }: TaskStatesSelectProps) => {
    const { customStateCaptions } = useCustomSettings();

    const isItemSelected = (item: TaskState): boolean => Boolean(value.find(i => i === item));

    const selectItem = (val: boolean, taskState: TaskState) => {
        const newSelectedArray = value.slice();

        if (val) {
            newSelectedArray.push(taskState);
        } else {
            const index = newSelectedArray.findIndex(i => i === taskState);
            newSelectedArray.splice(index, 1);
        }
        onChange(newSelectedArray);
    };

    const renderTooltip = (): JSX.Element => (
        <ColumnStack block gap={2}>
            {getAllTaskStates()
                .filter(x => customStateCaptions[x])
                .map((item, index) => (
                    <Fit key={index}>
                        <Checkbox
                            data-tid={item}
                            checked={isItemSelected(item)}
                            onValueChange={val => selectItem(val, item)}>
                            {customStateCaptions[item]}
                        </Checkbox>
                    </Fit>
                ))}
        </ColumnStack>
    );

    return (
        <span>
            <Tooltip render={renderTooltip} trigger="click" pos="bottom left">
                <Button>
                    <span data-tid="ButtonText" className={jsStyles.buttonText()}>
                        {value.length ? `Выбрано состояний: ${value.length}` : "Выбрать состояние"}
                    </span>
                    <ArrowShapeTriangleADownIcon16Regular />
                </Button>
            </Tooltip>
        </span>
    );
};
