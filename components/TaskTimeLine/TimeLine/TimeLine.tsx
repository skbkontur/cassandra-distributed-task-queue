import * as React from "react";

import cn from "./TimeLine.less";
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
            <div className={cn("root")} data-tid={"InnerTimeLine"}>
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
            <div className={cn("line-up")} />
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
            <div className={cn("branch-node")}>
                <div className={cn("hor-line")} ref={el => (this.line = el)} />
                <div className={cn("branch-nodes")} ref={el => (this.branches = el)}>
                    {children}
                </div>
            </div>
        );
    }
};

TimeLine.Entry = function TimeLineEntry({ children, icon, iconColor }: TimeLineEntryProps): JSX.Element {
    return (
        <div className={cn("entry")}>
            <div className={cn("icon")}>
                {React.cloneElement(icon, { color: iconColor })}
                <div className={cn("line")} />
            </div>
            <div className={cn("content")}>{children}</div>
        </div>
    );
};

TimeLine.Cycled = TimeLineCycled;
