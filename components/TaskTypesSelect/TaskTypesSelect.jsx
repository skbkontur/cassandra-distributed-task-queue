// @flow
import React from 'react';
import {
    Button,
    Icon,
    Tooltip,
    Checkbox,
    Input,
} from 'ui';
import { ColumnStack, RowStack } from 'ui/layout';
import cn from './TaskTypesSelect.less';

export type TaskTypesSelectProps = {
    availableTaskTypes: string[];
    value: string[];
    onChange: (selectedTaskTypes: string[]) => any;
};

type TaskTypesSelectState = {
    query: string;
};


export default class TaskTypesSelect extends React.Component {
    props: TaskTypesSelectProps;
    state: TaskTypesSelectState = {
        query: '',
    };

    render(): React.Element<*> {
        const { value } = this.props;
        return (
            <Tooltip render={() => this.renderTooltip()} trigger='click' pos='bottom left'>
                <Button>
                    <span data-tid='ButtonText' className={cn('button-text')}>
                        {value.length ? `Выбрано задач: ${value.length}` : 'Выбрать тип задач'}
                    </span>
                    <Icon name='caret-bottom'/>
                </Button>
            </Tooltip>
        );
    }

    renderTooltip(): ?React.Element<*> {
        const { availableTaskTypes } = this.props;
        const { query } = this.state;
        if (availableTaskTypes.length === 0) {
            return null;
        }
        const filteredTaskTypes = query
                    ? availableTaskTypes.filter(item => {
                        return item.search(new RegExp(query, 'i')) !== -1;
                    })
                    : availableTaskTypes;
        return (
            <ColumnStack gap={3}>
                <ColumnStack.Fit>
                    <RowStack gap={2}>
                        <RowStack.Fit>
                            <Input
                                value={query}
                                rightIcon={<Icon name='search' />}
                                onChange={(e, val) => this.setState({ query: val })}
                            />
                        </RowStack.Fit>
                        <RowStack.Fit>
                            <Button onClick={() => this.clear()}>Очистить все</Button>
                        </RowStack.Fit>
                        <RowStack.Fit>
                            <Button onClick={() => this.invert()}>Инвертировать</Button>
                        </RowStack.Fit>
                    </RowStack>

                </ColumnStack.Fit>
                <ColumnStack.Fit>
                    <div className={cn('tooltip-columns-wrapper')}>
                        <div className={cn('tooltip-columns')}>
                            {filteredTaskTypes.map((item, index) => {
                                return (
                                    <Checkbox
                                        checked={this.isItemSelected(item)} key={index}
                                        onChange={(e, val) => this.selectItem(val, item)}>
                                        {item}
                                    </Checkbox>
                                );
                            })}
                        </div>
                    </div>
                </ColumnStack.Fit>
            </ColumnStack>

        );
    }

    selectItem(val: boolean, taskType: string) {
        const { value, onChange } = this.props;
        const newSelectedArray = value.slice();
        if (val) {
            newSelectedArray.push(taskType);
        }
        else {
            const index = newSelectedArray.findIndex(i => i === taskType);
            newSelectedArray.splice(index, 1);
        }
        onChange(newSelectedArray);
    }

    clear() {
        const { onChange } = this.props;
        onChange([]);
        this.setState({ query: '' });
    }

    invert() {
        const { onChange, availableTaskTypes, value } = this.props;
        this.setState({ query: '' });
        const inverseValues = availableTaskTypes.filter(item => !value.includes(item));
        onChange(inverseValues);
    }

    isItemSelected(item: string): boolean {
        const { value } = this.props;
        return Boolean(value.find(i => i === item));
    }
}
