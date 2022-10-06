import { isValid, parse as parseDateInternal } from "date-fns";
import _ from "lodash";

import { DateTimeRange } from "../DataTypes/DateTimeRange";
import { DateUtils } from "../Utils/DateUtils";

export type QueryObject = any;

export interface Mapper<T> {
    parse(parsedQueryString: QueryObject): Nullable<T>;
    stringify(parsedQueryString: QueryObject, value: Nullable<T>): QueryObject;
}

export class PlainValueMapper<T> implements Mapper<T> {
    public parameterName: string;

    public constructor(queryStringParameterName: string) {
        this.parameterName = queryStringParameterName;
    }

    public parseString(_parameterValue: string): Nullable<T> {
        throw new Error("Method should be implemented");
    }

    public stringifyValue(_value: Nullable<T>): Nullable<string> {
        throw new Error("Method should be implemented");
    }

    protected parseEmpty(): Nullable<T> {
        return null;
    }

    public parse(parsedQueryString: QueryObject): Nullable<T> {
        const value = parsedQueryString[this.parameterName];
        if (value === undefined) {
            return this.parseEmpty();
        }
        if (typeof value !== "string") {
            return null;
        }
        return this.parseString(value);
    }

    public stringify(parsedQueryString: QueryObject, value: Nullable<T>): QueryObject {
        const result = this.stringifyValue(value);
        if (result === undefined || result === null) {
            return parsedQueryString;
        }
        return {
            ...parsedQueryString,
            [this.parameterName]: this.stringifyValue(value),
        };
    }
}

export class IntegerMapper extends PlainValueMapper<number> {
    public defaultValue: null | number;

    public constructor(queryStringParameterName: string, defaultValue: Nullable<number>) {
        super(queryStringParameterName);
        if (defaultValue !== undefined) {
            this.defaultValue = defaultValue;
        } else {
            this.defaultValue = null;
        }
    }

    public parseString(parameterValue: string): Nullable<number> {
        const result = parseInt(parameterValue, 10);
        return isNaN(result) ? this.defaultValue : result;
    }

    protected parseEmpty(): Nullable<number> {
        return this.defaultValue;
    }

    public stringifyValue(value: Nullable<number>): any {
        if (value == null) {
            return null;
        }
        if (this.defaultValue != null && value === this.defaultValue) {
            return null;
        }
        return value.toString();
    }
}

export class StringArrayMapper extends PlainValueMapper<string[]> {
    public defaultValue: null | Nullable<string[]>;

    public constructor(queryStringParameterName: string, defaultValue: Nullable<string[]>) {
        super(queryStringParameterName);
        this.defaultValue = defaultValue || null;
    }

    protected parseEmpty(): Nullable<any[]> {
        return this.defaultValue;
    }

    public parseString(parameterValue: Nullable<string>): Nullable<string[]> {
        if (parameterValue == null || parameterValue === "") {
            return this.defaultValue;
        }
        const values = parameterValue.split(" ");
        return values;
    }

    public stringifyValue(value: null | undefined | string[]): Nullable<string> {
        if (value == null || value.length === 0) {
            return undefined;
        }
        const result = value.filter(x => x !== null).join(" ");
        return result.length === 0 ? undefined : result;
    }
}

export class DateTimeRangeMapper {
    private readonly parameterName: string;
    private readonly defaultValue: null | DateTimeRange;

    public constructor(queryStringParameterPrefix: string, defaultValue: null | DateTimeRange) {
        this.parameterName = queryStringParameterPrefix;
        this.defaultValue = defaultValue || null;
    }

    public parseDate(parameterValue: Nullable<string>): Nullable<Date> {
        if (!parameterValue) {
            return null;
        }
        const parsedValue = parseDateInternal(parameterValue, "yyyy-MM-dd", new Date());
        return isValid(parsedValue) ? DateUtils.fromLocalToUtc(parsedValue) : null;
    }

    public stringifyDate(value: Nullable<Date>): Nullable<string> {
        return value ? DateUtils.convertDateToString(value, undefined, "yyyy-MM-dd") : undefined;
    }

    public parse(parsedQueryString: QueryObject): Nullable<DateTimeRange> {
        const fromString = parsedQueryString[this.parameterName + ".from"];
        const toString = parsedQueryString[this.parameterName + ".to"];
        if (this.defaultValue != null && fromString == undefined && toString == undefined) {
            return this.defaultValue;
        }
        const lowerBound = this.parseDate(fromString);
        const upperBound = this.parseDate(toString);
        if (this.defaultValue != null && lowerBound == undefined && upperBound == undefined) {
            return this.defaultValue;
        }
        return {
            lowerBound: lowerBound,
            upperBound: upperBound,
        };
    }

