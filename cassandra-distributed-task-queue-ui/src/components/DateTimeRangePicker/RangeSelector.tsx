import { TimeUtils, DateUtils, TimeZone } from "@skbkontur/edi-ui";
import { endOfDay, startOfDay, subDays, subMonths, subYears } from "date-fns";

import { DateTimeRange } from "../../Domain/DataTypes/DateTimeRange";

function utc(): Date {
    return DateUtils.toTimeZone(new Date(), TimeUtils.TimeZones.UTC);
}

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
        return this.setBounds(subDays(utc(), 1));
    }

    public getToday(): DateTimeRange {
        return this.setBounds(utc());
    }

    public getWeek(): DateTimeRange {
        return this.setBounds(subDays(utc(), 6), utc());
    }

    public getMonth(): DateTimeRange {
        return this.setBounds(subMonths(utc(), 1), utc());
    }

    public getMonthOf(date: Date | string): DateTimeRange {
        return this.setBounds(subMonths(DateUtils.toTimeZone(date, TimeUtils.TimeZones.UTC), 1), utc());
    }

    public getYear(): DateTimeRange {
        return this.setBounds(subYears(utc(), 1), utc());
    }
}
