import Decimal from "decimal.js";

export function ticksToMilliseconds(timeStr: Nullable<string>): Nullable<number> {
    if (!timeStr) {
        return null;
    }
    const commonTime = new Decimal(timeStr);
    return commonTime.div(10000).toNumber();
}
