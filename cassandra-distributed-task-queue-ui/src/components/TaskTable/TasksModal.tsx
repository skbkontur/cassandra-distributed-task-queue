import { ColumnStack, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Input, Modal, ThemeContext } from "@skbkontur/react-ui";
import React from "react";

import { numberToString } from "../../Domain/numberToString";
import { jsStyles } from "../ErrorHandling/ErrorHandlingContainer.styles";

interface TasksModalProps {
    modalType: "Rerun" | "Cancel";
    counter: number;
    onCancelAll: () => void;
    onRerunAll: () => void;
    onCloseModal: () => void;
}

export function TasksModal({
    modalType,
    counter,
    onCancelAll,
    onRerunAll,
    onCloseModal,
}: TasksModalProps): JSX.Element {
    const [manyTaskConfirm, setManyTaskConfirm] = React.useState("");
    const theme = React.useContext(ThemeContext);

    const confirmedRegExp = /б.*л.*я/i;

    return (
        <Modal onClose={onCloseModal} width={500} data-tid="ConfirmMultipleOperationModal">
            <Modal.Header>
                <span className={jsStyles.modalText(theme)}>Нужно подтверждение</span>
            </Modal.Header>
            <Modal.Body>
                <ColumnStack gap={2} className={jsStyles.modalText(theme)}>
                    <Fit>
                        <span data-tid="ModalText">
                            {modalType === "Rerun"
                                ? "Уверен, что все эти таски надо перезапустить?"
                                : "Уверен, что все эти таски надо остановить?"}
                        </span>
                    </Fit>
                    {counter > 100 && [
                        <Fit key="text">
                            Это действие может задеть больше 100 тасок, если это точно надо сделать, то напиши прописью
                            количество тасок (их {counter}):
                        </Fit>,
                        <Fit key="input">
                            <Input
                                data-tid="ConfirmationInput"
                                value={manyTaskConfirm}
                                onValueChange={val => setManyTaskConfirm(val)}
                            />
                        </Fit>,
                    ]}
                </ColumnStack>
            </Modal.Body>
            <Modal.Footer>
                <RowStack gap={2}>
                    <Fit>
                        {modalType === "Rerun" ? (
                            <Button
                                data-tid="RerunButton"
                                use="success"
                                disabled={
                                    counter > 100 &&
                                    !confirmedRegExp.test(manyTaskConfirm) &&
                                    manyTaskConfirm !== numberToString(counter)
                                }
                                onClick={onRerunAll}>
                                Перезапустить все
                            </Button>
                        ) : (
                            <Button
                                data-tid="CancelButton"
                                use="danger"
                                disabled={
                                    counter > 100 &&
                                    !confirmedRegExp.test(manyTaskConfirm) &&
                                    manyTaskConfirm !== numberToString(counter)
                                }
                                onClick={onCancelAll}>
                                Остановить все
                            </Button>
                        )}
                    </Fit>
                    <Fit>
                        <Button data-tid="CloseButton" onClick={onCloseModal}>
                            Закрыть
                        </Button>
                    </Fit>
                </RowStack>
            </Modal.Footer>
        </Modal>
    );
}
