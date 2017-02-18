// @flow
import React from 'react';
import type { TaskManupulationResultMap } from '../../api/RemoteTaskQueueApi';
import cn from './TaskActionResult.less';

export type TaskActionResultProps = {
    actionResult: TaskManupulationResultMap;
    showTasks: boolean;
};

type TaskActionResultState = {
    showErrors: boolean;
    showSuccess: boolean;
};

export default class TaskActionResult extends React.Component {
    props: TaskActionResultProps;
    state: TaskActionResultState;

    componentWillMount() {
        const { actionResult } = this.props;
        this.chooseShowedBlocks(actionResult);
    }
    componentWillReceiveProps(nextProps: TaskActionResultProps) {
        this.chooseShowedBlocks(nextProps.actionResult);
    }

    render(): React.Element<*> {
        const { showErrors, showSuccess } = this.state;
        const { showTasks } = this.props;
        return (
            <div>
                {showErrors &&
                    <div className={cn('error')} data-tid='Error'>
                        {showTasks
                            ? <div data-tid='ErrorText'>Ошибки: {this.filteredResults(false)}</div>
                            : <div data-tid='ErrorText'>Не вышло!</div>
                        }
                    </div>
                }
                {showSuccess &&
                    <div className={cn('success')} data-tid='Success'>
                        {showTasks
                            ? <div data-tid='SuccessText'>Успешно: {this.filteredResults(true)}</div>
                            : <div data-tid='SuccessText'>Успех!</div>
                        }
                    </div>
                }
            </div>
        );
    }

    chooseShowedBlocks(actionResult: any) {
        const resultValues = Object.values(actionResult);
        this.state = {
            showErrors: Boolean(resultValues.filter(item => item !== 'Success').length),
            showSuccess: Boolean(resultValues.filter(item => item === 'Success').length),
        };
    }

    filteredResults(success: boolean): React.Element<*>[] {
        const { actionResult } = this.props;
        const oneTypeArray = [];

        for (const key in actionResult) {
            if (!actionResult.hasOwnProperty(key)) {
                continue;
            }
            if ((success && actionResult[key] === 'Success') || (!success && actionResult[key] !== 'Success')) {
                oneTypeArray.push(key);
            }
        }

        return oneTypeArray.map(item => {
            return <p key={item}>{item}</p>;
        });
    }
}
