import ArrowBoldLeftIcon from "@skbkontur/react-icons/ArrowBoldLeft";
import ArrowBoldRightIcon from "@skbkontur/react-icons/ArrowBoldRight";
import Link from "@skbkontur/react-ui/Link";
import { LocationDescriptor } from "history";
import React from "react";

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
                    <Link to={prevPageLocation} icon={<ArrowBoldLeftIcon />} data-tid="PrevLink">
                        Предыдущая
                    </Link>
                )}
                {nextPageLocation && (
                    <Link to={nextPageLocation} icon={<ArrowBoldRightIcon />} data-tid="NextLink">
                        Следующая
                    </Link>
                )}
            </div>
        );
    }
}
