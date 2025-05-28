import { ArrowALeftIcon24Regular } from "@skbkontur/icons/ArrowALeftIcon24Regular";
import { Fill, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Loader, ThemeContext } from "@skbkontur/react-ui";
import type { CSSProperties, ReactElement, ReactNode } from "react";
import { useContext } from "react";
import { To } from "react-router-dom";

import { RouterLink } from "../RouterLink/RouterLink";

import { jsStyles } from "./CommonLayout.styles";

interface CommonLayoutProps {
    topRightTools?: Nullable<ReactElement> | string;
    children?: ReactNode;
    withArrow?: boolean;
    style?: CSSProperties;
}

export function CommonLayout({ children, topRightTools, withArrow, ...restProps }: CommonLayoutProps): ReactElement {
    const theme = useContext(ThemeContext);
    return (
        <div className={`${jsStyles.commonLayout(theme)} ${withArrow ? jsStyles.withArrow() : ""}`} {...restProps}>
            {topRightTools && <div className={jsStyles.topRightTools()}>{topRightTools}</div>}
            {children}
        </div>
    );
}

interface CommonLayoutContentProps {
    children?: React.ReactNode;
    className?: void | string;
}

CommonLayout.Content = function Content({ children, ...restProps }: CommonLayoutContentProps): ReactElement {
    return (
        <div className={jsStyles.content()} {...restProps}>
            {children}
        </div>
    );
};

interface CommonLayoutHeaderProps {
    title: string | ReactElement;
    tools?: ReactElement | null;
    children?: ReactElement;
    borderBottom?: boolean;
}

CommonLayout.Header = function Header({
    title,
    tools,
    children,
    borderBottom,
    ...restProps
}: CommonLayoutHeaderProps): ReactElement {
    const theme = useContext(ThemeContext);
    return (
        <div className={`${jsStyles.header()} ${borderBottom ? jsStyles.borderBottom(theme) : ""}`} {...restProps}>
            <RowStack baseline block gap={2}>
                <Fit>
                    <h2 className={jsStyles.headerTitle()} data-tid="Header">
                        {title}
                    </h2>
                </Fit>
                {tools && <Fill>{tools}</Fill>}
            </RowStack>
            {children && <div className={`${jsStyles.content()} ${jsStyles.headerContent()}`}>{children}</div>}
        </div>
    );
};

interface CommonLayoutGoBackProps {
    to: To;
}

CommonLayout.GoBack = function CommonLayoutGoBack({ to }: CommonLayoutGoBackProps): ReactElement {
    const theme = useContext(ThemeContext);
    return (
        <RouterLink data-tid="GoBack" to={to} className={jsStyles.backLink()}>
            <ArrowALeftIcon24Regular align="none" className={jsStyles.backLinkIcon(theme)} />
        </RouterLink>
    );
};

interface ContentLoaderProps {
    children?: React.ReactNode;
    active: boolean;
    type?: "big";
    caption?: string;
}

CommonLayout.ContentLoader = function ContentLoader(props: ContentLoaderProps): ReactElement {
    const { active, children, ...restProps } = props;

    return (
        <Loader className={jsStyles.loader()} active={active} type="big" {...restProps}>
            {children}
        </Loader>
    );
};
