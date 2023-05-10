import {
    DEFAULT_THEME,
    DEFAULT_THEME_8PX_OLD,
    FLAT_THEME_8PX_OLD,
    ThemeContext,
    ThemeFactory,
} from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import React from "react";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import { TasksPageContainer } from "../../src/containers/TasksPageContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

import { reactUiDark } from "./reactUiDark";

export default {
    title: "Themes/Tasks",
};

const TypesContainer = ({ theme }: { theme: Theme }) => (
    <MemoryRouter initialEntries={["/AdminTools?q=AllTasks"]}>
        <Routes>
            <Route
                path="/AdminTools"
                element={
                    <ThemeContext.Provider value={theme}>
                        <TasksPageContainer
                            rtqMonitoringApi={new RtqMonitoringApiFake()}
                            useErrorHandlingContainer
                            isSuperUser
                        />
                    </ThemeContext.Provider>
                }
            />
        </Routes>
    </MemoryRouter>
);

export const Default = (): JSX.Element => <TypesContainer theme={DEFAULT_THEME} />;
export const Flat = (): JSX.Element => <TypesContainer theme={FLAT_THEME_8PX_OLD} />;
export const Old = (): JSX.Element => <TypesContainer theme={DEFAULT_THEME_8PX_OLD} />;
export const Dark = (): JSX.Element => <TypesContainer theme={ThemeFactory.create(reactUiDark)} />;
