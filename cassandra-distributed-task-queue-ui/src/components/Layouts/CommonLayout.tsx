import { IconArrowALeftRegular24 } from "@skbkontur/icons/IconArrowALeftRegular24";
import { Fill, Fit, RowStack } from "@skbkontur/react-stack-layout";
import { Loader, ThemeContext } from "@skbkontur/react-ui";
import { useStyles } from "@skbkontur/react-ui/lib/renderEnvironment";
import type { CSSProperties, ReactElement, ReactNode } from "react";
import { useContext } from "react";
import { To } from "react-router-dom";

import { RouterLink } from "../RouterLink/RouterLink";

import { getStyles } from "./CommonLayout.styles";

interface CommonLayoutProps {
    topRightTools?: Nullable<ReactElement> | string;
    children?: ReactNode;
    withArrow?: boolean;
    style?: CSSProperties;
}

export function CommonLayout({ children, topRightTools, withArrow, ...restProps }: CommonLayoutProps): ReactElement {
    const theme = useContext(ThemeContext);
    const jsStyles = useStyles(getStyles);
    return (
        <div className={`${jsStyles.commonLayout(theme)} ${withArrow ? jsStyles.withArrow() : ""}`} {...restProps}>
            {topRightTools && <div className={jsStyles.topRightTools()}>{topRightTools}</div>}
            {children}
        </div>
    );
}

interface CommonLayoutContentProps {
    children?: ReactNode;
    className?: void | string;
}

CommonLayout.Content = function Content({ children, ...restProps }: CommonLayoutContentProps): ReactElement {
    const jsStyles = useStyles(getStyles);
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
    const jsStyles = useStyles(getStyles);
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
    const jsStyles = useStyles(getStyles);
    return (
        <RouterLink data-tid="GoBack" to={to} className={jsStyles.backLink()}>
            <IconArrowALeftRegular24 align="none" className={jsStyles.backLinkIcon(theme)} />
        </RouterLink>
    );
};

interface ContentLoaderProps {
    children?: ReactNode;
    active: boolean;
    size?: "large";
    caption?: string;
}

CommonLayout.ContentLoader = function ContentLoader(props: ContentLoaderProps): ReactElement {
    const { active, children, ...restProps } = props;
    const jsStyles = useStyles(getStyles);

    return (
        <Loader className={jsStyles.loader()} active={active} size="large" {...restProps}>
            {children}
        </Loader>
    );
};
