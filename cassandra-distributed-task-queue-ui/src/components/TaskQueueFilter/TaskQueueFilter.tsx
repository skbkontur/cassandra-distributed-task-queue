import { ColumnStack, Fill, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Input, Link, Modal, ThemeContext } from "@skbkontur/react-ui";
import React from "react";

import { RtqMonitoringSearchRequest } from "../../Domain/Api/RtqMonitoringSearchRequest";
import { DateTimeRangePicker } from "../DateTimeRangePicker/DateTimeRangePicker";
import { TaskStatesSelect } from "../TaskStatesSelect/TaskStatesSelect";
import { TaskTypesSelect } from "../TaskTypesSelect/TaskTypesSelect";

import { jsStyles } from "./TaskQueueFilter.styles";

export interface TaskQueueFilterProps {
    value: RtqMonitoringSearchRequest;
    availableTaskTypes: string[] | null;
    onChange: (filterParams: Partial<RtqMonitoringSearchRequest>) => void;
    onSearchButtonClick: () => void;
}

export function TaskQueueFilter({
    value,
    availableTaskTypes,
    onChange,
    onSearchButtonClick,
}: TaskQueueFilterProps): JSX.Element {
    const [openedModal, setOpenedModal] = React.useState(false);
    const theme = React.useContext(ThemeContext);

    const openModal = () => {
        setOpenedModal(true);
    };

    const closeModal = () => {
        setOpenedModal(false);
    };

    const renderModal = (): JSX.Element => {
        return (
            <Modal data-tid="Modal" onClose={closeModal} width={900}>
                <Modal.Header>
                    <span className={jsStyles.modalText(theme)}>Справка</span>
                </Modal.Header>
                <Modal.Body>
                    <div className={jsStyles.modalText(theme)}>
                        При поиске задач можно пользоваться следующими инструментами поиска:
                        <ol className={jsStyles.modalList()}>
                            <li>
                                Ввод значения без дополнительных указаний. В этом случае найдутся все задачи, в
                                текстовых полях которых встречается это значение.
                            </li>
                            <li>
                                Операторы AND, OR, NOT, скобки между ними. Например:{" "}
                                <code>(value1 OR value2) AND NOT value3</code>.
                            </li>
                            <li>
                                Ввод значения с указанием конкретного поля, в котором его нужно искать. Поля есть двух
                                типов: Meta и Data.SomeTaskName, в полях Meta лежит общая информация по задаче, в полях
                                Data.SomeTaskName - информация по типу задачи.
                                <br />
                                Например: <code>Meta.Id:value</code>,{" "}
                                <code>Data.FtpMessageDeliveryFinished.DocumentCirculationId:value</code>,
                                <code>Data.\*.DocumentCirculationId:(value1 OR value2)</code>. Конкретный список полей в
                                Meta можно узнать в RemoteTaskQueue.Monitoring.Storage.RtqElastcisearchSchema
                            </li>
                            <li>
                                Знаки * и ?. Например, <code>Meta.Name:val*</code> или <code>Meta.Name:val?e</code>.
                                Звездочка не может быть в начале искомого значения (<code>Meta.Name:*lue</code> не
                                работает).
                                <br />
                                Знаки можно использовать не только в значениях, но и при указании полей. По запросу{" "}
                                <code>Data.DocumentCirculationId.\*Id:value</code> во всех поля даты, которые
                                заканчиваются на id найдётся value.
                            </li>
                            <li>
                                Задание интервалов для дат. Квадратные скобки при задании интервала означают, что концы
                                включены, фигурные - что исключены. Звездочка показывает, что интервал в эту сторону
                                бесконечен.
                                <br />
                                Например: <code>date:[2012-01-01 TO 2012-12-31]</code>,
                                <code>date:&#123;* TO 2012-01-01&#125;.</code>
                            </li>
                            <li>
                                Задание интервалов для чисел аналогично заданию интервалов для дат. Например:{" "}
                                <code>count:[1 TO 5&#125;</code>.
                            </li>
                            <li>
                                Сравнение чисел. Например: <code>age:&gt;=10</code>,<code>age:&lt;=10</code>,{" "}
                                <code>age:(&gt;=10 AND &lt; 20)</code>.
                            </li>
                            <li>
                                Проверка полей на заполненность. Например: <code>NOT _exists_:Meta.ParentTaskId</code>-
                                поле пусто, <code>_exists_:Meta.ParentTaskId</code> - поле заполнено.
                            </li>
                        </ol>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <div className={jsStyles.modalFooter()}>
                        <Button onClick={closeModal}>Закрыть</Button>
                    </div>
                </Modal.Footer>
            </Modal>
        );
    };

    const { enqueueTimestampRange, queryString, states, names } = value;
    const defaultEnqueueDateTimeRange = {
        lowerBound: null,
        upperBound: null,
    };
    return (
        <RowStack block gap={1}>
            <Fill>
                <ColumnStack stretch block gap={1}>
                    <Fit>
                        <Input
                            width="100%"
                            data-tid={"SearchStringInput"}
                            value={queryString || ""}
                            onValueChange={value => onChange({ queryString: value })}
                            onKeyPress={e => {
                                if (e.key === "Enter") {
                                    onSearchButtonClick();
                                }
                            }}
                        />
                    </Fit>
                    <Fit className={jsStyles.searchLink()}>
                        <Link onClick={openModal} data-tid="OpenModalButton">
                            Что можно ввести в строку поиска
                        </Link>
                        {openedModal && renderModal()}
                    </Fit>
                </ColumnStack>
            </Fill>
            <Fit>
                <DateTimeRangePicker
                    data-tid="DateTimeRangePicker"
                    hideTime
                    value={enqueueTimestampRange || defaultEnqueueDateTimeRange}
                    onChange={value => onChange({ enqueueTimestampRange: value })}
                />
            </Fit>
            <Fit>
                <TaskTypesSelect
                    data-tid="TaskTypesSelect"
                    value={names || []}
                    disabled={availableTaskTypes === null}
                    availableTaskTypes={availableTaskTypes || []}
                    onChange={selectedTypes => onChange({ names: selectedTypes })}
                />
            </Fit>
            <Fit>
                <TaskStatesSelect
                    data-tid={"TaskStatesSelect"}
                    value={states || []}
                    onChange={selectedStates => onChange({ states: selectedStates })}
                />
            </Fit>
            <Fit>
                <Button data-tid={"SearchButton"} onClick={onSearchButtonClick} use="primary">
                    Найти
                </Button>
            </Fit>
        </RowStack>
    );
}
