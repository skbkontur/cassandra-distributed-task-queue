import { ColumnStack, Fit } from "@skbkontur/react-stack-layout";
import React from "react";

import { DateTimeRange } from "../../Domain/DataTypes/DateTimeRange";
import { TimeZone } from "../../Domain/DataTypes/Time";
import { TimeUtils } from "../../Domain/Utils/TimeUtils";

import { DatePicker } from "./DatePicker";
import styles from "./DateTimeRangePicker.less";
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
    lowerBoundDefaultTime?: string;
    upperBoundDefaultTime?: string;
    predefinedRanges?: PredefinedRangeDefinition[];
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

export class DateTimeRangePicker extends React.Component<DateTimeRangePickerProps> {
    public static defaultProps = {
        lowerBoundDefaultTime: "00:00",
        upperBoundDefaultTime: "23:59",
        predefinedRanges: defaultPredefinedRanges,
    };

    private handleClickPredefinedRange(value: DateTimeRange) {
        const { disabled, onChange } = this.props;
        if (!disabled) {
            onChange(value);
        }
    }

    public render(): JSX.Element {
        const {
            disabled,
            value,
            error,
            onChange,
            timeZone,
            lowerBoundDefaultTime,
            upperBoundDefaultTime,
            predefinedRanges,
            ...rest
        } = this.props;
        const { lowerBound, upperBound } = value;
        const fixedTimezone = TimeUtils.getTimeZoneOffsetOrDefault(timeZone);

        return (
            <ColumnStack className={styles.dateRange} {...rest}>
                <Fit>
                    <span className={styles.dateRangeItem}>
                        <DatePicker
                            data-tid="From"
                            error={error}
                            value={lowerBound}
                            disabled={disabled}
                            defaultTime={lowerBoundDefaultTime}
                            timeZone={fixedTimezone}
                            onChange={(nextValue: Nullable<Date>) =>
                                onChange({ lowerBound: nextValue, upperBound: upperBound })
                            }
                        />
                    </span>
                    <span className={styles.dateRangeItem}>&mdash;</span>
                    <span className={styles.dateRangeItem}>
                        <DatePicker
                            data-tid="To"
                            error={error}
                            value={upperBound}
                            disabled={disabled}
                            defaultTime={upperBoundDefaultTime}
                            timeZone={fixedTimezone}
                            onChange={(nextValue: Nullable<Date>) =>
                                onChange({ upperBound: nextValue, lowerBound: lowerBound })
                            }
                        />
                    </span>
                </Fit>
                <Fit className={`${styles.templates} ${styles.smallGap} ${disabled && styles.disabled}`}>
                    {(predefinedRanges || defaultPredefinedRanges).map(x => (
                        <span
                            key={x.tid}
                            onClick={_ => this.handleClickPredefinedRange(x.getRange(this.props.timeZone))}
                            data-tid={x.tid}>
                            {x.caption}
                        </span>
                    ))}
                </Fit>
            </ColumnStack>
        );
    }
}