    public stringify(parsedQueryString: QueryObject, value: Nullable<DateTimeRange>): QueryObject {
        if (value === null || value === undefined) {
            return parsedQueryString;
        }
        if (this.isDefaultValue(value)) {
            const {
                [this.parameterName + ".from"]: from,
                [this.parameterName + ".to"]: to,
                ...result
            } = parsedQueryString;
            return result;
        }
        let result = parsedQueryString;
        if (value.lowerBound !== null && value.lowerBound !== undefined) {
            result = {
                ...result,
                [this.parameterName + ".from"]: this.stringifyDate(value.lowerBound),
            };
        }
        if (value.upperBound !== null && value.upperBound !== undefined) {
            result = {
                ...result,
                [this.parameterName + ".to"]: this.stringifyDate(value.upperBound),
            };
        }
        return result;
    }

    public isDefaultValue(value: DateTimeRange): boolean {
        if (this.defaultValue == null) {
            return false;
        }
        const defaultLower = this.defaultValue.lowerBound;
        const valueLower = value.lowerBound;
        if (defaultLower != null && valueLower != null && defaultLower.getTime() !== valueLower.getTime()) {
            return false;
        }

        const defaultUpper = this.defaultValue.upperBound;
        const valueUpper = value.upperBound;
        if (defaultUpper != null && valueUpper != null && defaultUpper.getTime() !== valueUpper.getTime()) {
            return false;
        }
        if (defaultLower == null && valueLower != null) {
            return false;
        }
        if (defaultUpper == null && valueUpper != null) {
            return false;
        }

        return true;
    }
}

export class StringMapper extends PlainValueMapper<string> {
    public defaultValue: null | string;

    public constructor(queryStringParameterName: string, defaultValue: Nullable<string>) {
        super(queryStringParameterName);
        this.defaultValue = defaultValue || null;
    }

    public parseEmpty(): Nullable<string> {
        return this.defaultValue;
    }

    public parseString(parameterValue: string): Nullable<string> {
        return parameterValue;
    }

    public stringifyValue(value: Nullable<string>): any {
        if (value === null || value === undefined || value === "") {
            return null;
        }
        if (this.defaultValue != null && this.defaultValue === value) {
            return null;
        }
        return value;
    }
}

export class SetMapper<T> extends PlainValueMapper<T[]> {
    public enumValues: { [key: string]: T };
    public allowNegationOperator: boolean;
    public static nullValueString = "Unknown";

    public constructor(
        queryStringParameterName: string,
        enumValues: { [key: string]: T },
        allowNegationOperator: boolean
    ) {
        super(queryStringParameterName);
        this.allowNegationOperator = allowNegationOperator;
        this.enumValues = enumValues;
    }

    public parseString(parameterValue: string): Nullable<T[]> {
        if (parameterValue === null || parameterValue === undefined || parameterValue === "") {
            return null;
        }
        let values;
        if (this.allowNegationOperator && parameterValue[0] === "!") {
            const negatedValues = parameterValue.replace(/^!/, "").split(" ");
            values = _.difference(this.getKeys(), negatedValues);
        } else {
            values = parameterValue.split(" ");
        }
        return values.filter(x => this.getKeys().includes(x)).map(x => this.getEnumValue(x));
    }

    public buildPositiveValuesString(values: T[]): string {
        return values
            .map(x => this.getString(x))
            .filter(x => x !== null)
            .join(" ");
    }

    public buildNegativeValuesString(values: T[]): string {
        const positiveValues = values.map(x => this.getString(x)).filter(x => x !== null);
        const negativeValues = _.difference(this.getKeys(), positiveValues);
        return "!" + negativeValues.join(" ");
    }

    public stringifyValue(value: Nullable<T[]>): Nullable<string> {
        if (value === null || value === undefined || value.length === 0) {
            return undefined;
        }
        const positiveValues = this.buildPositiveValuesString(value);
        if (this.allowNegationOperator) {
            const negativeValues = this.buildNegativeValuesString(value);
            if (positiveValues.length > negativeValues.length) {
                return negativeValues;
            }
        }
        return positiveValues.length === 0 ? undefined : positiveValues;
    }

    protected getString(value: T): string | null {
        for (const enumString in this.enumValues) {
            if (this.enumValues[enumString] === value) {
                return enumString;
            }
        }
        return null;
    }

    protected getKeys(): string[] {
        return Object.keys(this.enumValues);
    }

    protected getEnumValue(repr: string): T {
        return this.enumValues[repr];
    }
}
