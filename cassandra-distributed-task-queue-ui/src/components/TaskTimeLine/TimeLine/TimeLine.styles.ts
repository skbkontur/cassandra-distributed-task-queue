import { css } from "@skbkontur/react-ui/lib/theming/Emotion";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";

export const jsStyles = {
    branch(): string {
        return css`
            padding-top: 10px;
            position: relative;

            &:not(:first-child) {
                margin-left: 10px;
            }
        `;
    },

    lineUp(t: Theme): string {
        return css`
            position: absolute;
            top: 0;
            left: 6px;
            bottom: 0;
            height: 10px;
            width: 1px;
            background-color: ${t.borderColorGrayDark};
        `;
    },

    date(t: Theme): string {
        return css`
            font-size: 12px;
            color: ${t.textColorDisabled};
        `;
    },

    horLine(t: Theme): string {
        return css`
            position: absolute;
            top: 0;
            left: 6px;
            right: 0;
            height: 1px;
            background-color: ${t.borderColorGrayDark};
        `;
    },

    branchNode(): string {
        return css`
            position: relative;
            display: inline-block;
        `;
    },

    branchNodes(): string {
        return css`
            display: flex;
            flex-direction: row;
        `;
    },

    cycle(): string {
        return css`
            display: flex;
            flex-direction: row;
        `;
    },

    cycleInfo(): string {
        return css`
            margin-left: 10px;
        `;
    },

    cycleIcon(): string {
        return css`
            margin-left: 10px;
            margin-right: -5px;
        `;
    },

    lines(t: Theme): string {
        return css`
            margin-left: 10px;
            position: relative;
            overflow: hidden;
            min-width: 10px;

            > div {
                background-color: ${t.borderColorGrayDark};
                position: absolute;
            }
        `;
    },

    line2(): string {
        return css`
            position: absolute;
            top: 10px;
            right: 0;
            bottom: 10px;
            width: 1px;
        `;
    },

    line1(): string {
        return css`
            top: 10px;
            left: 0;
            right: 0;
            height: 1px;
        `;
    },

    line3(): string {
        return css`
            bottom: 10px;
            left: 0;
            right: 0;
            height: 1px;
        `;
    },

    entry(): string {
        return css`
            display: flex;
            flex-direction: row;
        `;
    },

    icon(): string {
        return css`
            position: relative;
            text-align: center;
        `;
    },

    line(t: Theme): string {
        return css`
            position: absolute;
            top: 20px;
            left: 6px;
            bottom: 0;
            width: 1px;
            background-color: ${t.borderColorGrayDark};
        `;
    },

    content(): string {
        return css`
            margin-left: 10px;
            margin-bottom: 10px;
        `;
    },

    root(): string {
        return css`
            > .__root-entry:last-child {
                .__root-entry-line {
                    display: none;
                }

                .__root-entry-content {
                    margin-bottom: 0;
                }
            }

            > .__root-cycle:last-child > .__root-cycle-entries > .__root-entry:last-child {
                .__root-entry-line {
                    display: none;
                }

                .__root-entry-content {
                    margin-bottom: 0;
                }
            }
        `;
    },
};
