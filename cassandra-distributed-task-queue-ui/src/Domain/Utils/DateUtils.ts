import { addMinutes, format, parse, subMinutes } from "date-fns";

import { RussianDateFormat } from "../DataTypes/DateTimeRange";
import { TimeZone } from "../DataTypes/Time";

export class DateUtils {
    private static readonly datePickerFormat: RussianDateFormat = "dd.MM.yyyy";

    public static isCorrectTime(time: string): boolean {
        return Boolean(time.match(/^([01]?[0-9]|2[0-3]):[0-5][0-9]/));
    }

    public static convertDateToString(
        date: Date | string,
        timeZone: number | undefined,
        dateformat: string = this.datePickerFormat
    ): string {
        const zonedDate = timeZone == undefined ? new Date(date) : this.toTimeZone(new Date(date), timeZone);
        return format(zonedDate, dateformat);
    }

    public static convertStringToDate(date: RussianDateFormat): Date {
        return parse(date, this.datePickerFormat, new Date());
    }

    public static toTimeZone(date: Date | string, timeZone: TimeZone | number): Date {
        const dateDate = new Date(date);
        return addMinutes(dateDate, dateDate.getTimezoneOffset() + timeZone);
    }

    public static fromLocalToUtc(date: Date | string): Date {
        const dateDate = new Date(date);
        return subMinutes(dateDate, dateDate.getTimezoneOffset());
    }
}
