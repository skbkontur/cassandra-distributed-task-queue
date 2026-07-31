import { memoizeGetStyles } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

import { baseSize } from "../Layouts/CommonLayout.styles";

export const getStyles = memoizeGetStyles(({ css }) => ({
    dateRangeItem: () => css`
        margin-right: ${baseSize * 2}px;
        line-height: 32px;

        &:last-child {
            margin-right: 0;
        }
    `,

    templates: (t: Theme) => css`
        margin: ${baseSize}px 0;
        color: ${t.textColorDisabledContrast};

        span {
            margin-right: ${baseSize * 4}px;
        }

        span:hover {
            text-decoration: underline;
            cursor: pointer;
        }
    `,

    smallGap: () => css`
        span {
            margin-right: ${3 * baseSize}px;
        }

        span:last-child {
            margin-right: 0;
        }
    `,
}));
