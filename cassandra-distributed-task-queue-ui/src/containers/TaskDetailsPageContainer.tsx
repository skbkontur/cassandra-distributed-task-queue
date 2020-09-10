import Loader from "@skbkontur/react-ui/Loader";
import { LocationDescriptor } from "history";
import React from "react";

import { IRtqMonitoringApi } from "../Domain/Api/RtqMonitoringApi";
import { RtqMonitoringTaskModel } from "../Domain/Api/RtqMonitoringTaskModel";
import { ICustomRenderer } from "../Domain/CustomRenderer";
import { ErrorHandlingContainer } from "../components/ErrorHandling/ErrorHandlingContainer";
import { TaskDetailsPage } from "../components/TaskDetailsPage/TaskDetailsPage";
import { TaskNotFoundPage } from "../components/TaskNotFoundPage/TaskNotFoundPage";

interface TaskDetailsPageContainerProps {
    id: string;
    rtqMonitoringApi: IRtqMonitoringApi;
    customRenderer: ICustomRenderer;
    isSuperUser: boolean;
    path: string;
    parentLocation: Nullable<string>;
    useErrorHandlingContainer: boolean;
}

interface TaskDetailsPageContainerState {
    taskDetails: Nullable<RtqMonitoringTaskModel>;
    loading: boolean;
    notFoundError: boolean;
}

export class TaskDetailsPageContainer extends React.Component<
    TaskDetailsPageContainerProps,
    TaskDetailsPageContainerState
> {
    public state: TaskDetailsPageContainerState = {
        loading: false,
        taskDetails: null,
        notFoundError: false,
    };

    public componentDidMount(): void {
        this.loadData(this.props.id);
    }

    public componentDidUpdate(prevProps: TaskDetailsPageContainerProps): void {
        if (prevProps.id !== this.props.id) {
            this.loadData(this.props.id);
        }
    }

    public render(): JSX.Element {
        const { taskDetails, loading, notFoundError } = this.state;
        const { parentLocation, isSuperUser, customRenderer, path, useErrorHandlingContainer } = this.props;

        return (
            <Loader active={loading} type="big" data-tid="Loader">
                {notFoundError && <TaskNotFoundPage parentLocation={parentLocation || path} />}
                {taskDetails && (
                    <TaskDetailsPage
                        getTaskLocation={id => this.getTaskLocation(id)}
                        parentLocation={parentLocation || path}
                        allowRerunOrCancel={isSuperUser}
                        taskDetails={taskDetails}
                        customRenderer={customRenderer}
                        onRerun={this.handlerRerun}
                        onCancel={this.handlerCancel}
                        path={path}
                    />
                )}
                {useErrorHandlingContainer && <ErrorHandlingContainer />}
            </Loader>
        );
    }

    private getTaskLocation(id: string): LocationDescriptor {
        const { path, parentLocation } = this.props;

        return {
            pathname: `${path}/${id}`,
            state: { parentLocation: parentLocation },
        };
    }

    private async loadData(id: string): Promise<void> {
        const { rtqMonitoringApi } = this.props;

        this.setState({ loading: true, notFoundError: false });
        try {
            const taskDetails = await rtqMonitoringApi.getTaskDetails(id);
            if (taskDetails) {
                this.setState({ taskDetails: taskDetails });
                return;
            }
            this.setState({ notFoundError: true });
        } finally {
            this.setState({ loading: false });
        }
    }

    private readonly handlerRerun = async (): Promise<void> => {
        const { rtqMonitoringApi, id } = this.props;
        this.setState({ loading: true });
        try {
            await rtqMonitoringApi.rerunTasks([id]);
            const taskDetails = await rtqMonitoringApi.getTaskDetails(id);
            this.setState({ taskDetails: taskDetails });
        } finally {
            this.setState({ loading: false });
        }
    };

    private readonly handlerCancel = async (): Promise<void> => {
        const { rtqMonitoringApi, id } = this.props;
        this.setState({ loading: true });
        try {
            await rtqMonitoringApi.cancelTasks([id]);
            const taskDetails = await rtqMonitoringApi.getTaskDetails(id);
            this.setState({ taskDetails: taskDetails });
        } finally {
            this.setState({ loading: false });
        }
    };
}
