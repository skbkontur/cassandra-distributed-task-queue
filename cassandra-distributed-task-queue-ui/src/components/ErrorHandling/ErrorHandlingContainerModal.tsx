import { CopyToClipboardToast } from "@skbkontur/edi-ui";
import { IconCopyRegular16 } from "@skbkontur/icons/IconCopyRegular16";
import { Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Button, Link, Modal, ThemeContext } from "@skbkontur/react-ui";
import { useStyles } from "@skbkontur/react-ui/lib/renderEnvironment";
import { ReactElement, useContext, useEffect, useState } from "react";

import { getStyles } from "./ErrorHandlingContainer.styles";

interface ErrorHandlingContainerModalProps {
    canClose: boolean;
    onClose: () => void;
    message: string;
    stack: Nullable<string>;
    serverStack: Nullable<string>;
}

const copyData = async (stack: Nullable<string>) => {
    if (stack) {
        await CopyToClipboardToast.copyText(stack);
    }
};

export const ErrorHandlingContainerModal = ({
    canClose,
    onClose,
    message,
    stack,
    serverStack,
}: ErrorHandlingContainerModalProps): ReactElement => {
    const [showStack, setShowStack] = useState(false);
    const theme = useContext(ThemeContext);
    const styles = useStyles(getStyles);

    useEffect(() => {
        const handleKeyPress = (e: KeyboardEvent) => {
            if (e.key === "h") {
                setShowStack(true);
            }
        };

        window.addEventListener("keypress", handleKeyPress);
        return () => window.removeEventListener("keypress", handleKeyPress);
    }, []);

    return (
        <Modal data-tid="ErrorHandlingContainerModal" onClose={canClose ? onClose : undefined} noClose={!canClose}>
            <Modal.Header data-tid="Header">
                <span className={styles.modalText(theme)}>Произошла непредвиденная ошибка</span>
            </Modal.Header>
            <Modal.Body>
                <div className={styles.modalText(theme)}>
                    <div className={styles.userMessage()}>
                        <div data-tid="CallToActionInErrorMessage">
                            <div className={styles.content()}>
                                <p>Попробуйте повторить запрос или обновить страницу через некоторое время.</p>
                            </div>
                        </div>
                    </div>
                    {showStack && (
                        <div>
                            <hr />
                            <div className={styles.errorMessageWrap()} data-tid="ErrorMessage">
                                {message}
                            </div>
                        </div>
                    )}
                    {showStack && (
                        <div className={styles.stackTraces()}>
                            {stack && (
                                <RowStack baseline block gap={2}>
                                    <Fit>
                                        <h4 className={styles.header()}>Client stack trace</h4>
                                    </Fit>
                                    <Fit>
                                        <Link icon={<IconCopyRegular16 />} onClick={() => copyData(stack)}>
                                            Скопировать
                                        </Link>
                                    </Fit>
                                </RowStack>
                            )}
                            {stack && (
                                <div className={styles.stackTraceContainer()}>
                                    <pre data-tid="ClientErrorStack" className={styles.stackTrace(theme)}>
                                        {stack}
                                    </pre>
                                </div>
                            )}
                            {serverStack && (
                                <RowStack baseline block gap={2}>
                                    <Fit>
                                        <h4 className={styles.header()}>Server stack trace</h4>
                                    </Fit>
                                    <Fit>
                                        <Link icon={<IconCopyRegular16 />} onClick={() => copyData(serverStack)}>
                                            Скопировать
                                        </Link>
                                    </Fit>
                                </RowStack>
                            )}
                            {serverStack && (
                                <div className={styles.stackTraceContainer()}>
                                    <pre data-tid="ServerErrorStack" className={styles.stackTrace(theme)}>
                                        {serverStack}
                                    </pre>
                                </div>
                            )}
                        </div>
                    )}
                </div>
            </Modal.Body>
            {canClose && (
                <Modal.Footer panel>
                    <Button use="outline" onClick={onClose} data-tid="CloseButton">
                        Закрыть
                    </Button>
                </Modal.Footer>
            )}
        </Modal>
    );
};
