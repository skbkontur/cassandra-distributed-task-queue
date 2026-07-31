import { memoizeGetStyles } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

export const baseSize = 5;
const newBaseSize = 4;

const globalHorizontalPadding = 7 * newBaseSize;
const globalVerticalPadding = 10 * newBaseSize;

export const getStyles = memoizeGetStyles(({ css }) => ({
    commonLayout: (t: Theme) => css`
        color: ${t.textColorDefault};
        background-color: ${t.bgDefault};
        position: relative;
        flex: 1 1 auto;
        display: flex;
        flex-direction: column;
        height: 100%;
        padding: ${globalVerticalPadding}px ${globalHorizontalPadding}px 0;
    `,

    withArrow: () => css`
        padding: ${globalVerticalPadding}px ${newBaseSize * 12}px 0;
    `,

    topRightTools: () => css`
        position: absolute;
        top: ${baseSize}px;
        right: ${baseSize}px;
        z-index: 1;
    `,

    headerContent: () => css`
        margin-top: ${3 * newBaseSize}px;
    `,

    headerTitle: () => css`
        margin: 0;
        font-weight: 700;
        font-size: 29px;
        line-height: 48px;
    `,

    content: () => css`
        display: flex;
        flex-direction: column;
        flex: 1 1 auto;
        padding-bottom: ${newBaseSize * 5}px;
    `,

    header: () => css`
        margin-bottom: ${5 * newBaseSize}px;
    `,

    borderBottom: (t: Theme) => css`
        border-bottom: 2px solid ${t.grayXLight};
    `,

    backLink: () => css`
        position: absolute;
        left: 0;
        height: ${newBaseSize * 12}px;
        width: ${newBaseSize * 12}px;
        display: flex;
        align-items: center;
        justify-content: center;
    `,

    backLinkIcon: (t: Theme) => css`
        display: block;
        color: #757575;

        &:hover {
            color: ${t.textColorDefault};
        }
    `,

    loader: () => css`
        display: flex !important;
        flex-direction: column;
        flex: 1 1 auto;
    `,
}));
