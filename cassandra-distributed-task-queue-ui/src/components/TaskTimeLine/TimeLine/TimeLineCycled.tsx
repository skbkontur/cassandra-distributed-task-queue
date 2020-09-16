import React from "react";

import styles from "./TimeLine.less";

export interface TimeLineCycledProps {
    children?: any;
    content?: Nullable<JSX.Element> | Nullable<string>;
    icon?: JSX.Element;
}

export class TimeLineCycled extends React.Component<TimeLineCycledProps> {
    public entries: HTMLElement | null = null;
    public lines: HTMLElement | null = null;

    public componentDidUpdate() {
        this.updateLinesHeight();
    }

    public componentDidMount() {
        this.updateLinesHeight();
    }

    public updateLinesHeight() {
        if (this.entries != null) {
            const entries = this.entries.children;
            const lastEntry = entries[entries.length - 1];
            const lastEntryHeight = lastEntry.clientHeight;
            const lines = this.lines;
            if (!isNaN(lastEntryHeight) && lines != null) {
                lines.style.marginBottom = (lastEntryHeight - 22).toString() + "px";
            }
        }
    }

    public render(): JSX.Element {
        const { children, content, icon } = this.props;

        return (
            <div className={styles.cycle}>
                <div ref={el => (this.entries = el)} className={styles.entries}>
                    {children}
                </div>
                <div className={styles.lines} ref={el => (this.lines = el)}>
                    <div className={styles.line1} />
                    <div className={styles.line2} />
                    <div className={styles.line3} />
                </div>
                {icon && <div className={styles.icon}>{icon}</div>}
                {content && <div className={styles.info}>{content}</div>}
            </div>
        );
    }
}
