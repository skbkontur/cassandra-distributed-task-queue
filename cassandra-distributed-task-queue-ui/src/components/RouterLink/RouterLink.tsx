import { ThemeContext } from "@skbkontur/react-ui";
import type { ReactElement } from "react";
import { useContext } from "react";
import { Link, To } from "react-router-dom";

import { jsStyles } from "./RouterLink.styles";

interface RouterLinkProps {
    to: To & { state?: any };
    children?: React.ReactNode;
    className?: string;
}

export const RouterLink = ({ to, children, className }: RouterLinkProps): ReactElement => {
    const theme = useContext(ThemeContext);
    return (
        <Link
            className={`${className} ${jsStyles.routerLink(theme)}`}
            to={to}
            state={Object.prototype.hasOwnProperty.call(to, "state") && to["state"]}>
            {children}
        </Link>
    );
};
