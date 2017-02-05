// @flow
import { connect } from 'react-redux';
import type { Dispatch } from 'react-redux';
import TasksPage from '../components/TasksPage/TasksPage';
import { SuperUserAccessLevels } from '../../Domain/Globals';

function propsSelector(state: *): * {
    const currentUser = state.get('currentUser');
    return {
        allowRerunOrCancel: currentUser &&
            [SuperUserAccessLevels.God, SuperUserAccessLevels.Developer]
                .includes(currentUser.superUserAccessLevel),
        counter: state.get('totalCount') ? state.get('totalCount') : 0,
        loading: state.get('loadingSearchResult') || state.get('loadingTaskNames') || state.get('isActionOnTask'),
        actionsOnTaskResult: state.get('actionOnTaskResult'),
        error: state.get('failSearch') || state.get('failActionOnTasks'),
    };
}

function actionsSelector(dispatch: Dispatch): * {
    return {
        onRerunAll: () => dispatch({ type: 'Items.Rerun' }),
        onCancelAll: () => dispatch({ type: 'Items.Cancel' }),
    };
}
export default connect(propsSelector, actionsSelector)(TasksPage);
