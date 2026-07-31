import { StringUtils, TimeUtils, TimeZone, Time, DateUtils, RussianDateFormat } from "@skbkontur/edi-ui";
import { DatePicker as DefaultDatePicker } from "@skbkontur/react-ui";
import { ReactElement, CSSProperties, useEffect, useState } from "react";

interface DatePickerProps {
    value: Nullable<Date>;
    onChange: (value: Nullable<Date>) => void;
    width?: CSSProperties["width"];
    minDate?: Date | string;
    maxDate?: Date | string;
    isHoliday?: (day: string, isWeekend: boolean) => boolean;
    timeZone?: TimeZone | number;
    defaultTime?: Time;
    disabled?: boolean;
    error?: boolean;
    "data-tid"?: string;
}

const DatePickerDefaultProps = {
    width: 120,
    minDate: "01.01.1900",
    maxDate: "31.12.2099",
    isHoliday: (_day: string, isWeekend: boolean) => isWeekend,
};

const defaultTime = "00:00";

const convertDateToStringWithTimezone = (date: Nullable<Date | string>, timeZone?: number): string => {
    const timeZoneOffset = TimeUtils.getTimeZoneOffsetOrDefault(timeZone);
    return date ? DateUtils.formatDate(date, "dd.MM.yyyy", timeZoneOffset) : "";
};

const convertStringToDate = (
    newStringifiedDate: RussianDateFormat,
    timeZone: TimeZone | number | undefined,
    time: Time
): Date => {
    const date = DateUtils.formatDate(newStringifiedDate, "dd.MM.yyyy");
    const ISODate = DateUtils.formatDate(date, "yyyy-MM-dd");
    const timeZoneOffset = TimeUtils.getTimeZoneOffsetOrDefault(timeZone);
    return new Date(`${ISODate}T${time}${TimeUtils.timeZoneOffsetToString(timeZoneOffset)}`);
};

export const DatePicker = ({
    value,
    onChange,
    minDate,
    maxDate,
    timeZone = TimeUtils.TimeZones.UTC,
    defaultTime: defaultTimeProp,
    ...restProps
}: DatePickerProps): ReactElement => {
    const [date, setDate] = useState(() => convertDateToStringWithTimezone(value, timeZone));

    useEffect(() => {
        setDate(convertDateToStringWithTimezone(value, timeZone));
    }, [value, timeZone]);

    const handleChange = (newStringifiedDate: RussianDateFormat): void => {
        setDate(newStringifiedDate);
        if (StringUtils.isNullOrWhitespace(newStringifiedDate)) {
            onChange(null);
            return;
        }

        if (!DefaultDatePicker.validate(newStringifiedDate)) {
            return;
        }

        onChange(convertStringToDate(newStringifiedDate, timeZone, defaultTimeProp || defaultTime));
    };

    return (
        <DefaultDatePicker
            {...DatePickerDefaultProps}
            {...restProps}
            maxDate={convertDateToStringWithTimezone(maxDate, timeZone)}
            minDate={convertDateToStringWithTimezone(minDate, timeZone)}
            value={date}
            onValueChange={handleChange}
        />
    );
};
