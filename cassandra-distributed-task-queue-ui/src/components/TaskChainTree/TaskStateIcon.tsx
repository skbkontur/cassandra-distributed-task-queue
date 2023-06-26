import { CheckAIcon } from "@skbkontur/icons/esm/icons/CheckAIcon";
import { QuestionSquareIcon } from "@skbkontur/icons/esm/icons/QuestionSquareIcon";
import { TimeClockIcon } from "@skbkontur/icons/esm/icons/TimeClockIcon";
import { XCircleIcon } from "@skbkontur/icons/esm/icons/XCircleIcon";
import { XIcon } from "@skbkontur/icons/esm/icons/XIcon";
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
            return <QuestionSquareIcon color={getIconColor(theme, "warning")} />;
        case TaskState.New:
            return <TimeClockIcon color={getIconColor(theme, "waiting")} />;
        case TaskState.WaitingForRerun:
            return <TimeClockIcon color={getIconColor(theme, "waiting")} />;
        case TaskState.WaitingForRerunAfterError:
            return <TimeClockIcon color={getIconColor(theme, "error")} />;
        case TaskState.Finished:
            return <CheckAIcon color={getIconColor(theme, "success")} />;
        case TaskState.InProcess:
            return <TimeClockIcon color={getIconColor(theme, "waiting")} />;
        case TaskState.Fatal:
            return <XCircleIcon color={getIconColor(theme, "error")} />;
        case TaskState.Canceled:
            return <XIcon color={getIconColor(theme, "error")} />;
        default:
            return <CheckAIcon />;
    }
}
