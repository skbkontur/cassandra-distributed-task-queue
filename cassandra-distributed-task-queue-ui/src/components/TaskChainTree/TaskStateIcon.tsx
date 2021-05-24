import ClearIcon from "@skbkontur/react-icons/Clear";
import ClockIcon from "@skbkontur/react-icons/Clock";
import DeleteIcon from "@skbkontur/react-icons/Delete";
import HelpLiteIcon from "@skbkontur/react-icons/HelpLite";
import OkIcon from "@skbkontur/react-icons/Ok";
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
            return <HelpLiteIcon color={getIconColor(theme, "warning")} />;
        case TaskState.New:
            return <ClockIcon color={getIconColor(theme, "waiting")} />;
        case TaskState.WaitingForRerun:
            return <ClockIcon color={getIconColor(theme, "waiting")} />;
        case TaskState.WaitingForRerunAfterError:
            return <ClockIcon color={getIconColor(theme, "error")} />;
        case TaskState.Finished:
            return <OkIcon color={getIconColor(theme, "success")} />;
        case TaskState.InProcess:
            return <ClockIcon color={getIconColor(theme, "waiting")} />;
        case TaskState.Fatal:
            return <ClearIcon color={getIconColor(theme, "error")} />;
        case TaskState.Canceled:
            return <DeleteIcon color={getIconColor(theme, "error")} />;
        default:
            return <OkIcon />;
    }
}
