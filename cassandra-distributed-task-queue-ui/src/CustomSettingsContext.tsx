import { createContext, PropsWithChildren, useContext } from "react";
import type { JSX } from "react";

import { TaskState } from "./Domain/Api/TaskState";
import { CustomRenderer, ICustomRenderer } from "./Domain/CustomRenderer";

const TaskStateCaptions = {
    [TaskState.Unknown]: "Unknown",
    [TaskState.New]: "New",
    [TaskState.WaitingForRerun]: "Waiting for rerun",
    [TaskState.WaitingForRerunAfterError]: "Waiting for rerun after error",
    [TaskState.Finished]: "Finished",
    [TaskState.InProcess]: "In process",
    [TaskState.Fatal]: "Fatal",
    [TaskState.Canceled]: "Canceled",
};

export type TaskStateDict = Partial<Record<TaskState, string>>;

export interface ICustomSettings {
    customDetailRenderer: ICustomRenderer;
    customStateCaptions: TaskStateDict;
    hideMissingMeta: boolean;
    customSearchHelp?: JSX.Element;
}

const defaultValue: ICustomSettings = {
    customStateCaptions: TaskStateCaptions,
    customDetailRenderer: new CustomRenderer(),
    hideMissingMeta: false,
};

const CustomSettingsContext = createContext<ICustomSettings>(defaultValue);

export const CustomSettingsProvider = ({
    customStateCaptions,
    customSearchHelp,
    customDetailRenderer,
    hideMissingMeta,
    children,
}: PropsWithChildren<Partial<ICustomSettings>>) => {
    const stateCaptions = customStateCaptions || TaskStateCaptions;
    const renderer = customDetailRenderer || new CustomRenderer();
    return (
        <CustomSettingsContext.Provider
            value={{
                customStateCaptions: stateCaptions,
                customDetailRenderer: renderer,
                customSearchHelp,
                hideMissingMeta: !!hideMissingMeta,
            }}>
            {children}
        </CustomSettingsContext.Provider>
    );
};

export const useCustomSettings = () => useContext(CustomSettingsContext);
