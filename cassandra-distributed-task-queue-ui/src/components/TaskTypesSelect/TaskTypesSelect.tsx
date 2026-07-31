import { IconArrowShapeTriangleADownRegular16 } from "@skbkontur/icons/IconArrowShapeTriangleADownRegular16";
import { IconSearchLoupeRegular16 } from "@skbkontur/icons/IconSearchLoupeRegular16";
import { ColumnStack, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Checkbox, Input, Tooltip } from "@skbkontur/react-ui";
import { useStyles } from "@skbkontur/react-ui/lib/renderEnvironment";
import { ReactElement, useState } from "react";

import { getStyles } from "./TaskTypesSelect.styles";

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
    const jsStyles = useStyles(getStyles);

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
                            <Input value={query} rightIcon={<IconSearchLoupeRegular16 />} onValueChange={setQuery} />
                        </Fit>
                        <Fit>
                            <Button use="outline" onClick={clear}>
                                Очистить все
                            </Button>
                        </Fit>
                        <Fit>
                            <Button use="outline" onClick={invert}>
                                Инвертировать
                            </Button>
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
            <Tooltip
                render={renderTooltip}
                trigger="click"
                allowedPositions={["bottom right", "bottom left"]}
                data-tid="Tooltip">
                <Button use="outline" disabled={disabled}>
                    <span className={jsStyles.buttonText()}>
                        {value.length ? `Выбрано задач: ${value.length}` : "Выбрать тип задач"}
                    </span>
                    <IconArrowShapeTriangleADownRegular16 />
                </Button>
            </Tooltip>
        </span>
    );
};
