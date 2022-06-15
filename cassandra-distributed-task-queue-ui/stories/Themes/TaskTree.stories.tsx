import {
    DEFAULT_THEME,
    DEFAULT_THEME_8PX_OLD,
    FLAT_THEME_8PX_OLD,
    ThemeContext,
    ThemeFactory,
} from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import React from "react";
import StoryRouter from "storybook-react-router";

import { TaskChainsTreeContainer } from "../../src/containers/TaskChainsTreeContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

import { reactUiDark } from "./reactUiDark";

export default {
    title: "Themes/TaskTree",
    decorators: [StoryRouter()],
};

const TaskTreeContainer = ({ theme }: { theme: Theme }) => (
    <ThemeContext.Provider value={theme}>
        <TaskChainsTreeContainer
            rtqMonitoringApi={new RtqMonitoringApiFake()}
            searchQuery={"?q=DocumentCirculationId"}
            useErrorHandlingContainer
            path="/AdminTools"
        />
    </ThemeContext.Provider>
);

export const Default = (): JSX.Element => <TaskTreeContainer theme={DEFAULT_THEME} />;
export const Flat = (): JSX.Element => <TaskTreeContainer theme={FLAT_THEME_8PX_OLD} />;
export const Old = (): JSX.Element => <TaskTreeContainer theme={DEFAULT_THEME_8PX_OLD} />;
export const Dark = (): JSX.Element => <TaskTreeContainer theme={ThemeFactory.create(reactUiDark)} />;
