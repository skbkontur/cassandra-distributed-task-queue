import { memoizeGetStyles } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

export const getStyles = memoizeGetStyles(({ css }) => ({
    taskId: (t: Theme) => css`
        font-size: 12px;
        color: ${t.textColorDisabled};
    `,
}));
