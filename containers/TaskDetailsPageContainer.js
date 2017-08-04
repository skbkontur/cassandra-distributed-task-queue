// @flow
import React from "react";
import $c from "property-chain";
import DelayedLoader from "../../Commons/DelayedLoader/DelayedLoader";
import TaskDetailsPage from "../components/TaskDetailsPage/TaskDetailsPage";
import TaskNotFoundPage from "../components/TaskNotFoundPage/TaskNotFoundPage";
import { SuperUserAccessLevels } from "../../Domain/Globals";
import type { RemoteTaskInfoModel, IRemoteTaskQueueApi } from "../api/RemoteTaskQueueApi";
import { withRemoteTaskQueueApi } from "../api/RemoteTaskQueueApiInjection";
import { takeLastAndRejectPrevious } from "PromiseUtils";
import { getCurrentUserInfo } from "../../Domain/Globals";
import type { RouterLocationDescriptor } from "../../Commons/DataTypes/Routing";
import { ApiError } from "Domain/ApiBase/ApiBase";

type TaskDetailsPageContainerProps = {
    id: string,
    remoteTaskQueueApi: IRemoteTaskQueueApi,
    parentLocation: ?RouterLocationDescriptor,
};

type TaskDetailsPageContainerState = {
    taskDetails: ?RemoteTaskInfoModel,
    loading: boolean,
    notFoundError: boolean,
};

class TaskDetailsPageContainer extends React.Component {
    props: TaskDetailsPageContainerProps;
    state: TaskDetailsPageContainerState = {
        loading: false,
        taskDetails: null,
        notFoundError: false,
    };
    getTaskDetails = takeLastAndRejectPrevious(
        this.props.remoteTaskQueueApi.getTaskDetails.bind(this.props.remoteTaskQueueApi)
    );

    componentWillMount() {
        this.loadData(this.props.id);
    }

    componentWillReceiveProps(nextProps: TaskDetailsPageContainerProps) {
        if (this.props.id !== nextProps.id) {
            this.loadData(nextProps.id);
        }
    }

    getTaskLocation(id: string): RouterLocationDescriptor {
        const { parentLocation } = this.props;

        return {
            pathname: `/AdminTools/Tasks/${id}`,
            state: { parentLocation: parentLocation },
        };
    }

    async loadData(id: string): Promise<void> {
        this.setState({ loading: true, notFoundError: false });
        try {
            try {
                const taskDetails = await this.getTaskDetails(id);
                this.setState({ taskDetails: taskDetails });
                // @flow-coverage-ignore-next-line
            } catch (e) {
                // @flow-coverage-ignore-next-line
                const error: mixed = e;
                if (typeof error === "object" && error instanceof ApiError) {
                    // @flow-coverage-ignore-next-line
                    if (error.statusCode === 404) {
                        this.setState({ notFoundError: true });
                        return;
                    }
                }
                throw error;
            }
        } finally {
            this.setState({ loading: false });
        }
    }

    async handlerRerun(): Promise<void> {
        const { remoteTaskQueueApi, id } = this.props;
        this.setState({ loading: true });
        try {
            await remoteTaskQueueApi.rerunTasks([id]);
            const taskDetails = await this.getTaskDetails(id);
            this.setState({ taskDetails: taskDetails });
        } finally {
            this.setState({ loading: false });
        }
    }

    async handlerCancel(): Promise<void> {
        const { remoteTaskQueueApi, id } = this.props;
        this.setState({ loading: true });
        try {
            await remoteTaskQueueApi.cancelTasks([id]);
            const taskDetails = await this.getTaskDetails(id);
            this.setState({ taskDetails: taskDetails });
        } finally {
            this.setState({ loading: false });
        }
    }

    getDefaultParetnLocation(): RouterLocationDescriptor {
        return {
            pathname: "/AdminTools/Tasks",
        };
    }

    render(): React.Element<*> {
        const { taskDetails, loading, notFoundError } = this.state;
        const { parentLocation } = this.props;
        const currentUser = getCurrentUserInfo();

        return (
            <DelayedLoader active={loading} type="big" simulateHeightToEnablePageScroll>
                {notFoundError &&
                    <TaskNotFoundPage parentLocation={parentLocation || this.getDefaultParetnLocation()} />}
                {taskDetails &&
                    <TaskDetailsPage
                        getTaskLocation={id => this.getTaskLocation(id)}
                        parentLocation={parentLocation || this.getDefaultParetnLocation()}
                        allowRerunOrCancel={$c(currentUser)
                            .with(x => x.superUserAccessLevel)
                            .with(x => [SuperUserAccessLevels.God, SuperUserAccessLevels.Developer].includes(x))
                            .return(false)}
                        taskDetails={taskDetails}
                        onRerun={() => {
                            this.handlerRerun();
                        }}
                        onCancel={() => {
                            this.handlerCancel();
                        }}
                    />}
            </DelayedLoader>
        );
    }
}

export default withRemoteTaskQueueApi(TaskDetailsPageContainer);
