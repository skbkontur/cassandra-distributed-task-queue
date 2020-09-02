import { expect } from "chai";

import { numberToString } from "../../../src/RemoteTaskQueueMonitoring/Domain/numberToString";

describe("numberToString", () => {
    it("должен возвращать false, если передан 0", () => {
        expect(numberToString(0)).to.equals(false);
    });

    it("должен возвращать false, если передана нечисловая строка", () => {
        expect(numberToString("xyz")).to.equals(false);
    });

    it('должен возвращать "пять", если передано 5', () => {
        expect(numberToString(5)).to.equals("пять");
    });

    it('должен возвращать "двенадцать", если передано 12', () => {
        expect(numberToString(12)).to.equals("двенадцать");
    });

    it('должен возвращать "сорок шесть", если передано 46', () => {
        expect(numberToString(46)).to.equals("сорок шесть");
    });

    it('должен возвращать "сто", если передано 100', () => {
        expect(numberToString(100)).to.equals("сто");
    });

    it('должен возвращать "одна тысяча двадцать", если передано 1020', () => {
        expect(numberToString(1020)).to.equals("одна тысяча двадцать");
    });

    it('должен возвращать "двадцать пять тысяч триста шестьдесят четыре", если передано 25364', () => {
        expect(numberToString(25364)).to.equals("двадцать пять тысяч триста шестьдесят четыре");
    });

    // TODO: скрипт в этом случае отдает херню ("сто двадцать миллионов тысяч"), надо поправить
    /*    xit('должен возвращать "сто двадцать миллионов", если передано 120000000', () => {
        expect(numberToString(120000000)).to.equals('сто двадцать миллионов');
    });*/

    it('должен возвращать "один миллион одна тысяча", если передано 1001000', () => {
        expect(numberToString(1001000)).to.equals("один миллион одна тысяча");
    });

    it('должен возвращать "сто тысяч", если передано 100000', () => {
        expect(numberToString(100000)).to.equals("сто тысяч");
    });
});
