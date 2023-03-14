import React from "react";

import { RouterLink } from "../RouterLink/RouterLink";

import { jsStyles } from "./GoBackLink.styles";

export const GoBackLink = ({ backUrl }: { backUrl?: Nullable<string> }): JSX.Element => (
    <RouterLink data-tid="GoBack" className={jsStyles.goBackLink()} to={backUrl || ""}>
        <svg width="35" height="35" viewBox="0 0 35 35" fill="none" xmlns="http://www.w3.org/2000/svg">
            <circle cx="17.5" cy="17.5" r="17.5" fill="#DADADA" />
            <path d="M20.5 10L13 17L20.5 24" stroke="#333333" />
        </svg>
    </RouterLink>
);
