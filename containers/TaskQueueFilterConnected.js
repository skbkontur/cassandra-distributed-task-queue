// @flow
import { connect } from 'react-redux';

import type { Dispatch } from 'react-redux';

import TaskQueueFilter from '../components/TaskQueueFilter/TaskQueueFilter';

function propsSelector(state: *): * {
    return {
        value: state.get('filter').toJS(),
        availableTaskTypes: state.get('taskNames'),
    };
}

function actionsSelector(dispatch: Dispatch): * {
    return {
        onChange: update => dispatch({ type: 'Filter.Change', filter: update }),
        onSearchButtonClick: () => dispatch({ type: 'StartSearch' }),
    };
}

export default connect(propsSelector, actionsSelector)(TaskQueueFilter);
