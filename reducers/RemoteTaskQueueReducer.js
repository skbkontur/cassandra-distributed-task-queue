//@flow-ignore
import { Map } from 'immutable';
import { defineReducer, perform } from 'reelm/fluent';
import { select, put, call } from 'reelm/effects';
import moment from 'moment';
import { browserHistory } from 'react-router';
import type { IRemoteTaskQueueApi } from '../api/RemoteTaskQueueApi';

import buildQueryByFilter from '../Domain/BuildQueryByFilter';
import buildFilterByQuery from '../Domain/BuildFilterByQuery';

export const TasksPath = '/AdminTools/Tasks';

const initialState = Map({
    loadingSearchResult: false,
    loadingTaskNames: false,
    filter: Map({
        enqueueDateTimeRange: Map({
            lowerBound: moment()
                .hour(0)
                .minute(0)
                .toDate(),
            upperBound: moment()
                .hour(23)
                .minute(59)
                .toDate(),
        }),
        queryString: '',
        names: [],
        taskState: [],
    }),
    taskNames: [],
    searchResult: null,
    taskDetailsInfos: Map(),
    updatingTaskDetails: false,
    totalCount: 0,
    from: 0,
    size: 20,
    failGetDetails: null,
    failSearch: null,
    failActionOnTasks: null,
    isActionOnTask: false,
    actionOnTaskResult: null,
    currentUrl: '',
});

