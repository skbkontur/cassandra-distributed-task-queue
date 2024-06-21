import { Lens, pathLens, PropertyPicker } from "@skbkontur/edi-ui";

import { DateTimeRange } from "../DataTypes/DateTimeRange";

import { Mapper, IntegerMapper, StringArrayMapper, DateTimeRangeMapper, StringMapper, SetMapper } from "./Mappers";
import { Parser, QueryStringMapping, Stringifier } from "./QueryStringMapping";

interface IPropertyConfigurator<T> {
    createParser(): Parser<T>;
    createStringifier(): Stringifier<T>;
}

class PropertyConfigurator<T extends {}, TProperty> implements IPropertyConfigurator<T> {
    public mapper: Mapper<TProperty>;
    public lens: Lens<T, Nullable<TProperty>>;

    public constructor(propertyPicker: PropertyPicker<T, Nullable<TProperty>>, mapper: Mapper<TProperty>) {
        this.lens = pathLens(propertyPicker);
        this.mapper = mapper;
    }

    public createParser(): Parser<T> {
        return (target, parsedQueryString) => this.lens.set(target, this.mapper.parse(parsedQueryString));
    }

    public createStringifier(): Stringifier<T> {
        return (target, parsedQueryString) => this.mapper.stringify(parsedQueryString, this.lens.get(target));
    }
}

export class QueryStringMappingBuilder<T extends {}> {
    public configurators: Array<IPropertyConfigurator<T>> = [];

    public mapTo<TProperty>(
        propertyPicker: PropertyPicker<T, Nullable<TProperty>>,
        mapper: Mapper<TProperty>
    ): QueryStringMappingBuilder<T> {
        const result = new PropertyConfigurator<T, TProperty>(propertyPicker, mapper);
        this.configurators.push(result);
        return this;
    }

    public mapToStringArray(
        propertyPicker: PropertyPicker<T, Nullable<string[]>>,
        queryStringParameterName: string,
        defaultValue?: Nullable<string[]>
    ): QueryStringMappingBuilder<T> {
        return this.mapTo(propertyPicker, new StringArrayMapper(queryStringParameterName, defaultValue));
    }

    public mapToInteger(
        propertyPicker: PropertyPicker<T, Nullable<number>>,
        queryStringParameterName: string,
        defaultValue?: Nullable<number>
    ): QueryStringMappingBuilder<T> {
        return this.mapTo(propertyPicker, new IntegerMapper(queryStringParameterName, defaultValue));
    }

    public mapToDateTimeRange(
        propertyPicker: PropertyPicker<T, Nullable<DateTimeRange>>,
        queryStringParameterName: string,
        defaultValue: Nullable<DateTimeRange> = null
    ): QueryStringMappingBuilder<T> {
        return this.mapTo(propertyPicker, new DateTimeRangeMapper(queryStringParameterName, defaultValue));
    }

    public mapToString(
        propertyPicker: PropertyPicker<T, Nullable<string>>,
        queryStringParameterName: string,
        defaultValue?: Nullable<string>
    ): QueryStringMappingBuilder<T> {
        return this.mapTo(propertyPicker, new StringMapper(queryStringParameterName, defaultValue));
    }

    public mapToSet<TEnum>(
        propertyPicker: PropertyPicker<T, Nullable<TEnum[]>>,
        queryStringParameterName: string,
        enumValues: { [key: string]: TEnum },
        allowNegationOperator = false
    ): QueryStringMappingBuilder<T> {
        return this.mapTo(propertyPicker, new SetMapper(queryStringParameterName, enumValues, allowNegationOperator));
    }

    public build(): QueryStringMapping<T> {
        return new QueryStringMapping(
            this.configurators.map(x => x.createParser()),
            this.configurators.map(x => x.createStringifier())
        );
    }
}
