function cacheItemNames() {
    let itemSheet = DataSpreadsheet.getSheetByName("Items").getDataRange().getValues()
    for (let i = 1; i < itemSheet.length; i++) {
        ItemNames[itemSheet[i][0]] = itemSheet[i][1]
    }
}

function getBunnyData(dataSheetName, outputSheetName, terri, target, column = 4) {
    Logger.log(`Processing ${dataSheetName} for ${outputSheetName}`)
    cacheBunnySheet(dataSheetName)

    let dict = {};
    let total = 0;
    for (let [territory, rarity, item, amount] of DataSheets[dataSheetName]) {
        if (terri !== territory || target !== rarity)
            continue;

        total += amount
        if (item in dict) {
            dict[item] += amount
        } else {
            dict[item] = amount
        }
    }

    let valueArray = processData(outputSheetName, dict, total)
    setFormatAndSort(outputSheetName, valueArray, column, true)
}