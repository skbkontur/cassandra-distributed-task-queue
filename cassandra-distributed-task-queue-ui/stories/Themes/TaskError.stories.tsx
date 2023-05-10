import {
    DEFAULT_THEME,
    DEFAULT_THEME_8PX_OLD,
    FLAT_THEME_8PX_OLD,
    ThemeContext,
    ThemeFactory,
} from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import React from "react";
import { Route, Routes, MemoryRouter } from "react-router-dom";

import { CustomRenderer } from "../../index";
import { TaskDetailsPageContainer } from "../../src/containers/TaskDetailsPageContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

import { reactUiDark } from "./reactUiDark";

export default {
    title: "Themes/TaskError",
};

const TaskDetailsContainer = ({ theme }: { theme: Theme }) => (
    <ThemeContext.Provider value={theme}>
        <MemoryRouter initialEntries={["/AdminTools/Error"]}>
            <Routes>
                <Route
                    path="/AdminTools/:id"
                    element={
                        <TaskDetailsPageContainer
                            rtqMonitoringApi={new RtqMonitoringApiFake()}
                            customRenderer={new CustomRenderer()}
                            useErrorHandlingContainer
                            isSuperUser
                        />
                    }
                />
            </Routes>
        </MemoryRouter>
    </ThemeContext.Provider>
);

export const Default = (): JSX.Element => <TaskDetailsContainer theme={DEFAULT_THEME} />;
export const Flat = (): JSX.Element => <TaskDetailsContainer theme={FLAT_THEME_8PX_OLD} />;
export const Old = (): JSX.Element => <TaskDetailsContainer theme={DEFAULT_THEME_8PX_OLD} />;
export const Dark = (): JSX.Element => <TaskDetailsContainer theme={ThemeFactory.create(reactUiDark)} />;
