import { LocationDescriptor } from "history";
import * as React from "react";
import { RouterLink } from "ui/components";

import ArrowBoldLeftIcon from "@skbkontur/react-icons/ArrowBoldLeft";
import ArrowBoldRightIcon from "@skbkontur/react-icons/ArrowBoldRight";

export interface TasksPaginatorProps {
    nextPageLocation: LocationDescriptor | null;
    prevPageLocation: LocationDescriptor | null;
}

export class TasksPaginator extends React.Component<TasksPaginatorProps> {
    public render(): JSX.Element {
        const { nextPageLocation, prevPageLocation } = this.props;
        return (
            <div>
                {prevPageLocation && (
                    <RouterLink to={prevPageLocation} icon={<ArrowBoldLeftIcon />} data-tid="PrevLink">
                        Предыдущая
                    </RouterLink>
                )}
                {nextPageLocation && (
                    <RouterLink to={nextPageLocation} icon={<ArrowBoldRightIcon />} data-tid="NextLink">
                        Следующая
                    </RouterLink>
                )}
            </div>
        );
    }
}
