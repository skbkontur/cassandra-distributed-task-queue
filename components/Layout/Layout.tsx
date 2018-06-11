import * as React from "react";

import ServiceHeader from "../../../ServiceHeader/components/ServiceHeader";

type LayoutProps = {
    children?: any;
};

export default function Layout({ children }: LayoutProps): React.ReactNode {
    return <ServiceHeader currentInterfaceType={null}>{children}</ServiceHeader>;
}
