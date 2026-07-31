import { memoizeGetStyles } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

export const getStyles = memoizeGetStyles(({ css }) => ({
    taskDetailsRow: () => css`
        margin-bottom: 5px;
    `,

    modalText: (t: Theme) => css`
        color: ${t.textColorDefault};
    `,
}));
