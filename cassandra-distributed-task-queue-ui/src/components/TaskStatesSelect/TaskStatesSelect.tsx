import { IconArrowShapeTriangleADownRegular16 } from "@skbkontur/icons/IconArrowShapeTriangleADownRegular16";
import { ColumnStack } from "@skbkontur/react-stack-layout";
import { Button, Checkbox, Tooltip } from "@skbkontur/react-ui";
import { useStyles } from "@skbkontur/react-ui/lib/renderEnvironment";
import { ReactElement } from "react";

import { useCustomSettings } from "../../CustomSettingsContext";
import { TaskState } from "../../Domain/Api/TaskState";
import { getAllTaskStates } from "../../Domain/TaskStateExtensions";

import { getStyles } from "./TaskStatesSelect.styles";

export interface TaskStatesSelectProps {
    value: TaskState[];
    onChange: (selectedTaskStates: TaskState[]) => void;
}

export const TaskStatesSelect = ({ value, onChange }: TaskStatesSelectProps) => {
    const { customStateCaptions } = useCustomSettings();
    const jsStyles = useStyles(getStyles);

    const isItemSelected = (item: TaskState): boolean => value.some(i => i === item);

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

    const renderTooltip = (): ReactElement => (
        <ColumnStack block>
            {getAllTaskStates()
                .filter(x => customStateCaptions[x])
                .map((item, index) => (
                    <Checkbox
                        key={index}
                        data-tid={item}
                        checked={isItemSelected(item)}
                        onValueChange={val => selectItem(val, item)}>
                        {customStateCaptions[item]}
                    </Checkbox>
                ))}
        </ColumnStack>
    );

    return (
        <span>
            <Tooltip render={renderTooltip} trigger="click" pos="bottom left">
                <Button use="outline">
                    <span data-tid="ButtonText" className={jsStyles.buttonText()}>
                        {value.length ? `Выбрано состояний: ${value.length}` : "Выбрать состояние"}
                    </span>
                    <IconArrowShapeTriangleADownRegular16 />
                </Button>
            </Tooltip>
        </span>
    );
};
