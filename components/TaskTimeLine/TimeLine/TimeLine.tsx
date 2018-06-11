import * as React from "react";
import { Icon } from "ui";

import TimeLineCycled, { TimeLineCycledProps } from "./TimeLineCycled";
import cn from "./TimeLine.less";

type TimeLineProps = {
    children?: any;
};

export default class TimeLine extends React.Component<TimeLineProps> {
    render(): JSX.Element {
        const { children } = this.props;
        return (
            <div className={cn("root")} data-tid={"InnerTimeLine"}>
                {children}
            </div>
        );
    }
    static Branch: React.ComponentType<TimeLineProps>;
    static BranchNode: React.ComponentType<TimeLineProps>;
    static Entry: React.ComponentType<TimeLineEntryProps>;
    static Cycled: React.ComponentType<TimeLineCycledProps>;
}

type TimeLineEntryProps = {
    children?: React.ReactNode;
    icon?: Nullable<string>;
    iconColor?: Nullable<string>;
};

TimeLine.Branch = function TimeLine({ children }: TimeLineProps): JSX.Element {
    return (
        <div className={cn("root", "branch")}>
            <div className={cn("line-up")} />
            {children}
        </div>
    );
};

TimeLine.BranchNode = class TimeLineBranchNode extends React.Component<TimeLineProps> {
    refs: {
        branches: HTMLElement;
        line: HTMLElement;
    };

    componentDidUpdate() {
        this.updateLinesHeight();
    }

    componentDidMount() {
        this.updateLinesHeight();
    }

    updateLinesHeight() {
        if (this.refs.branches != null) {
            const branches = this.refs.branches.children;
            const lastEntry = branches[branches.length - 1];
            const lastEntryWidth = lastEntry.clientWidth;
            const line = this.refs.line;
            if (!isNaN(lastEntryWidth) && line != null) {
                line.style.marginRight = (lastEntryWidth - 7).toString() + "px";
            }
        }
    }

    render(): JSX.Element {
        const { children } = this.props;
        return (
            <div className={cn("branch-node")}>
                <div className={cn("hor-line")} ref="line" />
                <div className={cn("branch-nodes")} ref="branches">
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
                <Icon name={icon} color={iconColor} />
                <div className={cn("line")} />
            </div>
            <div className={cn("content")}>{children}</div>
        </div>
    );
};

TimeLine.Cycled = TimeLineCycled;
