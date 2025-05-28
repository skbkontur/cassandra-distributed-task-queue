import { LIGHT_THEME, DARK_THEME, ThemeContext, ThemeFactory } from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import { ReactElement, useContext } from "react";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import { TasksPageContainer } from "../../src/containers/TasksPageContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

export default {
    title: "Themes/Tasks",
};

const TypesContainer = ({ theme }: { theme: Theme }) => {
    const currentTheme = useContext(ThemeContext);
    return (
        <ThemeContext.Provider value={ThemeFactory.create(currentTheme, theme)}>
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
        </ThemeContext.Provider>
    );
};

export const Light = (): ReactElement => <TypesContainer theme={LIGHT_THEME} />;
export const Dark = (): ReactElement => <TypesContainer theme={DARK_THEME} />;
