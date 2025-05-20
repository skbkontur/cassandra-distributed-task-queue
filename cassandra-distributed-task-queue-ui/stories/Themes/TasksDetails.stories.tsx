import { LIGHT_THEME, DARK_THEME, ThemeContext, ThemeFactory } from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import { ReactElement, useContext } from "react";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import { TaskDetailsPageContainer } from "../../src/containers/TaskDetailsPageContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

export default {
    title: "Themes/TasksDetails",
};

const TaskDetailsContainer = ({ theme }: { theme: Theme }) => {
    const currentTheme = useContext(ThemeContext);
    return (
        <ThemeContext.Provider value={ThemeFactory.create(currentTheme, theme)}>
            <MemoryRouter initialEntries={["/AdminTools/Current"]}>
                <Routes>
                    <Route
                        path="/AdminTools/:id"
                        element={
                            <ThemeContext.Provider value={theme}>
                                <TaskDetailsPageContainer
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

export const Light = (): ReactElement => <TaskDetailsContainer theme={LIGHT_THEME} />;
export const Dark = (): ReactElement => <TaskDetailsContainer theme={DARK_THEME} />;
