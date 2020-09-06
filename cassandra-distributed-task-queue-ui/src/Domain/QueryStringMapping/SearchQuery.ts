export class SearchQuery {
    public static combine(...searchQueries: Array<Nullable<string>>): undefined | string {
        if (searchQueries.every(x => !x)) {
            return undefined;
        }
        const [firstQuery, ...restQueries] = searchQueries.filter(Boolean) as string[];
        return [firstQuery, ...restQueries.map(x => x.replace(/^\?/, "&"))].join("");
    }
}
