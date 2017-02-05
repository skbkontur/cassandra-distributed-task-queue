//@flow
import type { RemoteTaskQueueSearchRequest } from '../api/RemoteTaskQueueApi';
import moment from 'moment';

export default function buildQueryByFilter(filter: RemoteTaskQueueSearchRequest,
                                           allowedTasks: string[],
                                           from?: number, size?: number): string {
    const resultString = [];
    if (filter.queryString) {
        resultString.push(`query=${filter.queryString}`);
    }
    if (filter.enqueueDateTimeRange.lowerBound) {
        const date = moment(filter.enqueueDateTimeRange.lowerBound)
        .format('YYYY-MM-DD');
        resultString.push(`start=${date}`);
    }
    if (filter.enqueueDateTimeRange.upperBound) {
        const date = moment(filter.enqueueDateTimeRange.upperBound)
        .format('YYYY-MM-DD');
        resultString.push(`end=${date}`);
    }
    if (filter.names && filter.names.length && filter.names.length < allowedTasks.length) {
        resultString.push(createNamesString(filter.names, allowedTasks));
    }
    if (filter.taskState && filter.taskState.length) {
        const states = filter.taskState.join(',');
        resultString.push(`state=${states}`);
    }
    if (from !== undefined && from >= 0) {
        resultString.push(`from=${from}`);
    }
    if (size !== undefined && size > 0) {
        resultString.push(`size=${size}`);
    }

    return resultString.join('&');
}

function createNamesString(names: string[], allowedNames: string[]): string {
    const selectedPart = names.length / allowedNames.length;
    const needInverted = selectedPart > 0.5;
    let resultedNames;

    if (!needInverted) {
        resultedNames = names.join(',');
    }
    else {
        resultedNames = '!' + allowedNames.filter(item => !names.includes(item)).join(',');
    }

    return (`name=${resultedNames}`);
}
