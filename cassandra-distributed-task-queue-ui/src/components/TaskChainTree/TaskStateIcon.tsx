import {
    CheckAIcon16Regular,
    QuestionSquareIcon16Regular,
    TimeClockIcon16Regular,
    XCircleIcon16Regular,
    XIcon16Regular,
} from "@skbkontur/icons";
import { ThemeContext } from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import React from "react";

import { TaskState } from "../../Domain/Api/TaskState";

interface TaskStateIconProps {
    taskState: TaskState;
}

export const getIconColor = (theme: Theme, severity: string): string | undefined => {
    switch (severity) {
        case "error":
            return theme.linkDangerHoverColor;
        case "success":
            return theme.linkSuccessColor;
        case "waiting":
            return theme.textColorDisabled;
        case "warning":
            return theme.warningMain;
        default:
            return undefined;
    }
};

export function TaskStateIcon({ taskState }: TaskStateIconProps): JSX.Element {
    const theme = React.useContext(ThemeContext);

    switch (taskState) {
        case TaskState.Unknown:
            return <QuestionSquareIcon16Regular color={getIconColor(theme, "warning")} />;
        case TaskState.New:
            return <TimeClockIcon16Regular color={getIconColor(theme, "waiting")} />;
        case TaskState.WaitingForRerun:
            return <TimeClockIcon16Regular color={getIconColor(theme, "waiting")} />;
        case TaskState.WaitingForRerunAfterError:
            return <TimeClockIcon16Regular color={getIconColor(theme, "error")} />;
        case TaskState.Finished:
            return <CheckAIcon16Regular color={getIconColor(theme, "success")} />;
        case TaskState.InProcess:
            return <TimeClockIcon16Regular color={getIconColor(theme, "waiting")} />;
        case TaskState.Fatal:
            return <XCircleIcon16Regular color={getIconColor(theme, "error")} />;
        case TaskState.Canceled:
            return <XIcon16Regular color={getIconColor(theme, "error")} />;
        default:
            return <CheckAIcon16Regular />;
    }
}
