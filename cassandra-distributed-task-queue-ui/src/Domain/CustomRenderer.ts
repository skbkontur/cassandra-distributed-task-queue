import { RtqMonitoringSearchRequest } from "./Api/RtqMonitoringSearchRequest";
import { RtqMonitoringTaskModel } from "./Api/RtqMonitoringTaskModel";

export interface ICustomRenderer {
    renderDetails: (target: any, path: string[]) => null | JSX.Element;
    getRelatedTasksLocation: (taskDetails: RtqMonitoringTaskModel) => Nullable<RtqMonitoringSearchRequest>;
}

export class NullCustomRenderer implements ICustomRenderer {
    public getRelatedTasksLocation(): Nullable<RtqMonitoringSearchRequest> {
        return null;
    }

    public renderDetails(): null | JSX.Element {
        return null;
    }
}
