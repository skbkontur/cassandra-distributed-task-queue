// @flow
import React from 'react';
import {
    Icon,
} from 'ui';
import cn from './TasksPaginator.less';

export type TasksPaginatorProps = {
    hasNextLink: boolean;
    hasPrevLink: boolean;
    onPrevPage: () => any;
    onNextPage: () => any;
};

export default class TasksPaginator extends React.Component {
    props: TasksPaginatorProps;

    render(): React.Element<*> {
        const { hasNextLink, hasPrevLink, onNextPage, onPrevPage } = this.props;
        return (
            <div>
                { hasPrevLink &&
                    <button
                        data-tid='PrevLink'
                        className={cn('button')}
                        onClick={onPrevPage}>
                        <Icon name='arrow-left' /> Предыдущая
                    </button>
                }
                { hasNextLink &&
                    <button
                        data-tid='NextLink'
                        className={cn('button')}
                        onClick={onNextPage}>
                        Следующая <Icon name='arrow-right' />
                    </button>
                }
            </div>
        );
    }
}
