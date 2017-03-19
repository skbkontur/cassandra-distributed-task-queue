// @flow

type TakeLastAndRejectPrevious =
    (
        <TA1, TA2, TA3, TR>(fn: (a1: TA1, a2: TA2, a3: TA3, ...rest: void[]) => Promise<TR>) =>
            ((a1: TA1, a2: TA2, a3: TA3, ...rest: void[]) => Promise<TR>)
    )
    &
    (
        <TA1, TA2, TR>(fn: (a1: TA1, a2: TA2, ...rest: void[]) => Promise<TR>) =>
            ((a1: TA1, a2: TA2, ...rest: void[]) => Promise<TR>)
    )
    &
    (
        <TA1, TR>(fn: (a1: TA1, ...rest: void[]) => Promise<TR>) =>
            ((a1: TA1, ...rest: void[]) => Promise<TR>)
    )
    ;

function takeLastAndRejectPreviousImpl(func: any): any {
    const result = (...args) => {
        return new Promise(async resolve => {
            const currentRequest = result.requestIncrement++;
            result.lastRequestId = currentRequest;
            const functionResult = await func(...args);
            if (result.lastRequestId === currentRequest) {
                resolve(functionResult);
            }
        });
    };
    result.requestIncrement = 0;
    result.lastRequestId = null;
    return result;
}

const takeLastAndRejectPrevious: TakeLastAndRejectPrevious = (takeLastAndRejectPreviousImpl: any);
export { takeLastAndRejectPrevious };
