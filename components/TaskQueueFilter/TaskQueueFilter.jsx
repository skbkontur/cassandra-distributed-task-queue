// @flow
import React from 'react';
import TaskTypesSelect from '../TaskTypesSelect/TaskTypesSelect';
import TaskStatesSelect from '../TaskStatesSelect/TaskStatesSelect';
import type { RemoteTaskQueueSearchRequest } from '../../api/RemoteTaskQueueApi';
import {
    Button,
    Input,
    Modal,
} from 'ui';
import DateTimeRangePicker from '../../../Commons/DateTimeRangePicker/DateTimeRangePicker';

import { RowStack, ColumnStack } from 'ui/layout';
import cn from './TaskQueueFilter.less';

export type TaskQueueFilterProps = {
    value: RemoteTaskQueueSearchRequest;
    availableTaskTypes: string[];
    onChange: (filterParams: $Shape<RemoteTaskQueueSearchRequest>) => any;
    onSearchButtonClick: () => any;
};

type TaskQueueFilterState = {
    openedModal: boolean;
};

export default class TaskQueueFilter extends React.Component {
    props: TaskQueueFilterProps;
    state: TaskQueueFilterState = {
        openedModal: false,
    };

    render(): React.Element<*> {
        const { enqueueDateTimeRange, queryString, taskState, names } = this.props.value;
        const { availableTaskTypes, onChange, onSearchButtonClick } = this.props;
        const { openedModal } = this.state;
        const defaultEnqueueDateTimeRange = {
            lowerBound: null,
            upperBound: null,
        };
        return (
            <RowStack gap={1}>
                <RowStack.Fill>
                    <ColumnStack gap={1}>
                        <ColumnStack.Fit>
                            <Input
                                width={300}
                                tid={'SearchStringInput'}
                                value={queryString || ''}
                                onChange={(e, value) => onChange({ queryString: value })}
                            />
                        </ColumnStack.Fit>
                        <ColumnStack.Fit>
                            <button
                                onClick={() => this.openModal()}
                                tid='OpenModalButton'
                                className={cn('modal-button')}>
                                Что можно ввести в строку поиска
                            </button>
                            { openedModal && this.renderModal() }
                        </ColumnStack.Fit>
                    </ColumnStack>
                </RowStack.Fill>
                <RowStack.Fit>
                    <DateTimeRangePicker
                        tid={'DateTimeRangePicker'}
                        error={false}
                        // eslint-disable-next-line react/jsx-boolean-value
                        withoutHours={true}
                        value={enqueueDateTimeRange || defaultEnqueueDateTimeRange}
                        onChange={value => onChange({ enqueueDateTimeRange: value })}
                    />
                </RowStack.Fit>
                <RowStack.Fit>
                    <TaskTypesSelect
                        tid={'TaskTypesSelect'}
                        value={names || []}
                        availableTaskTypes={availableTaskTypes}
                        onChange={selectedTypes => onChange({ names: selectedTypes })}
                    />
                </RowStack.Fit>
                <RowStack.Fit>
                    <TaskStatesSelect
                        tid={'TaskStatesSelect'}
                        value={taskState || []}
                        onChange={selectedStates => onChange({ taskState: selectedStates })}
                    />
                </RowStack.Fit>
                <RowStack.Fit>
                    <Button
                        tid={'SearchButton'}
                        onClick={onSearchButtonClick}
                        use='primary'>Найти</Button>
                </RowStack.Fit>
            </RowStack>

        );
    }

    renderModal(): React.Element<*> {
        return (
            <Modal
                tid='Modal'
                onClose={() => this.closeModal()}
                width={900}>
                <Modal.Header>
                    Справка
                </Modal.Header>
                <Modal.Body>
                    При поиске задач можно пользоваться следующими инструментами поиска:
                    <ol className={cn('modal-list')}>
                        <li>Ввод значения без дополнительных указаний. В этом случае найдутся все задачи,
                            в полях которых встречается это значение.
                        </li>
                        <li>Операторы AND, OR, NOT, скобки между ними. Например:
                            <code>(value1 OR value2) AND NOT value3</code>.
                        </li>
                        <li>
                            Ввод значения с указанием конкретного поля, в котором его нужно искать.
                            Поля есть двух типов: Meta и Data, в полях Meta лежит общая информация по задаче,
                            в полях Data - информация по типу задачи.<br />
                            Например: <code>Meta.TaskId:value</code>, <code>Data.ScopeId:value</code>,
                            <code>Data.Value:(value1 OR value2)</code>.
                        </li>
                        <li>
                            Знаки * и ?. Например, <code>Data.ScopeId:val*</code> или <code>Data.ScopeId:val?e</code>.
                            Звездочка не может быть в начале искомого значения
                            (<code>Data.ScopeId:*lue</code> не работает).<br />
                            Знаки можно использовать не только в значениях, но и при указании полей.
                            По запросу <code>Data.\*Id:value</code> во всех поля даты,
                            которые заканчиваются на id найдётся value.
                        </li>
                        <li>
                            Задание интервалов для дат. Квадратные скобки при задании интервала означают,
                            что концы включены, фигурные - что исключены. Звездочка показывает,
                            что интервал в эту сторону бесконечен.<br />
                            Например: <code>date:[2012-01-01 TO 2012-12-31]</code>,
                            <code>date:&#123;* TO 2012-01-01&#125;.</code>
                        </li>
                        <li>
                            Задание интервалов для чисел аналогично заданию интервалов для дат.
                            Например: <code>count:[1 TO 5&#125;</code>.
                        </li>
                        <li>
                            Сравнение чисел. Например: <code>age:&gt;=10</code>,
                            <code>age:&lt;=10</code>, <code>age:(&gt;=10 AND &lt; 20)</code>.
                        </li>
                        <li>
                            Проверка полей на заполненность. Например: <code>_missing_:Data.Value</code>
                            - поле пусто, <code>_exists_:Data.Value</code> - поле заполнено.
                        </li>
                    </ol>
                </Modal.Body>
                <Modal.Footer>
                    <div className={cn('modal-footer')}>
                        <Button onClick={() => this.closeModal()}>Закрыть</Button>
                    </div>
                </Modal.Footer>
            </Modal>
        );
    }

    openModal() {
        this.setState({ openedModal: true });
    }

    closeModal() {
        this.setState({ openedModal: false });
    }
}
