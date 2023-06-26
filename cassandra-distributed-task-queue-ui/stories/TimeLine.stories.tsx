import { ArrowRoundTimeForwardIcon } from "@skbkontur/icons/esm/icons/ArrowRoundTimeForwardIcon";
import { CheckAIcon } from "@skbkontur/icons/esm/icons/CheckAIcon";
import { XIcon } from "@skbkontur/icons/esm/icons/XIcon";
import React from "react";

import { TimeLine } from "../src/components/TaskTimeLine/TimeLine/TimeLine";

export default {
    title: "RemoteTaskQueueMonitoring/TimeLine",
    component: TimeLine,
};

export const Direct = () => (
    <TimeLine>
        <TimeLine.Entry icon={<CheckAIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<XIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<CheckAIcon />}>
            <div>Started 2</div>
            <div>Now</div>
        </TimeLine.Entry>
    </TimeLine>
);

export const WithOneBranching = () => (
    <TimeLine>
        <TimeLine.Entry icon={<CheckAIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<XIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.BranchNode>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<CheckAIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<CheckAIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.Entry icon={<CheckAIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
        </TimeLine.BranchNode>
    </TimeLine>
);

export const WithOneBranchingOnManyBranches = () => (
    <TimeLine>
        <TimeLine.Entry icon={<CheckAIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<XIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.BranchNode>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<CheckAIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<CheckAIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.Entry icon={<CheckAIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<CheckAIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
        </TimeLine.BranchNode>
    </TimeLine>
);

export const WithManyBranchings = () => (
    <TimeLine>
        <TimeLine.Entry icon={<CheckAIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<XIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.BranchNode>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<CheckAIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<CheckAIcon />}>
                    <div>Started</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.Entry icon={<XIcon />}>
                    <div>Started</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.BranchNode>
                    <TimeLine.Branch>
                        <TimeLine.Entry icon={<CheckAIcon />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                    </TimeLine.Branch>
                    <TimeLine.Branch>
                        <TimeLine.Entry icon={<CheckAIcon />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                        <TimeLine.Entry icon={<CheckAIcon />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                    </TimeLine.Branch>
                    <TimeLine.Branch>
                        <TimeLine.Entry icon={<CheckAIcon />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                    </TimeLine.Branch>
                </TimeLine.BranchNode>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<CheckAIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
        </TimeLine.BranchNode>
    </TimeLine>
);

export const WithCycles = () => (
    <TimeLine>
        <TimeLine.Entry icon={<CheckAIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Cycled
            content={
                <div>
                    <div>Some cycle info</div>
                    <div>Now</div>
                </div>
            }>
            <TimeLine.Entry icon={<XIcon />}>
                <div>Started</div>
                <div>Now</div>
            </TimeLine.Entry>
            <TimeLine.Entry icon={<CheckAIcon />}>
                <div>Started 2</div>
                <div>Now</div>
            </TimeLine.Entry>
        </TimeLine.Cycled>
        <TimeLine.Entry icon={<CheckAIcon />}>
            <div>Started 4</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<CheckAIcon />}>
            <div>Started 5</div>
            <div>Now</div>
        </TimeLine.Entry>
    </TimeLine>
);

export const WithCyclesAndLongText = () => (
    <TimeLine>
        <TimeLine.Entry icon={<CheckAIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Cycled
            content={
                <div>
                    <div>Some cycle info</div>
                    <div>Now</div>
                </div>
            }>
            <TimeLine.Entry icon={<XIcon />}>
                <div>Started text text text text text text text</div>
                <div>Now</div>
            </TimeLine.Entry>
            <TimeLine.Entry icon={<CheckAIcon />}>
                <div>Started 2</div>
                <div>Now</div>
            </TimeLine.Entry>
        </TimeLine.Cycled>
    </TimeLine>
);

export const WithCyclesAndIcon = () => (
    <TimeLine>
        <TimeLine.Entry icon={<CheckAIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Cycled
            icon={<ArrowRoundTimeForwardIcon />}
            content={
                <div>
                    <div>Some cycle info</div>
                    <div>Now</div>
                </div>
            }>
            <TimeLine.Entry icon={<XIcon />}>
                <div>Started text text text text text text text</div>
                <div>Now</div>
            </TimeLine.Entry>
            <TimeLine.Entry icon={<CheckAIcon />}>
                <div>Started 2</div>
                <div>Now</div>
            </TimeLine.Entry>
        </TimeLine.Cycled>
    </TimeLine>
);
