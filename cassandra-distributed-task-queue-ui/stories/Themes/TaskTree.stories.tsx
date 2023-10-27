import {
    DEFAULT_THEME,
    DEFAULT_THEME_8PX_OLD,
    FLAT_THEME_8PX_OLD,
    ThemeContext,
    ThemeFactory,
} from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import { TaskChainsTreeContainer } from "../../src/containers/TaskChainsTreeContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

import { reactUiDark } from "./reactUiDark";

export default {
    title: "Themes/TaskTree",
};

const TaskTreeContainer = ({ theme }: { theme: Theme }) => (
    <MemoryRouter initialEntries={["/AdminTools?q=DocumentCirculationId"]}>
        <Routes>
            <Route
                path="/AdminTools"
                element={
                    <ThemeContext.Provider value={theme}>
                        <TaskChainsTreeContainer
                            rtqMonitoringApi={new RtqMonitoringApiFake()}
                            useErrorHandlingContainer
                        />
                    </ThemeContext.Provider>
                }
            />
        </Routes>
    </MemoryRouter>
);

export const Default = (): JSX.Element => <TaskTreeContainer theme={DEFAULT_THEME} />;
export const Flat = (): JSX.Element => <TaskTreeContainer theme={FLAT_THEME_8PX_OLD} />;
export const Old = (): JSX.Element => <TaskTreeContainer theme={DEFAULT_THEME_8PX_OLD} />;
export const Dark = (): JSX.Element => <TaskTreeContainer theme={ThemeFactory.create(reactUiDark)} />;
