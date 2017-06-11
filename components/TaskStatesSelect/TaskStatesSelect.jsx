// @flow
import React from 'react';
import { getAllTaskStates } from '../../Domain/TaskState';
import type { TaskState } from '../../Domain/TaskState';
import { TaskStates } from '../../Domain/TaskState';
import { Button, Icon, Tooltip, Checkbox } from 'ui';
import { ColumnStack, Fit } from 'ui/layout';
import cn from './TaskStatesSelect.less';

export type TaskStatesSelectProps = {
    value: TaskState[];
    onChange: (selectedTaskStates: TaskState[]) => void;
};

const TaskStateCaptions = {
    [TaskStates.Unknown]: 'Unknown',
    [TaskStates.New]: 'New',
    [TaskStates.WaitingForRerun]: 'Waiting for rerun',
    [TaskStates.WaitingForRerunAfterError]: 'Waiting for rerun after error',
    [TaskStates.Finished]: 'Finished',
    [TaskStates.InProcess]: 'In process',
    [TaskStates.Fatal]: 'Fatal',
    [TaskStates.Canceled]: 'Canceled',
};

export default class TaskStatesSelect extends React.Component {
    props: TaskStatesSelectProps;

    render(): React.Element<*> {
        const { value } = this.props;
        return (
            <Tooltip render={() => this.renderTooltip()} trigger='click' pos='bottom left'>
                <Button>
                    <span data-tid='ButtonText' className={cn('button-text')}>
                        {value.length ? `Выбрано состояний: ${value.length}` : 'Выбрать состояние'}
                    </span>
                    <Icon name='caret-bottom' />
                </Button>
            </Tooltip>
        );
    }

    renderTooltip(): React.Element<*> {
        return (
            <ColumnStack block gap={2}>
                {getAllTaskStates().map((item, index) => {
                    return (
                        <Fit key={index}>
                            <Checkbox
                                data-tid={item}
                                checked={this.isItemSelected(item)}
                                onChange={(e, val) => this.selectItem(val, item)}>
                                {TaskStateCaptions[item]}
                            </Checkbox>
                        </Fit>
                    );
                })}
            </ColumnStack>
        );
    }

    isItemSelected(item: TaskState): boolean {
        const { value } = this.props;
        return Boolean(value.find(i => i === item));
    }

    selectItem(val: boolean, taskState: TaskState) {
        const { value, onChange } = this.props;
        const newSelectedArray = value.slice();

        if (val) {
            newSelectedArray.push(taskState);
        }
        else {
            const index = newSelectedArray.findIndex(i => i === taskState);
            newSelectedArray.splice(index, 1);
        }
        onChange(newSelectedArray);
    }
}
