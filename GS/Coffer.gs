const Fireworks = [38540, 39502, 40393, 41501]
const Lockboxes = [31357, 33797, 22508, 23142, 23379, 24141, 24142, 24848, 24849, 31357, 33797]

function getBoxData(dataSheetName, outputSheetName, target, column = 1) {
    Logger.log(`Processing ${dataSheetName} for ${outputSheetName}`)
    cacheCofferSheet(dataSheetName, target)

    let dict = {};
    let total = 0;
    for (let [coffer, item, amount] of DataSheets[dataSheetName]) {
        if (target !== coffer)
            continue;

        if (target === 32161 && item !== 8841) {
            amount = ~~(amount / 2)
        } else if (target === 41667 && Fireworks.includes(item)) {
            amount = ~~(amount / 5)
        } else if (Lockboxes.includes(target) && amount > 1) {
            amount = 1;
        }

        total += amount
        if (item in dict) {
            dict[item] += amount
        } else {
            dict[item] = amount
        }
    }

    let valueArray = processData(outputSheetName, dict, total)
    setFormatAndSort(outputSheetName, valueArray, column, false)
}