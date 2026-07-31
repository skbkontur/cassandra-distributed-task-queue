import type { Emotion } from "@emotion/css/create-instance";
import { memoizeGetStyles } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

import { TaskState } from "../../../Domain/Api/TaskState";

const getBackgroundColor = (theme: Theme, state: TaskState) => {
    switch (state) {
        case TaskState.Finished:
            return theme.bgDefault;
        case TaskState.New:
        case TaskState.WaitingForRerun:
        case TaskState.InProcess:
            return theme.bgDisabled;
        case TaskState.Fatal:
        case TaskState.Canceled:
        case TaskState.WaitingForRerunAfterError:
            return "rgba(255, 0, 0, 0.1)";
        default:
            return "rgba(255, 255, 0, 0.1)";
    }
};

const getBorderColor = (theme: Theme, state: TaskState) => {
    switch (state) {
        case TaskState.Finished:
            return theme.borderColorGrayLight;
        case TaskState.New:
        case TaskState.WaitingForRerun:
        case TaskState.InProcess:
            return theme.borderColorGrayDark;
        case TaskState.Fatal:
        case TaskState.Canceled:
        case TaskState.WaitingForRerunAfterError:
            return theme.borderColorError;
        default:
            return theme.borderColorWarning;
    }
};

export const getTaskStateClassName = (css: Emotion["css"], theme: Theme, state: TaskState): string => css`
    background-color: ${getBackgroundColor(theme, state)};
    border: 1px solid ${getBorderColor(theme, state)};
`;

export const getStyles = memoizeGetStyles(({ css }) => ({
    checkbox: () => css`
        margin-top: -4px;
    `,

    taskDetails: () => css`
        padding: 8px;
        border-radius: 16px;
        min-width: 548px;
    `,

    infoBlock1: () => css`
        min-width: 280px;
    `,

    name: () => css`
        font-size: 16px;
    `,

    id: () => css`
        font-size: 12px;
    `,

    stateName: () => css`
        font-size: 12px;
    `,

    attempts: () => css`
        font-size: 12px;
        margin-left: 10px;
    `,

    dates: () => css`
        font-size: 12px;
    `,

    dateCaption: (t: Theme) => css`
        display: inline-block;
        width: 70px;
        color: ${t.textColorDisabled};
    `,

    parentTask: () => css`
        font-size: 12px;
    `,
}));
