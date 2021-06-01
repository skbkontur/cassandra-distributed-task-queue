import { css } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

import { baseSize } from "../Layouts/CommonLayout.styles";

export const jsStyles = {
    modalText(t: Theme): string {
        return css`
            color: ${t.textColorDefault};

            code {
                padding: 2px 4px;
                font-size: 90%;
                color: #c7254e;
                background-color: #f9f2f4;
                border-radius: 4px;
                font-family: "Menlo", "Monaco", "Consolas", "Courier New", monospace;
            }
        `;
    },

    searchLink(): string {
        return css`
            white-space: nowrap;
        `;
    },

    modalList(): string {
        return css`
            list-style: decimal;
            padding: ${2 * baseSize}px ${3 * baseSize}px 0;
        `;
    },

    modalFooter(): string {
        return css`
            text-align: right;
        `;
    },
};
