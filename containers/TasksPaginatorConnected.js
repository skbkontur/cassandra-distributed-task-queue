// @flow
import { connect } from 'react-redux';
import type { Dispatch } from 'react-redux';
import TasksPaginator from '../components/TasksPaginator/TasksPaginator';

function propsSelector(state: *): * {
    const hasPages = state.get('totalCount') > state.get('size');
    const isNotLastPage = state.get('size') + state.get('from') < state.get('totalCount');

    return {
        hasNextLink: hasPages && isNotLastPage,
        hasPrevLink: hasPages && state.get('from') > 0,
    };
}

function actionsSelector(dispatch: Dispatch): * {
    return {
        onNextPage: () => dispatch({ type: 'Pages.Next' }),
        onPrevPage: () => dispatch({ type: 'Pages.Prev' }),
    };
}
export default connect(propsSelector, actionsSelector)(TasksPaginator);
