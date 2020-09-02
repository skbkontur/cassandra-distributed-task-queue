import React from "react";

import styles from "./TimeLine.less";
import { TimeLineCycled, TimeLineCycledProps } from "./TimeLineCycled";

interface TimeLineProps {
    children?: any;
}

export class TimeLine extends React.Component<TimeLineProps> {
    public static Branch: React.ComponentType<TimeLineProps>;
    public static BranchNode: React.ComponentType<TimeLineProps>;
    public static Entry: React.ComponentType<TimeLineEntryProps>;
    public static Cycled: React.ComponentType<TimeLineCycledProps>;
    public render(): JSX.Element {
        const { children } = this.props;
        return (
            <div className={styles.root} data-tid={"InnerTimeLine"}>
                {children}
            </div>
        );
    }
}

interface TimeLineEntryProps {
    children?: React.ReactNode;
    icon: JSX.Element;
    iconColor?: string;
}

TimeLine.Branch = function TimeLineBranch({ children }: TimeLineProps): JSX.Element {
    return (
        <div className={cn("root", "branch")}>
            <div className={styles.lineUp} />
            {children}
        </div>
    );
};

TimeLine.BranchNode = class TimeLineBranchNode extends React.Component<TimeLineProps> {
    public branches: HTMLElement | null = null;
    public line: HTMLElement | null = null;

    public componentDidUpdate() {
        this.updateLinesHeight();
    }

    public componentDidMount() {
        this.updateLinesHeight();
    }

    public updateLinesHeight() {
        if (this.branches != null) {
            const branches = this.branches.children;
            const lastEntry = branches[branches.length - 1];
            const lastEntryWidth = lastEntry.clientWidth;
            const line = this.line;
            if (!isNaN(lastEntryWidth) && line != null) {
                line.style.marginRight = (lastEntryWidth - 7).toString() + "px";
            }
        }
    }

    public render(): JSX.Element {
        const { children } = this.props;
        return (
            <div className={styles.branchNode}>
                <div className={styles.horLine} ref={el => (this.line = el)} />
                <div className={styles.branchNodes} ref={el => (this.branches = el)}>
                    {children}
                </div>
            </div>
        );
    }
};

TimeLine.Entry = function TimeLineEntry({ children, icon, iconColor }: TimeLineEntryProps): JSX.Element {
    return (
        <div className={styles.entry}>
            <div className={styles.icon}>
                {React.cloneElement(icon, { color: iconColor })}
                <div className={styles.line} />
            </div>
            <div className={styles.content}>{children}</div>
        </div>
    );
};

TimeLine.Cycled = TimeLineCycled;
