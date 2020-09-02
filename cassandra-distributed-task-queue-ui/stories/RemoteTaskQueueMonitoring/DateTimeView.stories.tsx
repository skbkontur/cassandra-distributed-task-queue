import * as React from "react";

import { DateTimeView } from "Commons/DateTimeView/DateTimeView";
import { TimeUtils } from "Commons/TimeUtils";

export default {
    title: "RemoteTaskQueueMonitoring/DateTimeView",
    decorators: [(story: any) => <div style={{ padding: "30px" }}>{story()}</div>],
};

export const Direct = () => <DateTimeView value={TimeUtils.dateToTicks(new Date())} />;

export const TwoValues = () => (
    <div>
        <div>
            <DateTimeView value={TimeUtils.dateToTicks(new Date())} />
        </div>
        <div>
            <DateTimeView value={TimeUtils.dateToTicks(new Date())} />
        </div>
    </div>
);
