import { DEFAULT_THEME, DEFAULT_THEME_8PX, FLAT_THEME, ThemeContext, ThemeFactory } from "@skbkontur/react-ui";
import { Theme } from "@skbkontur/react-ui/lib/theming/Theme";
import React from "react";
import StoryRouter from "storybook-react-router";

import { TaskChainsTreeContainer } from "../../src/containers/TaskChainsTreeContainer";
import { RtqMonitoringApiFake } from "../Api/RtqMonitoringApiFake";

import { infraUiDark } from "./infraUiDark";
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
export const Flat = (): JSX.Element => <TaskTreeContainer theme={FLAT_THEME} />;
export const EightPx = (): JSX.Element => <TaskTreeContainer theme={DEFAULT_THEME_8PX} />;
export const Dark = (): JSX.Element => <TaskTreeContainer theme={ThemeFactory.create(reactUiDark)} />;
export const InfraDark = (): JSX.Element => <TaskTreeContainer theme={ThemeFactory.create(infraUiDark)} />;
