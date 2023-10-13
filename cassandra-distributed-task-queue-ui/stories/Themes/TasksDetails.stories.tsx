import {
    DEFAULT_THEME,
    DEFAULT_THEME_8PX_OLD,
    FLAT_THEME_8PX_OLD,
    ThemeContext,
    ThemeFactory,
} from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import { CustomRenderer } from "../../src/Domain/CustomRenderer";
import { TaskDetailsPageContainer } from "../../src/containers/TaskDetailsPageContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

import { reactUiDark } from "./reactUiDark";

export default {
    title: "Themes/TasksDetails",
};

const TaskDetailsContainer = ({ theme }: { theme: Theme }) => (
    <MemoryRouter initialEntries={["/AdminTools/Current"]}>
        <Routes>
            <Route
                path="/AdminTools/:id"
                element={
                    <ThemeContext.Provider value={theme}>
                        <TaskDetailsPageContainer
                            rtqMonitoringApi={new RtqMonitoringApiFake()}
                            customRenderer={new CustomRenderer()}
                            useErrorHandlingContainer
                            isSuperUser
                        />
                    </ThemeContext.Provider>
                }
            />
        </Routes>
    </MemoryRouter>
);

export const Default = (): JSX.Element => <TaskDetailsContainer theme={DEFAULT_THEME} />;
export const Flat = (): JSX.Element => <TaskDetailsContainer theme={FLAT_THEME_8PX_OLD} />;
export const Old = (): JSX.Element => <TaskDetailsContainer theme={DEFAULT_THEME_8PX_OLD} />;
export const Dark = (): JSX.Element => <TaskDetailsContainer theme={ThemeFactory.create(reactUiDark)} />;
