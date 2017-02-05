// @flow
import React from 'react';
import {
    Icon,
} from 'ui';
import cn from './TaskAccordion.less';

export type TaskAccordionProps = {
    customRender?: ?((target: any, path: string[]) => React.Element<*> | null);
    value: any;
    title: string;
    pathPrefix: string[];
};

type TaskAccordionState = {
    collapsed: boolean;
};

export default class TaskAccordion extends React.Component {
    props: TaskAccordionProps;
    state: TaskAccordionState;
    static defaultProps = {
        pathPrefix: [],
    };

    componentWillMount() {
        this.state = {
            collapsed: false,
        };
    }

    render(): React.Element<*> {
        const { value, title } = this.props;
        const { collapsed } = this.state;

        return (
            <div className={cn('value-wrapper')}>
                {title &&
                    <button
                        tid='ToggleButton'
                        className={cn('toggle-button')}
                        onClick={() => this.setState({ collapsed: !collapsed })}>
                        <Icon name={collapsed ? 'caret-right' : 'caret-bottom'} />
                        <span
                            tid='ToggleButtonText'
                            className={cn('toggle-button-text')}>{title}</span>
                    </button>}
                {value && !collapsed && this.renderValue()}
            </div>
        );
    }

    renderValue(): React.Element<any>[] {
        const { value, customRender, pathPrefix } = this.props;
        const keys = Object.keys(value);

        return keys.map(key => {
            const isObjectValue = isObject(value[key]);
            if (isObjectValue) {
                const newCustomRender = customRender ? (target, path) => customRender(value, [key, ...path]) : null;
                return (
                    <TaskAccordion
                        tid='InnerAccordion'
                        customRender={newCustomRender}
                        key={key}
                        value={value[key]}
                        title={key}
                        pathPrefix={[...pathPrefix, key]}
                    />
                );
            }
            return (
                <div key={key} className={cn('string-wrapper')} data-tid={[...pathPrefix, key].join('_')}>
                    <span
                        tid='Key'
                        className={cn('title')}>{key}: </span>
                    <span tid='Value' data-tid='Value'>
                        {(customRender && customRender(value, [key])) ||
                            (Array.isArray(value[key]) ? value[key].join(', ') : String(value[key]))
                        }
                    </span>
                </div>
            );
        });
    }
}

function isObject(maybeObj: any): boolean {
    return (typeof maybeObj === 'object') && (maybeObj !== null) && !Array.isArray(maybeObj);
}
