// @flow
import { createApiProvider, createWithApiWrapper } from '../../Commons/ApiInjection';
import type { WithApiWrapper } from '../../Commons/ApiInjection';
import type { IRemoteTaskQueueApi } from './RemoteTaskQueueApi';

type ApiProps = { remoteTaskQueueApi: IRemoteTaskQueueApi };

const ApiProvider: ReactClass<ApiProps> = createApiProvider(['remoteTaskQueueApi']);
export { ApiProvider };

const withRemoteTaskQueueApi: WithApiWrapper<ApiProps> = (createWithApiWrapper('remoteTaskQueueApi'): any);
export { withRemoteTaskQueueApi };
