import { ThemeContext } from "@skbkontur/react-ui";
import { useStyles } from "@skbkontur/react-ui/lib/renderEnvironment";
import { type ComponentType, type ReactElement, type ReactNode, useContext, useRef, useEffect } from "react";

import { getStyles } from "./TimeLine.styles";
import { TimeLineCycled, TimeLineCycledProps } from "./TimeLineCycled";

interface TimeLineProps {
    children?: ReactNode;
}

export interface TimeLineComponent {
    (props: TimeLineProps): ReactElement;
    Branch: ComponentType<TimeLineProps>;
    BranchNode: ComponentType<TimeLineProps>;
    Entry: ComponentType<TimeLineEntryProps>;
    Cycled: ComponentType<TimeLineCycledProps>;
}

export const TimeLine: TimeLineComponent = ({ children }: TimeLineProps): ReactElement => {
    const jsStyles = useStyles(getStyles);
    return (
        <div className={jsStyles.root()} data-tid={"InnerTimeLine"}>
            {children}
        </div>
    );
};

interface TimeLineEntryProps {
    children?: ReactNode;
    icon: ReactElement;
    iconColor?: string;
}

TimeLine.Branch = function TimeLineBranch({ children }: TimeLineProps): ReactElement {
    const theme = useContext(ThemeContext);
    const jsStyles = useStyles(getStyles);
    return (
        <div className={`${jsStyles.root()} ${jsStyles.branch()}`}>
            <div className={jsStyles.lineUp(theme)} />
            {children}
        </div>
    );
};

TimeLine.BranchNode = function TimeLineBranchNode({ children }: TimeLineProps) {
    const branches = useRef<HTMLDivElement>(null);
    const line = useRef<HTMLDivElement>(null);
    const theme = useContext(ThemeContext);
    const jsStyles = useStyles(getStyles);

    useEffect(() => {
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

TimeLine.Entry = function TimeLineEntry({ children, icon }: TimeLineEntryProps): ReactElement {
    const theme = useContext(ThemeContext);
    const jsStyles = useStyles(getStyles);
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
