import { ThemeContext } from "@skbkontur/react-ui";
import React from "react";

import { jsStyles } from "./TimeLine.styles";

export interface TimeLineCycledProps {
    children?: React.ReactNode;
    content?: Nullable<JSX.Element> | Nullable<string>;
    icon?: JSX.Element;
}

export function TimeLineCycled({ children, content, icon }: TimeLineCycledProps): JSX.Element {
    const entries = React.useRef<HTMLDivElement>(null);
    const lines = React.useRef<HTMLDivElement>(null);
    const theme = React.useContext(ThemeContext);

    React.useEffect(() => {
        if (entries.current != null) {
            const children = entries.current.children;
            const lastEntry = children[children.length - 1];
            const lastEntryHeight = lastEntry.clientHeight;
            const currentLines = lines.current;
            if (!isNaN(lastEntryHeight) && currentLines != null) {
                currentLines.style.marginBottom = (lastEntryHeight - 22).toString() + "px";
            }
        }
    });

    return (
        <div className={`__root-cycle ${jsStyles.cycle()}`}>
            <div ref={entries} className="__root-cycle-entries">
                {children}
            </div>
            <div className={jsStyles.lines(theme)} ref={lines}>
                <div className={jsStyles.line1()} />
                <div className={jsStyles.line2()} />
                <div className={jsStyles.line3()} />
            </div>
            {icon && <div className={jsStyles.cycleIcon()}>{icon}</div>}
            {content && <div className={jsStyles.cycleInfo()}>{content}</div>}
        </div>
    );
}
