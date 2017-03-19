// @flow
import React from 'react';
import { Icon } from 'ui';
import cn from './TimeLine.less';

type TimeLineProps = {
    children?: any;
};

export default function TimeLine({ children }: TimeLineProps): React.Element<*> {
    return (
        <div className={cn('root')}>
            {children}
        </div>
    );
}

type TimeLineEntryProps = {
    children?: any;
    icon?: ?string;
    iconColor?: ?string;
};

TimeLine.Entry = function TimeLineEntry({ children, icon, iconColor }: TimeLineEntryProps): React.Element<*> {
    return (
        <div className={cn('entry')}>
            <div className={cn('icon')}>
                <Icon name={icon} color={iconColor} />
                <div className={cn('line')} />
            </div>
            <div className={cn('content')}>
                {children}
            </div>
        </div>
    );
};

type TimeLineCycledProps = {
    children?: any;
    content?: ?any;
    icon?: ?string;
};

TimeLine.Cycled = class TimeLineCycled extends React.Component {
    props: TimeLineCycledProps;

    componentDidUpdate() {
        this.updateLinesHeight();
    }

    componentDidMount() {
        this.updateLinesHeight();
    }

    updateLinesHeight() {
        if (this.refs.entries) {
            const entries = this.refs.entries.children;
            const lastEntry = entries[entries.length - 1];
            const lastEntryHeight = lastEntry.clientHeight;
            if (!isNaN(lastEntryHeight)) {
                this.refs.lines.style.marginBottom = (lastEntryHeight - 22).toString() + 'px';
            }
        }
    }

    render(): React.Element<*> {
        const { children, content, icon } = this.props;

        return (
            <div className={cn('cycle')}>
                <div ref='entries' className={cn('entries')}>
                    {children}
                </div>
                <div className={cn('lines')} ref='lines'>
                    <div className={cn('line-1')} />
                    <div className={cn('line-2')} />
                    <div className={cn('line-3')} />
                </div>
                {icon &&
                    <div className={cn('icon')}>
                        <Icon name={icon} />
                    </div>
                }
                {content &&
                    <div className={cn('info')}>
                        {content}
                    </div>
                }
            </div>
        );
    }
};

