import { css } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

export const jsStyles = {
    taskId(t: Theme): string {
        return css`
            font-size: 12px;
            color: ${t.textColorDisabled};
        `;
    },
};
