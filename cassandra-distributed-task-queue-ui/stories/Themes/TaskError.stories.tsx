import { LIGHT_THEME, DARK_THEME, ThemeContext, ThemeFactory } from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import { ReactElement, useContext } from "react";
import { Route, Routes, MemoryRouter } from "react-router-dom";

import { TaskDetailsPageContainer } from "../../src/containers/TaskDetailsPageContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

export default {
    title: "Themes/TaskError",
};

const TaskDetailsContainer = ({ theme }: { theme: Theme }) => {
    const currentTheme = useContext(ThemeContext);
    return (
        <ThemeContext.Provider value={ThemeFactory.create(currentTheme, theme)}>
            <MemoryRouter initialEntries={["/AdminTools/Error"]}>
                <Routes>
                    <Route
                        path="/AdminTools/:id"
                        element={
                            <TaskDetailsPageContainer
                                rtqMonitoringApi={new RtqMonitoringApiFake()}
                                useErrorHandlingContainer
                                isSuperUser
                            />
                        }
                    />
                </Routes>
            </MemoryRouter>
        </ThemeContext.Provider>
    );
};

export const Default = (): ReactElement => <TaskDetailsContainer theme={LIGHT_THEME} />;
export const Dark = (): ReactElement => <TaskDetailsContainer theme={DARK_THEME} />;
