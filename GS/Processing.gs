let ItemNames = {};
let DataSheets = {};

function cacheItemNames() {
    let itemSheet = DataSpreadsheet.getSheetByName("Items").getDataRange().getValues()
    for (let i = 1; i < itemSheet.length; i++) {
        ItemNames[itemSheet[i][0]] = itemSheet[i][1]
    }
}

function cacheCofferSheet(dataSheetName, target) {
    if (dataSheetName in DataSheets)
        return

    let content = []
    let dataSheet = DataSpreadsheet.getSheetByName(dataSheetName).getDataRange().getValues()
    for (let i = 1; i < dataSheet.length; i++) {
        let [coffer, item, amount] = [dataSheet[i][0], dataSheet[i][1], dataSheet[i][2]]

        // Old data from sanctuary does not contain coffer id
        if (target === 41667) {
            amount = item
            item = coffer
            coffer = 41667
        }

        content.push([coffer, item, amount])
    }

    DataSheets[dataSheetName] = content
}

function cacheBunnySheet(dataSheetName) {
    let content = []
    let dataSheet = DataSpreadsheet.getSheetByName(dataSheetName).getDataRange().getValues()
    for (let i = 1; i < dataSheet.length; i++) {
        let [territory, rarity, items] = [dataSheet[i][0], dataSheet[i][1], dataSheet[i][2]]

        content.push([territory, rarity, JSON.parse(items)])
    }

    DataSheets[dataSheetName] = content
}

function processData(outputSheetName, dict, total) {
    let valueArray = [["Name", "Obtained", "Percentage"]]
    for (const [key, value] of Object.entries(dict)) {
        valueArray.push([ItemNames[key], value, value / total])
    }

    return valueArray
}

function setFormatAndSort(outputSheetName, valueArray, column, autoWidth) {
    let sheet = ActiveSpreadsheet.getSheetByName(outputSheetName)
    sheet.getRange(1, column, valueArray.length, 3).setValues(valueArray)
    sheet.getRange(1, column + 2, 1000).setNumberFormat("##0.00%")

    if (autoWidth) {
        sheet.autoResizeColumns(column, column+2)
    } else {
        sheet.setColumnWidth(column, 200)
    }

    sheet.getRange(2, column, valueArray.length, 3).sort(column + 1)
}

function setTotal(outputSheetName, column, rarity, total) {
    let sheet = ActiveSpreadsheet.getSheetByName(outputSheetName)
    sheet.getRange(2009534 - rarity, column).setValue(total)
}