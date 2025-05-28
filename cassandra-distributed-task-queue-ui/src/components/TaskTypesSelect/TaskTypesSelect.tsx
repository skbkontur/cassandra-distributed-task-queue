import { ArrowShapeTriangleADownIcon16Regular } from "@skbkontur/icons/ArrowShapeTriangleADownIcon16Regular";
import { SearchLoupeIcon16Regular } from "@skbkontur/icons/SearchLoupeIcon16Regular";
import { ColumnStack, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Checkbox, Input, Tooltip } from "@skbkontur/react-ui";
import { ReactElement, useState } from "react";

import { jsStyles } from "./TaskTypesSelect.styles";

export interface TaskTypesSelectProps {
    availableTaskTypes: string[];
    value: string[];
    disabled?: boolean;
    onChange: (selectedTaskTypes: string[]) => void;
}

export const TaskTypesSelect = ({
    availableTaskTypes,
    value,
    disabled,
    onChange,
}: TaskTypesSelectProps): ReactElement => {
    const [query, setQuery] = useState("");

    const selectItem = (val: boolean, taskType: string) => {
        const newSelectedArray = value.slice();
        if (val) {
            newSelectedArray.push(taskType);
        } else {
            const index = newSelectedArray.findIndex(i => i === taskType);
            newSelectedArray.splice(index, 1);
        }
        onChange(newSelectedArray);
    };

    const clear = () => {
        onChange([]);
        setQuery("");
    };

    const invert = () => {
        const inverseValues = availableTaskTypes.filter(item => !value.includes(item));
        onChange(inverseValues);
        setQuery("");
    };

    const isItemSelected = (item: string): boolean => {
        return Boolean(value.find(i => i === item));
    };

    const renderTooltip = (): Nullable<ReactElement> => {
        if (disabled || availableTaskTypes.length === 0) {
            return null;
        }
        const filteredTaskTypes = query
            ? availableTaskTypes.filter(item => item.search(new RegExp(query, "i")) !== -1)
            : availableTaskTypes;

        return (
            <ColumnStack gap={3}>
                <Fit>
                    <RowStack gap={2}>
                        <Fit>
                            <Input value={query} rightIcon={<SearchLoupeIcon16Regular />} onValueChange={setQuery} />
                        </Fit>
                        <Fit>
                            <Button onClick={clear}>Очистить все</Button>
                        </Fit>
                        <Fit>
                            <Button onClick={invert}>Инвертировать</Button>
                        </Fit>
                    </RowStack>
                </Fit>
                <Fit>
                    <div className={jsStyles.tooltipColumnsWrapper()}>
                        <div className={jsStyles.tooltipColumns()}>
                            {filteredTaskTypes.map((item, index) => (
                                <Checkbox
                                    data-tid={item}
                                    checked={isItemSelected(item)}
                                    key={index}
                                    onValueChange={val => selectItem(val, item)}>
                                    {item}
                                </Checkbox>
                            ))}
                        </div>
                    </div>
                </Fit>
            </ColumnStack>
        );
    };

    return (
        <span>
            <Tooltip render={renderTooltip} trigger="click" pos="bottom left" data-tid="Tooltip">
                <Button disabled={disabled}>
                    <span className={jsStyles.buttonText()}>
                        {value.length ? `Выбрано задач: ${value.length}` : "Выбрать тип задач"}
                    </span>
                    <ArrowShapeTriangleADownIcon16Regular />
                </Button>
            </Tooltip>
        </span>
    );
};
