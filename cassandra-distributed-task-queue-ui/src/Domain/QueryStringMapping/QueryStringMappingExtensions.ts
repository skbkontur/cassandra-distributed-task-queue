export function getEnumValues<TEnum>(keys: string[]): { [p: string]: TEnum } {
    return keys.reduce((prev, current) => {
        prev[current] = current;
        return prev;
    }, {});
}
