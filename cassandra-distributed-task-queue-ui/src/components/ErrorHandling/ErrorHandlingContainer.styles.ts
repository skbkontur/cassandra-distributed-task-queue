import { memoizeGetStyles } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

import { baseSize } from "../Layouts/CommonLayout.styles";

export const getStyles = memoizeGetStyles(({ css }) => ({
    modalText: (t: Theme) => css`
        color: ${t.textColorDefault};
    `,

    header: () => css`
        margin: 0;
        font-size: 18px;
        line-height: 25px;
        font-weight: 500;
    `,

    content: () => css`
        & > p {
            margin: 0;
        }
    `,

    userMessage: () => css`
        & > p {
            margin-bottom: ${baseSize}px;
        }
    `,

    errorMessageWrap: () => css`
        min-width: 100%;
        width: 0;
    `,

    stackTraceContainer: () => css`
        margin-bottom: ${baseSize * 4}px;
    `,

    stackTrace: (t: Theme) => css`
        font-family: "Consolas", monospace;
        font-size: 12px;
        padding: ${baseSize}px ${baseSize * 2}px;
        margin: ${baseSize}px ${-baseSize * 2}px;
        background-color: ${t.bgDisabled};
        white-space: pre-wrap;
        word-wrap: break-word;
        max-height: 250px;
        overflow-y: auto;
    `,

    stackTraces: () => css`
        margin: ${baseSize * 6}px 0;
        max-width: 800px;
    `,
}));
