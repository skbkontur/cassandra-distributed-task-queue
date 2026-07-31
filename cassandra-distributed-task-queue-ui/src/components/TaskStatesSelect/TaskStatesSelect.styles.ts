import { memoizeGetStyles } from "@skbkontur/react-ui/lib/theming/Emotion";

export const getStyles = memoizeGetStyles(({ css }) => ({
    buttonText: () => css`
        display: inline-block;
        width: 150px;
    `,
}));
