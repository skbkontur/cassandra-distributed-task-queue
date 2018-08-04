const arrNumbers = [
    [
        "",
        "один",
        "два",
        "три",
        "четыре",
        "пять",
        "шесть",
        "семь",
        "восемь",
        "девять",
        "десять",
        "одиннадцать",
        "двенадцать",
        "тринадцать",
        "четырнадцать",
        "пятнадцать",
        "шестнадцать",
        "семнадцать",
        "восемнадцать",
        "девятнадцать",
    ],
    ["", "", "двадцать", "тридцать", "сорок", "пятьдесят", "шестьдесят", "семьдесят", "восемьдесят", "девяносто"],
    ["", "сто", "двести", "триста", "четыреста", "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот"],
];

export default function numberToString(value: number | string): string | false {
    function numberParser(num: string, desc: number): string {
        let newNum: string = num;
        let result = "";
        let newNumHundred: string | number = "";
        if (newNum.length === 3) {
            newNumHundred = Number(newNum.substr(0, 1));
            newNum = newNum.substr(1, 3);
            result = arrNumbers[2][newNumHundred] + " ";
        }

        if (Number(newNum) < 20) {
            result += arrNumbers[0][Number(newNum)] + " ";
        } else {
            const firstNum = Number(newNum.substr(0, 1));
            const secondNum = Number(newNum.substr(1, 2));
            result += arrNumbers[1][firstNum] + " ";
            result += arrNumbers[0][secondNum] + " ";
        }
        const lastNum = parseFloat(newNum.substr(-1));
        switch (desc) {
            case 1:
                if (lastNum === 1) {
                    result += "тысяча ";
                } else if (lastNum > 1 && lastNum < 5) {
                    result += "тысячи ";
                } else {
                    result += "тысяч ";
                }
                result = result.replace("один ", "одна ");
                result = result.replace("два ", "две ");
                break;
            case 2:
                if (lastNum === 1) {
                    result += "миллион ";
                } else if (lastNum > 1 && lastNum < 5) {
                    result += "миллиона ";
                } else {
                    result += "миллионов ";
                }
                break;
            case 3:
                if (lastNum === 1) {
                    result += "миллиард ";
                } else if (lastNum > 1 && lastNum < 5) {
                    result += "миллиарда ";
                } else {
                    result += "миллиардов ";
                }
                break;
            default:
                break;
        }
        result = result.replace("  ", " ");
        return result;
    }

    if (!value || value === 0) {
        return false;
    }

    let copyNumber = value;

    if (typeof value === "string") {
        copyNumber = String(copyNumber).replace(",", ".");
        if (isNaN(parseInt(copyNumber, 10))) {
            return false;
        }
    }
    copyNumber = copyNumber.toString();
    const numberLength = copyNumber.length;
    let result = "";
    let numParser = "";
    let count = 0;
    for (let i = numberLength - 1; i >= 0; i--) {
        const numDigit = copyNumber.substr(i, 1);
        numParser = numDigit + numParser;
        if ((numParser.length === 3 || i === 0) && !isNaN(parseInt(numParser, 10))) {
            result = numberParser(numParser, count) + result;
            numParser = "";
            count++;
        }
    }
    return result.replace(/\s+$/, "").replace(/\s{2}/g, " ");
}
