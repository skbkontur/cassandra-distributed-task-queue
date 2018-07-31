import { createApiProvider, createWithApiWrapper } from "../../Commons/ApiInjection";
import { ApiProviderBase } from "../../Commons/ApiInjection";

import { IRemoteTaskQueueApi } from "./RemoteTaskQueueApi";

interface ApiProps {
    remoteTaskQueueApi: IRemoteTaskQueueApi;
}

const ApiProvider: ApiProviderBase<ApiProps> = createApiProvider(["remoteTaskQueueApi"]);
export { ApiProvider };

const withRemoteTaskQueueApi = createWithApiWrapper<ApiProps>("remoteTaskQueueApi");
export { withRemoteTaskQueueApi };
