// @flow
import React from 'react';
import { RouterLink } from 'ui';
import type { RouterLocationDescriptor } from '../../../Commons/DataTypes/Routing';

export type TasksPaginatorProps = {
    nextPageLocation: RouterLocationDescriptor | null;
    prevPageLocation: RouterLocationDescriptor | null;
};

export default class TasksPaginator extends React.Component {
    props: TasksPaginatorProps;

    render(): React.Element<*> {
        const { nextPageLocation, prevPageLocation } = this.props;
        return (
            <div>
                {prevPageLocation &&
                    <RouterLink
                        to={prevPageLocation}
                        icon='arrow-left'
                        data-tid='PrevLink'>
                        Предыдущая
                    </RouterLink>
                }
                {nextPageLocation &&
                    <RouterLink
                        to={nextPageLocation}
                        icon='arrow-right'
                        data-tid='NextLink'>
                        Следующая
                    </RouterLink>
                }
            </div>
        );
    }
}
