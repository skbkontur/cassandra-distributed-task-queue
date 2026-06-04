import { IconCheckARegular16 } from "@skbkontur/icons/IconCheckARegular16";
import { IconQuestionSquareRegular16 } from "@skbkontur/icons/IconQuestionSquareRegular16";
import { IconTimeClockRegular16 } from "@skbkontur/icons/IconTimeClockRegular16";
import { IconXCircleRegular16 } from "@skbkontur/icons/IconXCircleRegular16";
import { IconXRegular16 } from "@skbkontur/icons/IconXRegular16";
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
            return <IconQuestionSquareRegular16 color={getIconColor(theme, "warning")} />;
        case TaskState.New:
            return <IconTimeClockRegular16 color={getIconColor(theme, "waiting")} />;
        case TaskState.WaitingForRerun:
            return <IconTimeClockRegular16 color={getIconColor(theme, "waiting")} />;
        case TaskState.WaitingForRerunAfterError:
            return <IconTimeClockRegular16 color={getIconColor(theme, "error")} />;
        case TaskState.Finished:
            return <IconCheckARegular16 color={getIconColor(theme, "success")} />;
        case TaskState.InProcess:
            return <IconTimeClockRegular16 color={getIconColor(theme, "waiting")} />;
        case TaskState.Fatal:
            return <IconXCircleRegular16 color={getIconColor(theme, "error")} />;
        case TaskState.Canceled:
            return <IconXRegular16 color={getIconColor(theme, "error")} />;
        default:
            return <IconCheckARegular16 />;
    }
}
