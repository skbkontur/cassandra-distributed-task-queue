// @flow
import { connect } from 'react-redux';
import type { Dispatch } from 'react-redux';
import TaskDetailsPage from '../components/TaskDetailsPage/TaskDetailsPage';
import { SuperUserAccessLevels } from '../../Domain/Globals';

function propsSelector(state: *, { id }: any): * {
    const currentUser = state.get('currentUser');
    return {
        allowRerunOrCancel: currentUser &&
            [SuperUserAccessLevels.God, SuperUserAccessLevels.Developer]
                .includes(currentUser.superUserAccessLevel),
        taskDetails: state.getIn(['taskDetailsInfos', id]) && state.getIn(['taskDetailsInfos', id]).toJS(),
        loading: state.get('updatingTaskDetails') || state.get('isActionOnTask'),
        actionsOnTaskResult: state.get('actionOnTaskResult'),
        error: state.get('failGetDetails') || state.get('failActionOnTasks'),
    };
}

function actionsSelector(dispatch: Dispatch): * {
    return {
        onRerun: id => dispatch({ type: 'Item.Rerun', id: id }),
        onCancel: id => dispatch({ type: 'Item.Cancel', id: id }),
    };
}

export default connect(propsSelector, actionsSelector)(TaskDetailsPage);
