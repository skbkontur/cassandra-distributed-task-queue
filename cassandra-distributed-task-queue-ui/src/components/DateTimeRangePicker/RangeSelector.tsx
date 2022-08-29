import { endOfDay, startOfDay, subDays, subMonths, subYears } from "date-fns";

import { DateTimeRange } from "../../Domain/DataTypes/DateTimeRange";
import { TimeZone } from "../../Domain/DataTypes/Time";
import { DateUtils } from "../../Domain/Utils/DateUtils";

function now(): Date {
    return new Date();
}

export class RangeSelector {
    public timeZone: TimeZone | number;

    public constructor(timeZone: Nullable<TimeZone>) {
        this.timeZone = timeZone == null || timeZone == undefined ? -new Date().getTimezoneOffset() : timeZone;
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
        return this.setBounds(subDays(now(), 1));
    }

    public getToday(): DateTimeRange {
        return this.setBounds(now());
    }

    public getWeek(): DateTimeRange {
        return this.setBounds(subDays(now(), 6), now());
    }

    public getMonth(): DateTimeRange {
        return this.setBounds(subMonths(now(), 1), now());
    }

    public getMonthOf(date: Date | string): DateTimeRange {
        return this.setBounds(subMonths(new Date(date), 1), now());
    }

    public getYear(): DateTimeRange {
        return this.setBounds(subYears(now(), 1), now());
    }
}
