import { IconArrowRoundTimeForwardRegular16 } from "@skbkontur/icons/IconArrowRoundTimeForwardRegular16";
import { IconCheckARegular16 } from "@skbkontur/icons/IconCheckARegular16";
import { IconXRegular16 } from "@skbkontur/icons/IconXRegular16";

import { TimeLine } from "../src/components/TaskTimeLine/TimeLine/TimeLine";

export default {
    title: "RemoteTaskQueueMonitoring/TimeLine",
    component: TimeLine,
};

export const Direct = () => (
    <TimeLine>
        <TimeLine.Entry icon={<IconCheckARegular16 />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<IconXRegular16 />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<IconCheckARegular16 />}>
            <div>Started 2</div>
            <div>Now</div>
        </TimeLine.Entry>
    </TimeLine>
);

export const WithOneBranching = () => (
    <TimeLine>
        <TimeLine.Entry icon={<IconCheckARegular16 />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<IconXRegular16 />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.BranchNode>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<IconCheckARegular16 />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<IconCheckARegular16 />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.Entry icon={<IconCheckARegular16 />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
        </TimeLine.BranchNode>
    </TimeLine>
);

export const WithOneBranchingOnManyBranches = () => (
    <TimeLine>
        <TimeLine.Entry icon={<IconCheckARegular16 />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<IconXRegular16 />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.BranchNode>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<IconCheckARegular16 />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<IconCheckARegular16 />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.Entry icon={<IconCheckARegular16 />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<IconCheckARegular16 />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
        </TimeLine.BranchNode>
    </TimeLine>
);

export const WithManyBranchings = () => (
    <TimeLine>
        <TimeLine.Entry icon={<IconCheckARegular16 />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<IconXRegular16 />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.BranchNode>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<IconCheckARegular16 />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<IconCheckARegular16 />}>
                    <div>Started</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.Entry icon={<IconXRegular16 />}>
                    <div>Started</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.BranchNode>
                    <TimeLine.Branch>
                        <TimeLine.Entry icon={<IconCheckARegular16 />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                    </TimeLine.Branch>
                    <TimeLine.Branch>
                        <TimeLine.Entry icon={<IconCheckARegular16 />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                        <TimeLine.Entry icon={<IconCheckARegular16 />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                    </TimeLine.Branch>
                    <TimeLine.Branch>
                        <TimeLine.Entry icon={<IconCheckARegular16 />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                    </TimeLine.Branch>
                </TimeLine.BranchNode>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<IconCheckARegular16 />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
        </TimeLine.BranchNode>
    </TimeLine>
);

export const WithCycles = () => (
    <TimeLine>
        <TimeLine.Entry icon={<IconCheckARegular16 />}>
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
            <TimeLine.Entry icon={<IconXRegular16 />}>
                <div>Started</div>
                <div>Now</div>
            </TimeLine.Entry>
            <TimeLine.Entry icon={<IconCheckARegular16 />}>
                <div>Started 2</div>
                <div>Now</div>
            </TimeLine.Entry>
        </TimeLine.Cycled>
        <TimeLine.Entry icon={<IconCheckARegular16 />}>
            <div>Started 4</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<IconCheckARegular16 />}>
            <div>Started 5</div>
            <div>Now</div>
        </TimeLine.Entry>
    </TimeLine>
);

export const WithCyclesAndLongText = () => (
    <TimeLine>
        <TimeLine.Entry icon={<IconCheckARegular16 />}>
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
            <TimeLine.Entry icon={<IconXRegular16 />}>
                <div>Started text text text text text text text</div>
                <div>Now</div>
            </TimeLine.Entry>
            <TimeLine.Entry icon={<IconCheckARegular16 />}>
                <div>Started 2</div>
                <div>Now</div>
            </TimeLine.Entry>
        </TimeLine.Cycled>
    </TimeLine>
);

export const WithCyclesAndIcon = () => (
    <TimeLine>
        <TimeLine.Entry icon={<IconCheckARegular16 />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Cycled
            icon={<IconArrowRoundTimeForwardRegular16 />}
            content={
                <div>
                    <div>Some cycle info</div>
                    <div>Now</div>
                </div>
            }>
            <TimeLine.Entry icon={<IconXRegular16 />}>
                <div>Started text text text text text text text</div>
                <div>Now</div>
            </TimeLine.Entry>
            <TimeLine.Entry icon={<IconCheckARegular16 />}>
                <div>Started 2</div>
                <div>Now</div>
            </TimeLine.Entry>
        </TimeLine.Cycled>
    </TimeLine>
);
