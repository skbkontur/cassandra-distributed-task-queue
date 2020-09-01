import * as React from "react";

import cn from "./TimeLine.less";

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
            <div className={cn("cycle")}>
                <div ref={el => (this.entries = el)} className={cn("entries")}>
                    {children}
                </div>
                <div className={cn("lines")} ref={el => (this.lines = el)}>
                    <div className={cn("line-1")} />
                    <div className={cn("line-2")} />
                    <div className={cn("line-3")} />
                </div>
                {icon && <div className={cn("icon")}>{icon}</div>}
                {content && <div className={cn("info")}>{content}</div>}
            </div>
        );
    }
}
