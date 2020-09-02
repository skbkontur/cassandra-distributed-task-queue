import DeleteIcon from "@skbkontur/react-icons/Delete";
import OkIcon from "@skbkontur/react-icons/Ok";
import RefreshIcon from "@skbkontur/react-icons/Refresh";
import * as React from "react";

import { TimeLine } from "../../src/RemoteTaskQueueMonitoring/components/TaskTimeLine/TimeLine/TimeLine";

export default {
    title: "RemoteTaskQueueMonitoring/TimeLine",
    component: TimeLine,
};

export const Direct = () => (
    <TimeLine>
        <TimeLine.Entry icon={<OkIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<DeleteIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<OkIcon />}>
            <div>Started 2</div>
            <div>Now</div>
        </TimeLine.Entry>
    </TimeLine>
);

export const WithOneBranching = () => (
    <TimeLine>
        <TimeLine.Entry icon={<OkIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<DeleteIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.BranchNode>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<OkIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<OkIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.Entry icon={<OkIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
        </TimeLine.BranchNode>
    </TimeLine>
);

export const WithOneBranchingOnManyBranches = () => (
    <TimeLine>
        <TimeLine.Entry icon={<OkIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<DeleteIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.BranchNode>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<OkIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<OkIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.Entry icon={<OkIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<OkIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
        </TimeLine.BranchNode>
    </TimeLine>
);

export const WithManyBranchings = () => (
    <TimeLine>
        <TimeLine.Entry icon={<OkIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<DeleteIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.BranchNode>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<OkIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<OkIcon />}>
                    <div>Started</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.Entry icon={<DeleteIcon />}>
                    <div>Started</div>
                    <div>Now</div>
                </TimeLine.Entry>
                <TimeLine.BranchNode>
                    <TimeLine.Branch>
                        <TimeLine.Entry icon={<OkIcon />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                    </TimeLine.Branch>
                    <TimeLine.Branch>
                        <TimeLine.Entry icon={<OkIcon />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                        <TimeLine.Entry icon={<OkIcon />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                    </TimeLine.Branch>
                    <TimeLine.Branch>
                        <TimeLine.Entry icon={<OkIcon />}>
                            <div>Started 2</div>
                            <div>Now</div>
                        </TimeLine.Entry>
                    </TimeLine.Branch>
                </TimeLine.BranchNode>
            </TimeLine.Branch>
            <TimeLine.Branch>
                <TimeLine.Entry icon={<OkIcon />}>
                    <div>Started 2</div>
                    <div>Now</div>
                </TimeLine.Entry>
            </TimeLine.Branch>
        </TimeLine.BranchNode>
    </TimeLine>
);

export const WithCycles = () => (
    <TimeLine>
        <TimeLine.Entry icon={<OkIcon />}>
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
            <TimeLine.Entry icon={<DeleteIcon />}>
                <div>Started</div>
                <div>Now</div>
            </TimeLine.Entry>
            <TimeLine.Entry icon={<OkIcon />}>
                <div>Started 2</div>
                <div>Now</div>
            </TimeLine.Entry>
        </TimeLine.Cycled>
        <TimeLine.Entry icon={<OkIcon />}>
            <div>Started 4</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Entry icon={<OkIcon />}>
            <div>Started 5</div>
            <div>Now</div>
        </TimeLine.Entry>
    </TimeLine>
);

export const WithCyclesAndLongText = () => (
    <TimeLine>
        <TimeLine.Entry icon={<OkIcon />}>
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
            <TimeLine.Entry icon={<DeleteIcon />}>
                <div>Started text text text text text text text</div>
                <div>Now</div>
            </TimeLine.Entry>
            <TimeLine.Entry icon={<OkIcon />}>
                <div>Started 2</div>
                <div>Now</div>
            </TimeLine.Entry>
        </TimeLine.Cycled>
    </TimeLine>
);

export const WithCyclesAndIcon = () => (
    <TimeLine>
        <TimeLine.Entry icon={<OkIcon />}>
            <div>Started</div>
            <div>Now</div>
        </TimeLine.Entry>
        <TimeLine.Cycled
            icon={<RefreshIcon />}
            content={
                <div>
                    <div>Some cycle info</div>
                    <div>Now</div>
                </div>
            }>
            <TimeLine.Entry icon={<DeleteIcon />}>
                <div>Started text text text text text text text</div>
                <div>Now</div>
            </TimeLine.Entry>
            <TimeLine.Entry icon={<OkIcon />}>
                <div>Started 2</div>
                <div>Now</div>
            </TimeLine.Entry>
        </TimeLine.Cycled>
    </TimeLine>
);
