import { DEFAULT_THEME, DEFAULT_THEME_8PX, FLAT_THEME, ThemeContext, ThemeFactory } from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import React from "react";
import StoryRouter from "storybook-react-router";

import { TasksPageContainer } from "../../src/containers/TasksPageContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

import { infraUiDark } from "./infraUiDark";
import { reactUiDark } from "./reactUiDark";

export default {
    title: "Themes/Tasks",
    decorators: [StoryRouter()],
};

const TypesContainer = ({ theme }: { theme: Theme }) => (
    <ThemeContext.Provider value={theme}>
        <TasksPageContainer
            rtqMonitoringApi={new RtqMonitoringApiFake()}
            searchQuery="?q=AllTasks"
            useErrorHandlingContainer
            isSuperUser
            path="/AdminTools"
        />
    </ThemeContext.Provider>
);

export const Default = (): JSX.Element => <TypesContainer theme={DEFAULT_THEME} />;
export const Flat = (): JSX.Element => <TypesContainer theme={FLAT_THEME} />;
export const EightPx = (): JSX.Element => <TypesContainer theme={DEFAULT_THEME_8PX} />;
export const Dark = (): JSX.Element => <TypesContainer theme={ThemeFactory.create(reactUiDark)} />;
export const InfraDark = (): JSX.Element => <TypesContainer theme={ThemeFactory.create(infraUiDark)} />;
