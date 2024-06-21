import Decimal from "decimal.js";

export const ticksToMilliseconds = (timeStr: Nullable<string>): Nullable<string> => {
    if (!timeStr) {
        return null;
    }
    const commonTime = new Decimal(timeStr);
    return commonTime.div(10000).toString();
};
