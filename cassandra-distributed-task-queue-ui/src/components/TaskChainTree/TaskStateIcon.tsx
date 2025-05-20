import { CheckAIcon16Regular } from "@skbkontur/icons/CheckAIcon16Regular";
import { QuestionSquareIcon16Regular } from "@skbkontur/icons/QuestionSquareIcon16Regular";
import { TimeClockIcon16Regular } from "@skbkontur/icons/TimeClockIcon16Regular";
import { XCircleIcon16Regular } from "@skbkontur/icons/XCircleIcon16Regular";
import { XIcon16Regular } from "@skbkontur/icons/XIcon16Regular";
import { ThemeContext } from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import { useContext, ReactElement } from "react";

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

export function TaskStateIcon({ taskState }: TaskStateIconProps): ReactElement {
    const theme = useContext(ThemeContext);

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
