import { css } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

import { TaskState } from "../../../Domain/Api/TaskState";

function getBackgroundColor(theme: Theme, state: TaskState) {
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
}

function getBorderColor(theme: Theme, state: TaskState) {
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
}

export const jsStyles = {
    state(theme: Theme, state: TaskState): string {
        return css`
            background-color: ${getBackgroundColor(theme, state)};
            border: 1px solid ${getBorderColor(theme, state)};
        `;
    },

    taskDetails(): string {
        return css`
            padding: 5px 5px 5px 7px;
            border-radius: 2px;
        `;
    },

    infoBlock1(): string {
        return css`
            min-width: 280px;
        `;
    },

    name(): string {
        return css`
            font-size: 16px;
        `;
    },

    id(): string {
        return css`
            font-size: 12px;
        `;
    },

    stateName(): string {
        return css`
            font-size: 12px;
        `;
    },

    attempts(): string {
        return css`
            font-size: 12px;
            margin-left: 10px;
        `;
    },

    dates(): string {
        return css`
            font-size: 12px;
        `;
    },

    dateCaption(t: Theme): string {
        return css`
            display: inline-block;
            width: 70px;
            color: ${t.textColorDisabled};
        `;
    },

    parentTask(): string {
        return css`
            font-size: 12px;
        `;
    },
};
