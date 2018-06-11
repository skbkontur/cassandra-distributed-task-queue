import * as React from "react";
import { Icon } from "ui";

import cn from "./TimeLine.less";

export type TimeLineCycledProps = {
    children?: any;
    content?: Nullable<JSX.Element> | Nullable<string>;
    icon?: Nullable<string>;
};

export default class TimeLineCycled extends React.Component<TimeLineCycledProps> {
    refs: {
        entries: HTMLElement;
        lines: HTMLElement;
    };

    componentDidUpdate() {
        this.updateLinesHeight();
    }

    componentDidMount() {
        this.updateLinesHeight();
    }

    updateLinesHeight() {
        if (this.refs.entries != null) {
            const entries = this.refs.entries.children;
            const lastEntry = entries[entries.length - 1];
            const lastEntryHeight = lastEntry.clientHeight;
            const lines = this.refs.lines;
            if (!isNaN(lastEntryHeight) && lines != null) {
                lines.style.marginBottom = (lastEntryHeight - 22).toString() + "px";
            }
        }
    }

    render(): JSX.Element {
        const { children, content, icon } = this.props;

        return (
            <div className={cn("cycle")}>
                <div ref="entries" className={cn("entries")}>
                    {children}
                </div>
                <div className={cn("lines")} ref="lines">
                    <div className={cn("line-1")} />
                    <div className={cn("line-2")} />
                    <div className={cn("line-3")} />
                </div>
                {icon && (
                    <div className={cn("icon")}>
                        <Icon name={icon} />
                    </div>
                )}
                {content && <div className={cn("info")}>{content}</div>}
            </div>
        );
    }
}
