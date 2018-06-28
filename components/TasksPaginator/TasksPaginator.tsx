import * as React from "react";
import { RouterLink } from "ui";
import { LocationDescriptor } from "history";

export type TasksPaginatorProps = {
    nextPageLocation: LocationDescriptor | null;
    prevPageLocation: LocationDescriptor | null;
};

export default class TasksPaginator extends React.Component<TasksPaginatorProps> {
    render(): JSX.Element {
        const { nextPageLocation, prevPageLocation } = this.props;
        return (
            <div>
                {prevPageLocation && (
                    <RouterLink to={prevPageLocation} icon="ArrowBoldLeft" data-tid="PrevLink">
                        Предыдущая
                    </RouterLink>
                )}
                {nextPageLocation && (
                    <RouterLink to={nextPageLocation} icon="ArrowBoldRight" data-tid="NextLink">
                        Следующая
                    </RouterLink>
                )}
            </div>
        );
    }
}
