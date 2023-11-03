function getBunnyData(dataSheetName, outputSheetName, terri, target, column = 4) {
    Logger.log(`Processing ${dataSheetName} for ${outputSheetName}`)
    cacheBunnySheet(dataSheetName)

    let dict = {};
    let total = 0;
    for (let [territory, rarity, items] of DataSheets[dataSheetName]) {
        if (terri !== territory || target !== rarity)
            continue;

        total++
        for(let item of items) {
            if (item in dict) {
                dict[item]++
            } else {
                dict[item] = 1
            }
        }
    }

    let valueArray = processData(outputSheetName, dict, total)
    setFormatAndSort(outputSheetName, valueArray, column, true)
    setTotal(outputSheetName, 2, target, total)
}