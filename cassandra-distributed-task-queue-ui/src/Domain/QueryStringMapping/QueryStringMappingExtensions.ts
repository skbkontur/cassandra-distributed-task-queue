export function getEnumValues<TEnum>(keys: string[]): Record<string, TEnum> {
    return keys.reduce(
        (prev, current) => {
            prev[current] = current as TEnum;
            return prev;
        },
        {} as Record<string, TEnum>
    );
}
