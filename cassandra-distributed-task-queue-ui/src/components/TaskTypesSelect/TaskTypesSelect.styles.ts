import { css } from "@skbkontur/react-ui/lib/theming/Emotion";

import { baseSize } from "../Layouts/CommonLayout.styles";

export const jsStyles = {
    buttonText() {
        return css`
            display: inline-block;
            width: 138px;
        `;
    },

    tooltipColumns() {
        return css`
            column-count: 2;
            column-gap: 30px;
            max-width: 800px;

            label {
                padding-bottom: ${2 * baseSize}px;
                display: block;
            }
        `;
    },

    tooltipColumnsWrapper() {
        return css`
            max-height: 650px;
            overflow: auto;
            // хак, чтобы левый край чекбоксов не съедало в тултипе при overflow: auto (k.solovei, 17.03.2020)
            padding-left: 1px;
        `;
    },
};
