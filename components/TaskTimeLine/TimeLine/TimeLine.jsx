// @flow
import React from 'react';
import { Icon } from 'ui';
import TimeLineCycled from './TimeLineCycled';
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

TimeLine.Branch = function TimeLine({ children }: TimeLineProps): React.Element<*> {
    return (
        <div className={cn('root', 'branch')}>
            <div className={cn('line-up')} />
            {children}
        </div>
    );
};

TimeLine.BranchNode = class TimeLineBranchNode extends React.Component {
    props: TimeLineProps;

    componentDidUpdate() {
        this.updateLinesHeight();
    }

    componentDidMount() {
        this.updateLinesHeight();
    }

    updateLinesHeight() {
        if (this.refs.branches) {
            const branches = this.refs.branches.children;
            const lastEntry = branches[branches.length - 1];
            const lastEntryWidth = lastEntry.clientWidth;
            if (!isNaN(lastEntryWidth)) {
                this.refs.line.style.marginRight = (lastEntryWidth - 7).toString() + 'px';
            }
        }
    }

    render(): React.Element<*> {
        const { children } = this.props;
        return (
            <div className={cn('branch-node')}>
                <div className={cn('hor-line')} ref='line' />
                <div className={cn('branch-nodes')} ref='branches'>
                    {children}
                </div>
            </div>
        );
    }
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

TimeLine.Cycled = TimeLineCycled;
