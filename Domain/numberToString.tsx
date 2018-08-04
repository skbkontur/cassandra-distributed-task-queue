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

export default function numberToString(number: number | string): string | false {
    function numberParser(num: string, desc: number): string {
        let newNum: string = num;
        let string = "";
        let newNumHundred: string | number = "";
        if (newNum.length === 3) {
            newNumHundred = Number(newNum.substr(0, 1));
            newNum = newNum.substr(1, 3);
            string = arrNumbers[2][newNumHundred] + " ";
        }

        if (Number(newNum) < 20) {
            string += arrNumbers[0][Number(newNum)] + " ";
        } else {
            const firstNum = Number(newNum.substr(0, 1));
            const secondNum = Number(newNum.substr(1, 2));
            string += arrNumbers[1][firstNum] + " ";
            string += arrNumbers[0][secondNum] + " ";
        }
        const lastNum = parseFloat(newNum.substr(-1));
        switch (desc) {
            case 1:
                if (lastNum === 1) {
                    string += "тысяча ";
                } else if (lastNum > 1 && lastNum < 5) {
                    string += "тысячи ";
                } else {
                    string += "тысяч ";
                }
                string = string.replace("один ", "одна ");
                string = string.replace("два ", "две ");
                break;
            case 2:
                if (lastNum === 1) {
                    string += "миллион ";
                } else if (lastNum > 1 && lastNum < 5) {
                    string += "миллиона ";
                } else {
                    string += "миллионов ";
                }
                break;
            case 3:
                if (lastNum === 1) {
                    string += "миллиард ";
                } else if (lastNum > 1 && lastNum < 5) {
                    string += "миллиарда ";
                } else {
                    string += "миллиардов ";
                }
                break;
            default:
                break;
        }
        string = string.replace("  ", " ");
        return string;
    }

    if (!number || number === 0) {
        return false;
    }

    let copyNumber = number;

    if (typeof number === "string") {
        copyNumber = String(copyNumber).replace(",", ".");
        if (isNaN(parseInt(copyNumber, 10))) {
            return false;
        }
    }
    copyNumber = copyNumber.toString();
    const numberLength = copyNumber.length;
    let string = "";
    let numParser = "";
    let count = 0;
    for (let i = numberLength - 1; i >= 0; i--) {
        const numDigit = copyNumber.substr(i, 1);
        numParser = numDigit + numParser;
        if ((numParser.length === 3 || i === 0) && !isNaN(parseInt(numParser, 10))) {
            string = numberParser(numParser, count) + string;
            numParser = "";
            count++;
        }
    }
    return string.replace(/\s+$/, "").replace(/\s{2}/g, " ");
}
