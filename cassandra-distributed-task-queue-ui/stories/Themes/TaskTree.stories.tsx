import { LIGHT_THEME, DARK_THEME, ThemeContext, ThemeFactory } from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import { ReactElement, useContext } from "react";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import { TaskChainsTreeContainer } from "../../src/containers/TaskChainsTreeContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

export default {
    title: "Themes/TaskTree",
};

const TaskTreeContainer = ({ theme }: { theme: Theme }) => {
    const currentTheme = useContext(ThemeContext);
    return (
        <ThemeContext.Provider value={ThemeFactory.create(currentTheme, theme)}>
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
        </ThemeContext.Provider>
    );
};

export const Light = (): ReactElement => <TaskTreeContainer theme={LIGHT_THEME} />;
export const Dark = (): ReactElement => <TaskTreeContainer theme={DARK_THEME} />;
