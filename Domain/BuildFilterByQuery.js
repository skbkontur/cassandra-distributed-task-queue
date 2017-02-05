//@flow
import type { RemoteTaskQueueSearchRequest } from '../api/RemoteTaskQueueApi';
import { TaskStates } from './TaskState';
import type { TaskState } from './TaskState';

export type QueryStringType = {
    query?: string;
    start?: string;
    end?: string;
    name?: string;
    state?: string;
    from?: string;
    size?: string;
};

export type BuildFilterByQueryResult = {
    filter: RemoteTaskQueueSearchRequest;
    size: number;
    from: number;
};

export default function buildFilterByQuery(query: QueryStringType,
                                           allowedTasks: string[]): BuildFilterByQueryResult | null {
    if (!query || !Object.keys(query).length) {
        return null;
    }
    return {
        filter: {
            enqueueDateTimeRange: {
                lowerBound: query.start ? new Date(query.start) : null,
                upperBound: query.end ? new Date(query.end) : null,
            },
            queryString: query.query || '',
            taskState: query.state ? createTaskStateArray(query.state) : [],
            names: query.name ? createNamesArray(query.name, allowedTasks) : [],
        },
        size: query.size && !isNaN(query.size) ? Number(query.size) : 20,
        from: query.from && !isNaN(query.from) ? Number(query.from) : 0,
    };
}

function createTaskStateArray(states: string): TaskState[] {
    const taskState = states.split(',');

    return taskState.map(item => {
        return TaskStates[item];
    });
}

function createNamesArray(names: string, allowedNames: string[]): string[] {
    if (names.charAt(0) !== '!') {
        return names.split(',');
    }

    const excludedNames = names.slice(1).split(',');
    return allowedNames.filter(item => !excludedNames.includes(item));
}
