import { resolvePath } from "react-router-dom";

export class RouteUtils {
    public static backUrl = (path: string): string => (path.endsWith("/") ? ".." : resolvePath("..", path).pathname);
}
