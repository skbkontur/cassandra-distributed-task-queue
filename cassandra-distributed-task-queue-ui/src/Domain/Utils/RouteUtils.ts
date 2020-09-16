import { RouteComponentProps } from "react-router";

export class RouteUtils {
    public static backUrl(props: RouteComponentProps): string {
        return props.match.url.endsWith("/") ? ".." : ".";
    }
}
