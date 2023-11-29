import { ReactNode } from "react";

import { CustomSettingsProvider, ICustomSettings } from "../src/CustomSettingsContext";

export const CustomSettingsProviderDecorator =
    (settings: Partial<ICustomSettings>) =>
    (story: () => ReactNode): ReactNode => <CustomSettingsProvider {...settings}>{story()}</CustomSettingsProvider>;
