export class StringUtils {
    public static isNullOrWhitespace(value: Nullable<string>): value is null | undefined | "" {
        return value === null || value === undefined || value.trim() === "";
    }
}
