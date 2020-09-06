import React from "react";

import { TimeUtils } from "../src/Domain/Utils/TimeUtils";
import { DateTimeView } from "../src/components/DateTimeView/DateTimeView";

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
