import { TimeUtils, TimeZone } from "@skbkontur/edi-ui";
import { ColumnStack, Fit } from "@skbkontur/react-stack-layout";
import { ThemeContext } from "@skbkontur/react-ui";
import { useStyles } from "@skbkontur/react-ui/lib/renderEnvironment";
import { ReactElement, useContext } from "react";

import { DateTimeRange } from "../../Domain/DataTypes/DateTimeRange";

import { DatePicker } from "./DatePicker";
import { getStyles } from "./DateTimeRangePicker.styles";
import { RangeSelector } from "./RangeSelector";

export interface PredefinedRangeDefinition {
    tid?: string;
    caption: string;
    getRange: (timeZone?: TimeZone) => DateTimeRange;
}

export interface DateTimeRangePickerProps {
    error?: boolean;
    value: DateTimeRange;
    onChange: (value: DateTimeRange) => void;
    disabled?: boolean;
    timeZone?: TimeZone;
    hideTime?: boolean;
}

const defaultPredefinedRanges: PredefinedRangeDefinition[] = [
    {
        getRange: timeZone => new RangeSelector(timeZone).getYesterday(),
        tid: "TemplateDateYesterday",
        caption: "вчера",
    },
    {
        getRange: timeZone => new RangeSelector(timeZone).getToday(),
        tid: "TemplateDateToday",
        caption: "сегодня",
    },
    {
        getRange: timeZone => new RangeSelector(timeZone).getWeek(),
        tid: "TemplateDateWeek",
        caption: "неделя",
    },
    {
        getRange: timeZone => new RangeSelector(timeZone).getMonth(),
        tid: "TemplateDateMonth",
        caption: "месяц",
    },
    {
        getRange: timeZone => new RangeSelector(timeZone).getYear(),
        tid: "TemplateDateYear",
        caption: "год",
    },
];

export const DateTimeRangePicker = ({
    error,
    value: { lowerBound, upperBound },
    onChange,
    timeZone,
}: DateTimeRangePickerProps): ReactElement => {
    const theme = useContext(ThemeContext);
    const jsStyles = useStyles(getStyles);

    const fixedTimezone = TimeUtils.getTimeZoneOffsetOrDefault(timeZone);

    return (
        <ColumnStack>
            <Fit>
                <span className={jsStyles.dateRangeItem()}>
                    <DatePicker
                        data-tid="From"
                        error={error}
                        value={lowerBound}
                        defaultTime="00:00"
                        timeZone={fixedTimezone}
                        onChange={(nextValue: Nullable<Date>) =>
                            onChange({ lowerBound: nextValue, upperBound: upperBound })
                        }
                    />
                </span>
                <span className={jsStyles.dateRangeItem()}>&mdash;</span>
                <span className={jsStyles.dateRangeItem()}>
                    <DatePicker
                        data-tid="To"
                        error={error}
                        value={upperBound}
                        defaultTime="23:59"
                        timeZone={fixedTimezone}
                        onChange={(nextValue: Nullable<Date>) =>
                            onChange({ upperBound: nextValue, lowerBound: lowerBound })
                        }
                    />
                </span>
            </Fit>
            <Fit className={`${jsStyles.templates(theme)} ${jsStyles.smallGap()}`}>
                {defaultPredefinedRanges.map(({ caption, getRange, tid }) => (
                    <span key={tid} onClick={() => onChange(getRange(timeZone))} data-tid={tid}>
                        {caption}
                    </span>
                ))}
            </Fit>
        </ColumnStack>
    );
};
