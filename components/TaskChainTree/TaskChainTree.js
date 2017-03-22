// @flow
import React from 'react';
import { RouterLink } from 'ui';
import { ColumnStack } from 'ui/layout';
import type { RouterLocationDescriptor } from '../../../Commons/DataTypes/Routing';
import TimeLine from '../TaskTimeLine/TimeLine/TimeLine';
import type { TaskMetaInformationModel } from '../../api/RemoteTaskQueueApi';
import { TaskStates } from '../../Domain/TaskState';
import _ from 'lodash';
import AllowCopyToClipboard from '../../../Commons/AllowCopyToClipboard';
import cn from './TaskChainTree.less';

const IconColors = {
    red: '#d43517',
    green: '#3F9726',
    grey: '#a0a0a0',
    warning: '#ff9900',
};

type TaskChainTreeProps = {
    taskMetas: TaskMetaInformationModel[];
    getTaskLocation: (id: string) => RouterLocationDescriptor;
};

export default class TaskChainTree extends React.Component {
    props: TaskChainTreeProps;

    buildTaskTimeLineEntry(taskMeta: TaskMetaInformationModel): any {
        const { getTaskLocation } = this.props;

        let iconAndColorProps = {
            icon: 'ok',
            iconColor: undefined,
        };
        switch (taskMeta.state) {
            case TaskStates.Unknown:
                iconAndColorProps = {
                    icon: 'help-o',
                    iconColor: IconColors.warning,
                };
                break;
            case TaskStates.New:
                iconAndColorProps = {
                    icon: 'wait',
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.WaitingForRerun:
                iconAndColorProps = {
                    icon: 'wait',
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.WaitingForRerunAfterError:
                iconAndColorProps = {
                    icon: 'wait',
                    iconColor: IconColors.red,
                };
                break;
            case TaskStates.Finished:
                iconAndColorProps = {
                    icon: 'ok',
                    iconColor: IconColors.green,
                };
                break;
            case TaskStates.Inprocess:
                iconAndColorProps = {
                    icon: 'wait',
                    iconColor: IconColors.grey,
                };
                break;
            case TaskStates.Fatal:
                iconAndColorProps = {
                    icon: 'clear',
                    iconColor: IconColors.red,
                };
                break;
            case TaskStates.Canceled:
                iconAndColorProps = {
                    icon: 'remove',
                    iconColor: IconColors.red,
                };
                break;
            default:
                break;
        }
        return (
            <TimeLine.Entry {...iconAndColorProps} key={taskMeta.id}>
                <div className={cn('task-name')}>
                    <RouterLink
                        to={getTaskLocation(taskMeta.id)}>
                        {taskMeta.name}
                    </RouterLink>
                </div>
                <div className={cn('task-id')}>
                    <AllowCopyToClipboard>
                        {taskMeta.id}
                    </AllowCopyToClipboard>
                </div>
            </TimeLine.Entry>
        );
    }

    buildChildEntries(
        taskMeta: TaskMetaInformationModel,
        taskMetaHashSet: { [key: string]: TaskMetaInformationModel }
    ): any {
        if (!taskMeta.childTaskIds || taskMeta.childTaskIds.length === 0) {
            return [];
        }
        if (taskMeta.childTaskIds.length === 1) {
            return this.buildTaskTimeLine(taskMetaHashSet[taskMeta.childTaskIds[0]], taskMetaHashSet);
        }
        return [
            <TimeLine.BranchNode key={`${taskMeta.id}-branches`}>
                {taskMeta.childTaskIds
                    .map(x => taskMetaHashSet[x])
                    .filter(x => x)
                    .map((x, i) => (
                        <TimeLine.Branch key={i}>
                            {this.buildTaskTimeLine(x, taskMetaHashSet)}
                        </TimeLine.Branch>
                    ))}
            </TimeLine.BranchNode>,
        ];
    }

    buildTaskTimeLine(
        taskMeta: TaskMetaInformationModel,
        taskMetaHashSet: { [key: string]: TaskMetaInformationModel }
    ): any[] {
        return [
            this.buildTaskTimeLineEntry(taskMeta),
            ...this.buildChildEntries(taskMeta, taskMetaHashSet),
        ];
    }

    findMostParentTask(
        taskMetaHashSet: { [key: string]: TaskMetaInformationModel },
        startTaskMeta: TaskMetaInformationModel
    ): TaskMetaInformationModel {
        let result = startTaskMeta;
        while (result.parentTaskId) {
            if (!taskMetaHashSet[result.parentTaskId]) {
                return result;
            }
            result = taskMetaHashSet[result.parentTaskId];
        }
        return result;
    }

    findAllMostParents(taskMetaHashSet: { [key: string]: TaskMetaInformationModel }): [TaskMetaInformationModel] {
        return _.chain(taskMetaHashSet)
            .values()
            .map(x => this.findMostParentTask(taskMetaHashSet, x))
            .uniq()
            .sortBy(x => x.enqueueTime)
            .reverse()
            .value();
    }

    render(): React.Element<*> {
        const { taskMetas } = this.props;
        const taskMetaHashSet = taskMetas.reduce((result, taskMeta) => {
            result[taskMeta.id] = taskMeta;
            return result;
        }, {});

        const mostParentTasks = this.findAllMostParents(taskMetaHashSet);

        return (
            <ColumnStack block stretch gap={8}>
                {mostParentTasks.map((x, i) => (
                    <ColumnStack.Fit key={i}>
                        <TimeLine>
                            {this.buildTaskTimeLine(x, taskMetaHashSet)}
                        </TimeLine>
                    </ColumnStack.Fit>
                ))}
            </ColumnStack>
        );
    }
}
