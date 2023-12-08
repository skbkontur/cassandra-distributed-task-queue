import { expect, describe, it } from "vitest";

import { Lens, pathLens } from "../../src/Domain/lens";

interface X1 {
    p1: string;
}

interface X2 {
    c1: {
        p1: string;
    };
}

interface X3 {
    c1?: Nullable<{
        p1: Nullable<string>;
    }>;
}

export const view = <TTarget, TProp>(lens: Lens<TTarget, TProp>, target: TTarget): TProp => lens.get(target);

export const set = <TTarget, TProp>(lens: Lens<TTarget, TProp>, value: TProp, target: TTarget): TTarget =>
    lens.set(target, value);

export const idx = <TTarget, TProp>(t: TTarget, pick: (target: TTarget) => TProp): TProp => view(pathLens(pick), t);

describe("LensTest", () => {
    describe("for arrow function", () => {
        it("get simple property", () => {
            const lens = pathLens<X1, any>(x => x.p1);
            const target: X1 = { p1: "1" };
            expect(view(lens, target)).to.eql("1");
        });

        it("get by path", () => {
            const lens = pathLens<X2, any>(x => x.c1.p1);
            const target: X2 = { c1: { p1: "1" } };
            expect(view(lens, target)).to.eql("1");
        });

        it("set by path", () => {
            const lens = pathLens<X2, any>(x => x.c1.p1);
            const target: X2 = { c1: { p1: "1" } };
            expect(set(lens, "2", target)).to.eql({ c1: { p1: "2" } });
        });

        it("set by path with null", () => {
            const lens = pathLens<X3, any>(x => idx(x, (y: any) => y.c1.p1));
            const target: X3 = { c1: null };
            expect(set(lens, "2", target)).to.eql({ c1: { p1: "2" } });
        });

        it("check not mutate target", () => {
            const lens = pathLens<X2, any>(x => x.c1.p1);
            const target: X2 = { c1: { p1: "1" } };
            const newTarget = set(lens, "2", target);
            expect(newTarget !== target).to.eql(true);
            expect(target.c1.p1).to.eql("1");
        });

        it("set by path with nulls", () => {
            const lens = pathLens<X3, any>(x => idx(x, (y: any) => y.c1.p1));
            const target: X3 = {};
            expect(set(lens, "2", target)).to.eql({ c1: { p1: "2" } });
        });
    });

    describe("for function", () => {
        it("get simple property", () => {
            const lens = pathLens<X1, any>(function (x) {
                return x.p1;
            });
            const target: X1 = { p1: "1" };
            expect(view(lens, target)).to.eql("1");
        });

        it("get by path", () => {
            const lens = pathLens<X2, any>(function (x) {
                return x.c1.p1;
            });
            const target: X2 = { c1: { p1: "1" } };
            expect(view(lens, target)).to.eql("1");
        });

        it("set by path", () => {
            const lens = pathLens<X2, any>(function (x) {
                return x.c1.p1;
            });
            const target: X2 = { c1: { p1: "1" } };
            expect(set(lens, "2", target)).to.eql({ c1: { p1: "2" } });
        });

        it("set by path with null", () => {
            const lens = pathLens<X3, any>(function (x) {
                return idx(x, (y: any) => y.c1.p1);
            });
            const target: X3 = { c1: null };
            expect(set(lens, "2", target)).to.eql({ c1: { p1: "2" } });
        });

        it("check not mutate target", () => {
            const lens = pathLens<X2, any>(function (x) {
                return x.c1.p1;
            });
            const target: X2 = { c1: { p1: "1" } };
            const newTarget = set(lens, "2", target);
            expect(newTarget !== target).to.eql(true);
            expect(target.c1.p1).to.eql("1");
        });

        it("set by path with nulls", () => {
            const lens = pathLens<X3, any>(function (x) {
                return idx(x, (y: any) => y.c1.p1);
            });
            const target: X3 = {};
            expect(set(lens, "2", target)).to.eql({ c1: { p1: "2" } });
        });
    });
});
