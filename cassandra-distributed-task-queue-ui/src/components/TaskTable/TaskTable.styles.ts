import { css } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

export const jsStyles = {
    taskDetailsRow(): string {
        return css`
            margin-bottom: 5px;
        `;
    },

    modalText(t: Theme): string {
        return css`
            color: ${t.textColorDefault};
        `;
    },
};
