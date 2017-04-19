// @flow
import React from 'react';
import ReactDom from 'react-dom';
import { IndexRoute, Router, Route, browserHistory } from 'react-router';

import TasksPage from './containers/TasksPageContainer';
import TaskDetailsPage from './containers/TaskDetailsPageContainer';
import TaskChainsTreeContainer from './containers/TaskChainsTreeContainer';
import Layout from './components/Layout/Layout';

import 'ui/styles/reset.less';
import 'ui/styles/typography.less';

import { RemoteTaskQueueApi } from '../Domain/EDI/Api/RemoteTaskQueue/RemoteTaskQueue';
//import FakeApi from './api/FakeRemoteTaskQueueApi';
import { ApiProvider } from './api/RemoteTaskQueueApiInjection';
import { ErrorHandlingContainer } from '../Commons/ErrorHandling';

// const api = process.env.API === 'fake'
//     ? new FakeApi()
//     : new RealApi();
const api = new RemoteTaskQueueApi('/internal-api/remote-task-queue/');
const TasksPath = '/AdminTools/Tasks';

ReactDom.render(
    <ApiProvider remoteTaskQueueApi={api}>
        <ErrorHandlingContainer>
            <Router history={browserHistory}>
                <Route path={TasksPath} component={Layout}>
                    <IndexRoute
                        component={({ location }) => (
                            <TasksPage
                                searchQuery={location.search}
                                {...location.state}
                            />
                        )}
                    />
                    <Route
                        path='Tree'
                        component={({ location }) => (
                            <TaskChainsTreeContainer
                                searchQuery={location.search}
                                {...location.state}
                                parentLocation={(location.state && location.state.parentLocation) || null}
                            />
                        )}
                    />
                    <Route
                        path=':id'
                        component={({ location, params }) => (
                            <TaskDetailsPage
                                id={params.id}
                                parentLocation={(location.state && location.state.parentLocation) || null}
                            />
                        )}
                    />
                </Route>
            </Router>
        </ErrorHandlingContainer>
    </ApiProvider>,
    document.getElementById('content'));
