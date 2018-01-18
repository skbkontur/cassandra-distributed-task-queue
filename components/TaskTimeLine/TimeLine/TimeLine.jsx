// @flow
import * as React from "react";
import { Icon } from "ui";
import TimeLineCycled from "./TimeLineCycled";
import cn from "./TimeLine.less";

type TimeLineProps = {
    children?: any,
};

export default function TimeLine({ children }: TimeLineProps): React.Element<any> {
    return <div className={cn("root")}>{children}</div>;
}

type TimeLineEntryProps = {
    children?: any,
    icon?: ?string,
    iconColor?: ?string,
};

TimeLine.Branch = function TimeLine({ children }: TimeLineProps): React.Element<any> {
    return (
        <div className={cn("root", "branch")}>
            <div className={cn("line-up")} />
            {children}
        </div>
    );
};

TimeLine.BranchNode = class TimeLineBranchNode extends React.Component<TimeLineProps> {
    refs: {
        branches: ?HTMLElement,
        line: ?HTMLElement,
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

    render(): React.Node {
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

TimeLine.Entry = function TimeLineEntry({ children, icon, iconColor }: TimeLineEntryProps): React.Element<any> {
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
