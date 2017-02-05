// @flow
import { connect } from 'react-redux';
import type { Dispatch } from 'react-redux';
import TasksTable from '../components/TaskTable/TaskTable';
import { SuperUserAccessLevels } from '../../Domain/Globals';

function propsSelector(state: *): * {
    const currentUser = state.get('currentUser');
    return {
        allowRerunOrCancel: currentUser &&
            [SuperUserAccessLevels.God, SuperUserAccessLevels.Developer]
                .includes(currentUser.superUserAccessLevel),
        taskInfos: state.get('searchResults') ? state.get('searchResults').toJS() : [],
        currentUrl: state.get('currentUrl'),
    };
}

function actionsSelector(dispatch: Dispatch): * {
    return {
        onRerun: id => dispatch({ type: 'Item.Rerun', id: id }),
        onCancel: id => dispatch({ type: 'Item.Cancel', id: id }),
    };
}

export default connect(propsSelector, actionsSelector)(TasksTable);
