import { ThemeContext } from "@skbkontur/react-ui";
import { useStyles } from "@skbkontur/react-ui/lib/renderEnvironment";
import type { ReactElement, ReactNode } from "react";
import { useContext } from "react";
import { Link, To } from "react-router-dom";

import { getStyles } from "./RouterLink.styles";

interface RouterLinkProps {
    to: To & { state?: any };
    children?: ReactNode;
    className?: string;
}

export const RouterLink = ({ to, children, className }: RouterLinkProps): ReactElement => {
    const theme = useContext(ThemeContext);
    const jsStyles = useStyles(getStyles);
    return (
        <Link
            className={`${className} ${jsStyles.routerLink(theme)}`}
            to={to}
            state={Object.prototype.hasOwnProperty.call(to, "state") && to["state"]}>
            {children}
        </Link>
    );
};
