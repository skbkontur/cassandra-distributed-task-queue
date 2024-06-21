import { TimeUtils, Timestamp } from "@skbkontur/edi-ui";

export default {
    title: "RemoteTaskQueueMonitoring/DateTimeView",
    decorators: [(story: any) => <div style={{ padding: "30px" }}>{story()}</div>],
};

export const Direct = () => <Timestamp value={TimeUtils.dateToTicks(new Date())} />;

export const TwoValues = () => (
    <div>
        <div>
            <Timestamp value={TimeUtils.dateToTicks(new Date())} />
        </div>
        <div>
            <Timestamp value={TimeUtils.dateToTicks(new Date())} />
        </div>
    </div>
);
