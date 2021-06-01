import { DEFAULT_THEME, DEFAULT_THEME_OLD, FLAT_THEME, ThemeContext, ThemeFactory } from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import React from "react";
import StoryRouter from "storybook-react-router";

import { CustomRenderer } from "../../index";
import { TaskDetailsPageContainer } from "../../src/containers/TaskDetailsPageContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

import { reactUiDark } from "./reactUiDark";

export default {
    title: "Themes/TaskError",
    decorators: [StoryRouter()],
};

const TaskDetailsContainer = ({ theme }: { theme: Theme }) => (
    <ThemeContext.Provider value={theme}>
        <TaskDetailsPageContainer
            rtqMonitoringApi={new RtqMonitoringApiFake()}
            id="Error"
            customRenderer={new CustomRenderer()}
            useErrorHandlingContainer
            isSuperUser
            path="/AdminTools"
            parentLocation="/"
        />
    </ThemeContext.Provider>
);

export const Default = (): JSX.Element => <TaskDetailsContainer theme={DEFAULT_THEME} />;
export const Flat = (): JSX.Element => <TaskDetailsContainer theme={FLAT_THEME} />;
export const Old = (): JSX.Element => <TaskDetailsContainer theme={DEFAULT_THEME_OLD} />;
export const Dark = (): JSX.Element => <TaskDetailsContainer theme={ThemeFactory.create(reactUiDark)} />;
