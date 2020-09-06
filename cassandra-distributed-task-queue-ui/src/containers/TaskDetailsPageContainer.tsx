import Loader from "@skbkontur/react-ui/Loader";
import { LocationDescriptor } from "history";
import React from "react";

import { IRtqMonitoringApi } from "../Domain/Api/RtqMonitoringApi";
import { RtqMonitoringTaskModel } from "../Domain/Api/RtqMonitoringTaskModel";
import { ApiError } from "../Domain/ApiBase/ApiError";
import { takeLastAndRejectPrevious } from "../Domain/Utils/PromiseUtils";
import { ErrorHandlingContainer } from "../components/ErrorHandling/ErrorHandlingContainer";
import { TaskDetailsPage } from "../components/TaskDetailsPage/TaskDetailsPage";
import { TaskNotFoundPage } from "../components/TaskNotFoundPage/TaskNotFoundPage";

interface TaskDetailsPageContainerProps {
    id: string;
    rtqMonitoringApi: IRtqMonitoringApi;
    isSuperUser: boolean;
    parentLocation: Nullable<LocationDescriptor>;
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
    public getTaskDetails = takeLastAndRejectPrevious(
        this.props.rtqMonitoringApi.getTaskDetails.bind(this.props.rtqMonitoringApi)
    );

    public componentWillMount() {
        this.loadData(this.props.id);
    }

    public componentWillReceiveProps(nextProps: TaskDetailsPageContainerProps) {
        if (this.props.id !== nextProps.id) {
            this.loadData(nextProps.id);
        }
    }

    public getTaskLocation(id: string): LocationDescriptor {
        const { parentLocation } = this.props;

        return {
            pathname: `/AdminTools/Tasks/${id}`,
            state: { parentLocation: parentLocation },
        };
    }

    public async loadData(id: string): Promise<void> {
        this.setState({ loading: true, notFoundError: false });
        try {
            try {
                const taskDetails = await this.getTaskDetails(id);
                this.setState({ taskDetails: taskDetails });
            } catch (e) {
                if (e instanceof ApiError) {
                    if (e.statusCode === 404) {
                        this.setState({ notFoundError: true });
                        return;
                    }
                }
                throw e;
            }
        } finally {
            this.setState({ loading: false });
        }
    }

    public async handlerRerun(): Promise<void> {
        const { rtqMonitoringApi, id } = this.props;
        this.setState({ loading: true });
        try {
            await rtqMonitoringApi.rerunTasks([id]);
            const taskDetails = await this.getTaskDetails(id);
            this.setState({ taskDetails: taskDetails });
        } finally {
            this.setState({ loading: false });
        }
    }

    public async handlerCancel(): Promise<void> {
        const { rtqMonitoringApi, id } = this.props;
        this.setState({ loading: true });
        try {
            await rtqMonitoringApi.cancelTasks([id]);
            const taskDetails = await this.getTaskDetails(id);
            this.setState({ taskDetails: taskDetails });
        } finally {
            this.setState({ loading: false });
        }
    }

    public getDefaultParetnLocation(): LocationDescriptor {
        return {
            pathname: "/AdminTools/Tasks",
        };
    }

    public render(): JSX.Element {
        const { taskDetails, loading, notFoundError } = this.state;
        const { parentLocation, isSuperUser } = this.props;

        return (
            <Loader active={loading} type="big" data-tid="Loader">
                {notFoundError && (
                    <TaskNotFoundPage parentLocation={parentLocation || this.getDefaultParetnLocation()} />
                )}
                {taskDetails && (
                    <TaskDetailsPage
                        getTaskLocation={id => this.getTaskLocation(id)}
                        parentLocation={parentLocation || this.getDefaultParetnLocation()}
                        allowRerunOrCancel={isSuperUser}
                        taskDetails={taskDetails}
                        onRerun={() => {
                            this.handlerRerun();
                        }}
                        onCancel={() => {
                            this.handlerCancel();
                        }}
                    />
                )}
                <ErrorHandlingContainer />
            </Loader>
        );
    }
}
