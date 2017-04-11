// @flow
import React from 'react';
import { Button, RadioGroup, Tooltip } from 'ui';
import moment from 'moment';
import cn from './DateTimeView.less';
import { ticksToDate, TimeZones } from '../../../Commons/DataTypes/Time';
import type { Ticks, TimeZone } from '../../../Commons/DataTypes/Time';

type DateTimeViewProps = {
    value: ?Ticks;
};

type DateTimeViewState = {
    timezone: TimeZone;
    defaultTimezone: TimeZone;
};

class DateTimeViewStorage {
    callbacks = [];
    timezone: TimeZone | null = null;

    set(timezone: TimeZone) {
        this.timezone = timezone;
        try {
            window.localStorage.setItem(
                'RemoteTaskQueueMonitoring.DateTimeView.TimeZone', JSON.stringify(timezone));
        }
        catch (e) { // eslint-disable-line

        }
        for (const callback of this.callbacks) {
            callback();
        }
    }

    get(): TimeZone {
        if (this.timezone === null) {
            try {
                this.timezone = JSON.parse(
                    window.localStorage.getItem('RemoteTaskQueueMonitoring.DateTimeView.TimeZone'));
            }
            catch (e) {
                this.timezone = TimeZones.UTC;
            }
        }
        return this.timezone || TimeZones.UTC;
    }

    subscribe(callback: () => void) {
        this.callbacks.push(callback);
    }

    unsubscribe(callback: () => void) {
        const index = this.callbacks.indexOf(callback);
        if (index >= 0) {
            this.callbacks.splice(index, 1);
        }
    }
}

const storage = new DateTimeViewStorage();

export default class DateTimeView extends React.Component {
    props: DateTimeViewProps;
    state: DateTimeViewState = {
        defaultTimezone: storage.get(),
        timezone: storage.get(),
    };

    handleUpdateGlobalTimeZone = () => {
        this.setState({
            defaultTimezone: storage.get(),
            timezone: storage.get(),
        });
    }

    componentWillMount() {
        storage.subscribe(this.handleUpdateGlobalTimeZone);
    }

    componentWillUnmount() {
        storage.unsubscribe(this.handleUpdateGlobalTimeZone);
    }

    getTimeZoneShortName(timezone: TimeZone): string {
        switch (timezone) {
            case 0:
                return 'UTC';
            case 180:
                return 'МСК';
            case 300:
                return 'ЕКБ';
            default:
                return timezone.toString();
        }
    }

    renderWithTimeZone(timezone: TimeZone): React.Element<*> {
        const { value } = this.props;
        return moment(value ? ticksToDate(value) : null).utcOffset(timezone).format('DD.MM.YYYY HH:mm:ss.SSS');
    }

    handleChangeDefault() {
        storage.set(this.state.timezone);
    }

    selectTimeZone(): React.Element<*> {
        return (
            <div>
                <div>
                    <RadioGroup
                        items={Object.values(TimeZones)}
                        value={this.state.timezone}
                        onChange={(e, value) => this.setState({ timezone: value })}
                        renderItem={value => {
                            return (
                                <span>
                                    {this.renderWithTimeZone(value)}
                                    {' '}
                                    ({this.getTimeZoneShortName(value)})
                                </span>
                            );
                        }}
                    />
                </div>
                <div>
                    <Button onClick={() => this.handleChangeDefault()}>Использовать по умолчанию</Button>
                </div>
            </div>
        );
    }

    render(): React.Element<*> {
        const { defaultTimezone } = this.state;
        const { value } = this.props;
        if (!value) {
            return <span>-</span>;
        }
        return (
            <span>
                {this.renderWithTimeZone(defaultTimezone)}
                {' '}
                (
                    <Tooltip render={() => this.selectTimeZone()} trigger='click' pos='right top'>
                        <span className={cn('timezone')}>
                            {this.getTimeZoneShortName(defaultTimezone)}
                        </span>
                    </Tooltip>
                )
            </span>
        );
    }
}
