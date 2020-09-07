import { ColumnStack, Fit } from "@skbkontur/react-stack-layout";
import React, { SyntheticEvent } from "react";

import { DateTimeRange, DateTimeRangeChange, ICanBeValidated } from "../../Domain/DataTypes/DateTimeRange";
import { TimeZone } from "../../Domain/DataTypes/Time";
import { TimeUtils } from "../../Domain/Utils/TimeUtils";

import { DateTimePicker } from "./DateTimePicker";
import styles from "./DateTimeRangePicker.less";
import { RangeSelector } from "./RangeSelector";
import { DatePicker } from './DatePicker';

export interface PredefinedRangeDefinition {
    tid?: string;
    caption: string;
    getRange: (timeZone?: TimeZone) => DateTimeRange;
}

export interface DateTimeRangePickerProps {
    error?: boolean;
    value: DateTimeRange;
    onChange: DateTimeRangeChange;
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

export class DateTimeRangePicker extends React.Component<DateTimeRangePickerProps> implements ICanBeValidated {
    public static defaultProps = {
        lowerBoundDefaultTime: "00:00",
        upperBoundDefaultTime: "23:59",
        predefinedRanges: defaultPredefinedRanges,
    };

    public fromDatePicker: DatePicker | DateTimePicker | null = null;

    public focus() {
        if (this.fromDatePicker != null) {
            this.fromDatePicker.focus();
        }
    }

    public handleClickPredefinedRange(e: SyntheticEvent<any>, value: DateTimeRange) {
        const { disabled, onChange } = this.props;
        if (!disabled) {
            onChange(e, value);
        }
    }

    public setRefToFromDatePicker = (el: DatePicker) => {
        this.fromDatePicker = el;
    };

    public render(): JSX.Element {
        const {
            disabled,
            value,
            error,
            onChange,
            timeZone,
            hideTime,
            lowerBoundDefaultTime,
            upperBoundDefaultTime,
            predefinedRanges,
            ...rest
        } = this.props;
        const { lowerBound, upperBound } = value;
        const Picker = hideTime ? DatePicker : DateTimePicker;

        const fixedTimezone = TimeUtils.getTimeZoneOffsetOrDefault(timeZone);

        return (
            <ColumnStack className={styles.dateRange} {...rest}>
                <Fit>
                    <span className={styles.dateRangeItem}>
                        {/* отдельно позже разобрать и пофиксить это место
                        // @ts-ignore */}
                        <Picker
                            data-tid="From"
                            ref={this.setRefToFromDatePicker}
                            error={error}
                            value={lowerBound}
                            disabled={disabled}
                            defaultTime={lowerBoundDefaultTime}
                            timeZone={fixedTimezone}
                            onChange={(e: SyntheticEvent<any>, nextValue: Nullable<Date>) =>
                                onChange(e, { lowerBound: nextValue, upperBound: upperBound })
                            }
                        />
                    </span>
                    <span className={styles.dateRangeItem}>&mdash;</span>
                    <span className={styles.dateRangeItem}>
                        {/* отдельно позже разобрать и пофиксить это место
                        // @ts-ignore */}
                        <Picker
                            data-tid="To"
                            error={error}
                            value={upperBound}
                            disabled={disabled}
                            defaultTime={upperBoundDefaultTime}
                            timeZone={fixedTimezone}
                            onChange={(e: SyntheticEvent<any>, nextValue: Nullable<Date>) =>
                                onChange(e, { upperBound: nextValue, lowerBound: lowerBound })
                            }
                        />
                    </span>
                </Fit>
                <Fit className={`${styles.templates} ${hideTime && styles.smallGap} ${disabled && styles.disabled}`}>
                    {(predefinedRanges || defaultPredefinedRanges).map(x => (
                        <span
                            key={x.tid}
                            onClick={e => this.handleClickPredefinedRange(e, x.getRange(this.props.timeZone))}
                            data-tid={x.tid}>
                            {x.caption}
                        </span>
                    ))}
                </Fit>
            </ColumnStack>
        );
    }
}
