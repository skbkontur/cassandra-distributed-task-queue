// @flow
import { createApiProvider, createWithApiWrapper } from "../../Commons/ApiInjection";
import type { ApiProviderBase } from "../../Commons/ApiInjection";
import type { IRemoteTaskQueueApi } from "./RemoteTaskQueueApi";

type ApiProps = { remoteTaskQueueApi: IRemoteTaskQueueApi };

const ApiProvider: ApiProviderBase<ApiProps> = createApiProvider(["remoteTaskQueueApi"]);
export { ApiProvider };

const withRemoteTaskQueueApi = createWithApiWrapper("remoteTaskQueueApi", (null: ?ApiProps));
export { withRemoteTaskQueueApi };
