import { Button, RadioGroup, Tooltip } from "@skbkontur/react-ui";
import moment from "moment";
import React from "react";

import { Ticks, TimeZone } from "../../Domain/DataTypes/Time";
import { TimeUtils } from "../../Domain/Utils/TimeUtils";

import styles from "./DateTimeView.less";

interface DateTimeViewProps {
    value: Nullable<Ticks>;
}

interface DateTimeViewState {
    timezone: TimeZone;
    defaultTimezone: TimeZone;
}

class LocalStorageUtils {
    public static getNumberOrDefault(key: string, defaultValue: number): number {
        try {
            const value = localStorage.getItem(key);
            if (value == null) {
                return defaultValue;
            }
            const result = JSON.parse(value);
            if (result != null && typeof result === "number") {
                return result;
            }
            return defaultValue;
        } catch (e) {
            return defaultValue;
        }
    }
}

type Action = () => void;

class DateTimeViewStorage {
    public callbacks: Action[] = [];
    public timezone: TimeZone | null = null;

    public set(timezone: TimeZone) {
        this.timezone = timezone;
        try {
            localStorage.setItem("RemoteTaskQueueMonitoring.DateTimeView.TimeZone", JSON.stringify(timezone));
        } catch (e) {
            // Если ничего не вышло, то и ладно
        }
        for (const callback of this.callbacks) {
            callback();
        }
    }

    public get(): TimeZone {
        if (this.timezone == null) {
            const result = LocalStorageUtils.getNumberOrDefault(
                "RemoteTaskQueueMonitoring.DateTimeView.TimeZone",
                TimeUtils.TimeZones.UTC
            );
            if (result === 0 || result === 180 || result === 300) {
                this.timezone = result;
                return result;
            }
            this.timezone = TimeUtils.TimeZones.UTC;
            return TimeUtils.TimeZones.UTC;
        }
        return this.timezone;
    }

    public subscribe(callback: () => void) {
        this.callbacks.push(callback);
    }

    public unsubscribe(callback: () => void) {
        const index = this.callbacks.indexOf(callback);
        if (index >= 0) {
            this.callbacks.splice(index, 1);
        }
    }
}

const storage = new DateTimeViewStorage();

export class DateTimeView extends React.Component<DateTimeViewProps, DateTimeViewState> {
    public state: DateTimeViewState = {
        defaultTimezone: storage.get(),
        timezone: storage.get(),
    };

    private handleUpdateGlobalTimeZone = () => {
        this.setState({
            defaultTimezone: storage.get(),
            timezone: storage.get(),
        });
    };

    public componentDidMount(): void {
        storage.subscribe(this.handleUpdateGlobalTimeZone);
    }

    public componentWillUnmount(): void {
        storage.unsubscribe(this.handleUpdateGlobalTimeZone);
    }

    public getTimeZoneShortName(timezone: TimeZone): string {
        switch (timezone) {
            case 0:
                return "UTC";
            case 180:
                return "МСК";
            case 300:
                return "ЕКБ";
            default:
                throw new Error("OutOfRange");
        }
    }

    public renderWithTimeZone(timezone: TimeZone): string {
        const { value } = this.props;
        return moment(value ? TimeUtils.ticksToDate(value) : undefined)
            .utcOffset(timezone)
            .format("DD.MM.YYYY HH:mm:ss.SSS");
    }

    private handleChangeDefault() {
        storage.set(this.state.timezone);
    }

    public selectTimeZone(): JSX.Element {
        return (
            <div data-tid="TimeZonesPopup">
                <div>
                    <RadioGroup<TimeZone>
                        items={TimeUtils.getAllTimeZones()}
                        value={this.state.timezone}
                        data-tid="TimeZones"
                        onValueChange={value => this.setState({ timezone: value })}
                        renderItem={value => (
                            <>
                                {this.renderWithTimeZone(value)} ({this.getTimeZoneShortName(value)})
                            </>
                        )}
                    />
                </div>
                <div>
                    <Button onClick={() => this.handleChangeDefault()} data-tid="UseAsDefault">
                        Использовать по умолчанию
                    </Button>
                </div>
            </div>
        );
    }

    public render(): JSX.Element {
        const { defaultTimezone } = this.state;
        const { value } = this.props;
        if (!value) {
            return <span>-</span>;
        }
        return (
            <span>
                {this.renderWithTimeZone(defaultTimezone)} (
                <Tooltip data-tid="TimeZone" render={() => this.selectTimeZone()} trigger="click" pos="right top">
                    <span className={styles.timezone}>{this.getTimeZoneShortName(defaultTimezone)}</span>
                </Tooltip>
                )
            </span>
        );
    }
}
