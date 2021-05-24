import { css } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

import { baseSize } from "../Layouts/CommonLayout.styles";

export const jsStyles = {
    ticks(t: Theme): string {
        return css`
            color: ${t.textColorDisabled};
        `;
    },

    table(t: Theme): string {
        return css`
            max-width: 100%;
            min-width: 600px;
            border-collapse: collapse;
            border-spacing: 0;

            td {
                padding: ${2 * baseSize}px ${2 * baseSize}px ${2 * baseSize}px ${4 * baseSize}px;
                border: 1px solid ${t.borderColorGrayLight};
            }

            tr {
                &:hover {
                    background-color: ${t.bgDisabled};
                }
            }
        `;
    },
};