export default (api: IRemoteTaskQueueApi) => defineReducer(initialState)
    .on('Authenticate', (state, { user }) => state.set('currentUser', user))
    .on('RequestGetTaskDetails', perform(function* ({ id }: any): any {
        yield put({ type: 'BeginGetTaskDetails' });
        try {
            const result = yield call(() => api.getTaskDetails(id));
            yield put({ type: 'EndGetTaskDetails', taskDetails: result });
        }
        catch (ex) {
            yield put({ type: 'GetDetailsFailed', result: ex.toString() });
        }
    }))
    .on('BeginGetTaskDetails', state => state.merge({ updatingTaskDetails: true }))
    .on('EndGetTaskDetails', (state, { taskDetails }) => state
        .merge({ updatingTaskDetails: false, failGetDetails: null })
        .mergeIn(['taskDetailsInfos'], {
            [taskDetails.taskMeta.id]: taskDetails,
        }))
    .on('GetDetailsFailed', (state, { result }) => state.merge({ updatingTaskDetails: false, failGetDetails: result }))


    .on('Filter.Change', (state, { filter }) => state.mergeIn(['filter'], filter))
    .on('PageParams.Change', (state, { from, size }) => state.merge({ from: from, size: size }))
    .on('ResetFromParam', state => state.merge({ from: 0 }))
    .on('StartSearch', perform(function* (): any {
        const size = yield select(x => x.get('size'));
        yield put({ type: 'PageParams.Change', from: 0, size: size });
        yield put({ type: 'EndActionOnTask', result: [] });
        yield put({ type: 'UrlUpdate' });
    }))
    .on('UrlUpdate', perform(function* (): any {
        const filter = yield select(x => x.get('filter').toJS());
        const from = yield select(x => x.get('from'));
        const size = yield select(x => x.get('size'));
        const allowedTasks = yield select(x => x.get('taskNames').toJS());
        const url = `${TasksPath}?${buildQueryByFilter(filter, allowedTasks, from, size)}`;
        yield call(async () => browserHistory.push(url));
        yield put({ type: 'RequestSearch' });
        yield put({ type: 'UpdateCurrentUrlInState', url: url });
    }))
    .on('UpdateCurrentUrlInState', (state, { url }) => state.merge({ currentUrl: url }))
    .on('RequestSearch', perform(function* (): any {
        const filter = yield select(x => x.get('filter'));
        const from = yield select(x => x.get('from'));
        const size = yield select(x => x.get('size'));
        yield put({ type: 'BeginSearch' });
        try {
            const result = yield call(() => api.search(filter.toJS(), from, size));
            yield put({ type: 'CompleteSearch', result: result });
        }
        catch (ex) {
            yield put({ type: 'SearchFailed', result: ex.toString() });
        }
    }))
    .on('BeginSearch', state => state.merge({ loadingSearchResult: true }))
    .on('CompleteSearch', (state, { result }) =>
        state.merge({
            totalCount: result.totalCount,
            searchResults: result.taskMetas,
            loadingSearchResult: false,
            failSearch: null,
        }))
    .on('SearchFailed', (state, { result }) => state.merge({ loadingSearchResult: false, failSearch: result }))


    .on('AdminTools.Tasks.Enter', perform(function* (): any {
        yield put({ type: 'BeginAvailableTaskNamesUpdated' });
        try {
            const result = yield call(() => api.getAllTasksNames());
            yield put({ type: 'AvailableTaskNamesUpdated', taskNames: result });
            yield put({ type: 'EndAvailableTaskNamesUpdated' });
        }
        catch (ex) {
            yield put({ type: 'SearchFailed', result: ex.toString() });
            yield put({ type: 'EndAvailableTaskNamesUpdated' });
        }
    }))
    .on('BeginAvailableTaskNamesUpdated', state => state.merge({ loadingTaskNames: true }))
    .on('AvailableTaskNamesUpdated', (state, { taskNames }) => state.merge({ taskNames: taskNames }))
    .on('EndAvailableTaskNamesUpdated', state => state.merge({ loadingTaskNames: false }))
    .on('BuildFilterByQuery', perform(function* ({ location }: any): any {
        const allowedTasks = yield select(x => x.get('taskNames').toJS());
        yield put({ type: 'UpdateCurrentUrlInState', url: location.pathname + location.search });
        const result = buildFilterByQuery(location.query, allowedTasks);
        if (result !== null) {
            yield put({ type: 'Filter.Change', filter: result.filter });
            yield put({ type: 'PageParams.Change', from: result.from, size: result.size });
            yield put({ type: 'RequestSearch' });
        }
    }))


    .on('Item.Rerun', perform(function* ({ id }: any): any {
        yield put({ type: 'BeginActionOnTask' });
        try {
            const result = yield call(() => api.rerunTasks([id]));
            yield put({ type: 'EndActionOnTask', result: result });
        }
        catch (ex) {
            yield put({ type: 'RerunFailed', result: ex.toString() });
        }
    }))
    .on('Item.Cancel', perform(function* ({ id }: any): any {
        yield put({ type: 'BeginActionOnTask' });
        try {
            const result = yield call(() => api.cancelTasks([id]));
            yield put({ type: 'EndActionOnTask', result: result });
        }
        catch (ex) {
            yield put({ type: 'RerunFailed', result: ex.toString() });
        }
    }))
    .on('Items.Rerun', perform(function* (): any {
        yield put({ type: 'BeginActionOnTask' });
        const filter = yield select(x => x.get('filter').toJS());
        try {
            const result = yield call(() => api.rerunTasksByRequest(filter));
            yield put({ type: 'EndActionOnTask', result: result });
        }
        catch (ex) {
            yield put({ type: 'RerunFailed', result: ex.toString() });
        }
    }))
    .on('Items.Cancel', perform(function* (): any {
        yield put({ type: 'BeginActionOnTask' });
        const filter = yield select(x => x.get('filter').toJS());
        try {
            const result = yield call(() => api.cancelTasksByRequest(filter));
            yield put({ type: 'EndActionOnTask', result: result });
        }
        catch (ex) {
            yield put({ type: 'RerunFailed', result: ex.toString() });
        }
    }))
    .on('BeginActionOnTask', state => state.merge({ isActionOnTask: true }))
    .on('EndActionOnTask', (state, { result }) =>
        state.merge({ isActionOnTask: false })
             .set('actionOnTaskResult', result)
    )
    .on('RerunFailed', (state, { result }) => state.merge({ isActionOnTask: false, failActionOnTasks: result }))


    .on('Pages.Next', perform(function* (): any {
        const from = yield select(x => x.get('from'));
        const size = yield select(x => x.get('size'));

        yield put({ type: 'PageParams.Change', from: from + size, size: size });
        yield put({ type: 'UrlUpdate' });
    }))
    .on('Pages.Prev', perform(function* (): any {
        const from = yield select(x => x.get('from'));
        const size = yield select(x => x.get('size'));
        yield put({ type: 'PageParams.Change', from: (from - size) < 0 ? 0 : (from - size), size: size });
        yield put({ type: 'UrlUpdate' });
    }));
