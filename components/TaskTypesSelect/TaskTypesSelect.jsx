// @flow
import React from 'react';
import { Button, Icon, Tooltip, Checkbox, Input } from 'ui';
import { ColumnStack, RowStack, Fit } from 'ui/layout';
import cn from './TaskTypesSelect.less';

export type TaskTypesSelectProps = {
    availableTaskTypes: string[];
    value: string[];
    disabled?: boolean;
    onChange: (selectedTaskTypes: string[]) => void;
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
        const { disabled, value } = this.props;
        return (
            <Tooltip
                render={() => this.renderTooltip()}
                trigger={disabled ? 'closed' : 'click'}
                pos='bottom left'
                data-tid='Tooltip'>
                <Button disabled={disabled}>
                    <span className={cn('button-text')}>
                        {value.length ? `Выбрано задач: ${value.length}` : 'Выбрать тип задач'}
                    </span>
                    <Icon name='caret-bottom' />
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
                <Fit>
                    <RowStack gap={2}>
                        <Fit>
                            <Input
                                value={query}
                                rightIcon={<Icon name='search' />}
                                onChange={(e, val) => this.setState({ query: val })}
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
                    <div className={cn('tooltip-columns-wrapper')}>
                        <div className={cn('tooltip-columns')}>
                            {filteredTaskTypes.map((item, index) => {
                                return (
                                    <Checkbox
                                        data-tid={item}
                                        checked={this.isItemSelected(item)}
                                        key={index}
                                        onChange={(e, val) => this.selectItem(val, item)}>
                                        {item}
                                    </Checkbox>
                                );
                            })}
                        </div>
                    </div>
                </Fit>
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
