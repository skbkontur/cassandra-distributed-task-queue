import { endOfDay, max, min, startOfDay, subDays, subMonths, subYears } from "date-fns";

import { DateTimeRange } from "../../Domain/DataTypes/DateTimeRange";
import { TimeZone } from "../../Domain/DataTypes/Time";
import { DateUtils } from "../../Domain/Utils/DateUtils";

export class RangeSelector {
    public timeZone: TimeZone | number;

    public constructor(timeZone: Nullable<TimeZone>) {
        this.timeZone = timeZone == null ? -new Date().getTimezoneOffset() : timeZone;
    }

    public setBounds(start: Date, end: Date = start): DateTimeRange {
        const lower = startOfDay(start);
        const upper = endOfDay(end);
        return {
            lowerBound: DateUtils.toTimeZone(lower, this.timeZone),
            upperBound: DateUtils.toTimeZone(upper, this.timeZone),
        };
    }

    public getYesterday(): DateTimeRange {
        return this.setBounds(subDays(new Date(), 1));
    }

    public getToday(): DateTimeRange {
        return this.setBounds(new Date());
    }

    public getTodayConsideringUtc(): DateTimeRange {
        const now = new Date();
        const utcNow = DateUtils.toTimeZone(now, 0);
        const lower = min([now, utcNow]);
        const upper = max([now, utcNow]);
        return this.setBounds(lower, upper);
    }

    public getWeek(): DateTimeRange {
        return this.setBounds(subDays(new Date(), 6), new Date());
    }

    public getMonth(): DateTimeRange {
        return this.setBounds(subMonths(new Date(), 1), new Date());
    }

    public getMonthOf(date: Date | string): DateTimeRange {
        return this.setBounds(subMonths(new Date(date), 1), new Date());
    }

    public getYear(): DateTimeRange {
        return this.setBounds(subYears(new Date(), 1), new Date());
    }
}
