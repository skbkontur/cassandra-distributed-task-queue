import { memoizeGetStyles } from "@skbkontur/react-ui/lib/theming/Emotion";

export const getStyles = memoizeGetStyles(({ css }) => ({
    buttonText: () => css`
        display: inline-block;
        width: 138px;
    `,

    tooltipColumns: () => css`
        column-count: 2;
        column-gap: 30px;
        max-width: 800px;

        label {
            display: block;
        }
    `,

    tooltipColumnsWrapper: () => css`
        max-height: 650px;
        overflow: auto;
        // хак, чтобы левый край чекбоксов не съедало в тултипе при overflow: auto (k.solovei, 17.03.2020)
        padding-left: 1px;
    `,
}));
