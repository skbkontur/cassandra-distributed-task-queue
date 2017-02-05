// @flow
import React from 'react';
import ReactDom from 'react-dom';
import { Provider } from 'react-redux';
import { reelmRunner } from 'reelm';
import { compose, createStore } from 'redux';
import { IndexRoute, Router, Route, browserHistory } from 'react-router';

import TasksPage from './containers/TasksPageConnected';
import TaskDetailsPage from './containers/TaskDetailsPageConnected';

import 'ui/styles/reset.less';
import 'ui/styles/typography.less';

import remoteTaskQueueReducer from './reducers/RemoteTaskQueueReducer';

import { TasksPath } from './reducers/RemoteTaskQueueReducer';
import RealApi from './api/RemoteTaskQueueApi';
import FakeApi from './api/FakeRemoteTaskQueueApi';

import { getCurrentUserInfo } from '../Domain/Globals';

const api = process.env.API === 'fake'
    ? new FakeApi()
    : new RealApi();

const store = createStore(
    remoteTaskQueueReducer(api),
    compose(
        reelmRunner(),
        window.devToolsExtension ? window.devToolsExtension() : f => f
    ));

ReactDom.render(
    <Provider store={store}>
        <Router history={browserHistory}>
            <Route path={TasksPath} onEnter={() => {
                store.dispatch({ type: 'Authenticate', user: getCurrentUserInfo() });
            }}>
                <IndexRoute
                    component={TasksPage}
                    onEnter={async props => {
                        await store.dispatch({ type: 'AdminTools.Tasks.Enter' });
                        await store.dispatch({ type: 'BuildFilterByQuery', location: props.location });
                    }}
                />
                <Route
                    path=':id'
                    component={({ params }) => <TaskDetailsPage id={params.id} />}
                    onEnter={({ params }) => store.dispatch({ type: 'RequestGetTaskDetails', id: params.id })}
                />
            </Route>
        </Router>
    </Provider>,
    document.getElementById('content'));
