import { css } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

import { baseSize } from "../Layouts/CommonLayout.styles";

export const jsStyles = {
    taskDataContainer(): string {
        return css`
            max-width: 100%;
        `;
    },

    exception(t: Theme): string {
        return css`
            background: rgba(255, 255, 0, 0.1);
            border: 1px solid ${t.borderColorWarning};
            display: block;
            padding: ${3 * baseSize}px;
            overflow-x: auto;
            border-radius: ${baseSize}px;
        `;
    },

    exceptionContainer(): string {
        return css`
            width: 100%;
            overflow-x: auto;
        `;
    },
};
