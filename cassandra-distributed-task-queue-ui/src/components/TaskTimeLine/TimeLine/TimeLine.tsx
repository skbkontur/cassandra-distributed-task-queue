import { ThemeContext } from "@skbkontur/react-ui";
import React from "react";

import { jsStyles } from "./TimeLine.styles";
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
            <div className={jsStyles.root()} data-tid={"InnerTimeLine"}>
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
    const theme = React.useContext(ThemeContext);
    return (
        <div className={`${jsStyles.root()} ${jsStyles.branch()}`}>
            <div className={jsStyles.lineUp(theme)} />
            {children}
        </div>
    );
};

TimeLine.BranchNode = function TimeLineBranchNode({ children }: TimeLineProps) {
    const branches = React.useRef<HTMLDivElement>(null);
    const line = React.useRef<HTMLDivElement>(null);
    const theme = React.useContext(ThemeContext);

    React.useEffect(() => {
        if (branches.current != null) {
            const children = branches.current.children;
            const lastEntry = children[children.length - 1];
            const lastEntryWidth = lastEntry.clientWidth;
            const currentLine = line.current;
            if (!isNaN(lastEntryWidth) && currentLine != null) {
                currentLine.style.marginRight = (lastEntryWidth - 7).toString() + "px";
            }
        }
    });

    return (
        <div className={jsStyles.branchNode()}>
            <div className={jsStyles.horLine(theme)} ref={line} />
            <div className={jsStyles.branchNodes()} ref={branches}>
                {children}
            </div>
        </div>
    );
};

TimeLine.Entry = function TimeLineEntry({ children, icon }: TimeLineEntryProps): JSX.Element {
    const theme = React.useContext(ThemeContext);
    return (
        <div className={`__root-entry ${jsStyles.entry()}`}>
            <div className={jsStyles.icon()}>
                {icon}
                <div className={`__root-entry-line ${jsStyles.line(theme)}`} />
            </div>
            <div className={`__root-entry-content ${jsStyles.content()}`}>{children}</div>
        </div>
    );
};

TimeLine.Cycled = TimeLineCycled;
