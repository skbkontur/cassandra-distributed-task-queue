import { ArrowShapeTriangleADownIcon16Regular, SearchLoupeIcon16Regular } from "@skbkontur/icons";
import { ColumnStack, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Checkbox, Input, Tooltip } from "@skbkontur/react-ui";
import React from "react";

import { jsStyles } from "./TaskTypesSelect.styles";

export interface TaskTypesSelectProps {
    availableTaskTypes: string[];
    value: string[];
    disabled?: boolean;
    onChange: (selectedTaskTypes: string[]) => void;
}

interface TaskTypesSelectState {
    query: string;
}

export class TaskTypesSelect extends React.Component<TaskTypesSelectProps, TaskTypesSelectState> {
    public state: TaskTypesSelectState = {
        query: "",
    };

    public render(): JSX.Element {
        const { disabled, value } = this.props;
        return (
            <span>
                <Tooltip render={this.renderTooltip} trigger="click" pos="bottom left" data-tid="Tooltip">
                    <Button disabled={disabled}>
                        <span className={jsStyles.buttonText()}>
                            {value.length ? `Выбрано задач: ${value.length}` : "Выбрать тип задач"}
                        </span>
                        <ArrowShapeTriangleADownIcon16Regular />
                    </Button>
                </Tooltip>
            </span>
        );
    }

    private selectItem(val: boolean, taskType: string) {
        const { value, onChange } = this.props;
        const newSelectedArray = value.slice();
        if (val) {
            newSelectedArray.push(taskType);
        } else {
            const index = newSelectedArray.findIndex(i => i === taskType);
            newSelectedArray.splice(index, 1);
        }
        onChange(newSelectedArray);
    }

    private clear() {
        const { onChange } = this.props;
        onChange([]);
        this.setState({ query: "" });
    }

    private invert() {
        const { onChange, availableTaskTypes, value } = this.props;
        this.setState({ query: "" });
        const inverseValues = availableTaskTypes.filter(item => !value.includes(item));
        onChange(inverseValues);
    }

    private isItemSelected(item: string): boolean {
        const { value } = this.props;
        return Boolean(value.find(i => i === item));
    }

    private readonly renderTooltip = (): Nullable<JSX.Element> => {
        const { availableTaskTypes, disabled } = this.props;
        const { query } = this.state;
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
                            <Input
                                value={query}
                                rightIcon={<SearchLoupeIcon16Regular />}
                                onValueChange={val => this.setState({ query: val })}
                            />
                        </Fit>
                        <Fit>
                            <Button onClick={() => this.clear()}>Очистить все</Button>
                        </Fit>
                        <Fit>
                            <Button onClick={() => this.invert()}>Инвертировать</Button>
                        </Fit>
                    </RowStack>
                </Fit>
                <Fit>
                    <div className={jsStyles.tooltipColumnsWrapper()}>
                        <div className={jsStyles.tooltipColumns()}>
                            {filteredTaskTypes.map((item, index) => (
                                <Checkbox
                                    data-tid={item}
                                    checked={this.isItemSelected(item)}
                                    key={index}
                                    onValueChange={val => this.selectItem(val, item)}>
                                    {item}
                                </Checkbox>
                            ))}
                        </div>
                    </div>
                </Fit>
            </ColumnStack>
        );
    };
}
