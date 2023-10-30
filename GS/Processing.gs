const Fireworks = [38540, 39502, 40393, 41501]
const Lockboxes = [31357, 33797, 22508, 23142, 23379, 24141, 24142, 24848, 24849, 31357, 33797]

const ActiveSpreadsheet = SpreadsheetApp.getActive()
const DataSpreadsheet = SpreadsheetApp.openById("1Blg4BL9zTr0O7fIEcQfRMJd1iWgJH5cJ51BAv5PjOYY")

let ItemNames = {};
let DataSheets = {};

function cacheItemNames() {
    let itemSheet = DataSpreadsheet.getSheetByName("Items").getDataRange().getValues()
    for (let i = 1; i < itemSheet.length; i++) {
        ItemNames[itemSheet[i][0]] = itemSheet[i][1]
    }
}

function cacheDataSheet(dataSheetName, target) {
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

function getData(dataSheetName, outputSheetName, target, column = 1) {
    Logger.log(`Processing ${dataSheetName} for ${outputSheetName}`)
    cacheDataSheet(dataSheetName, target)

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

    processData(outputSheetName, dict, total, column)
}

function processData(outputSheetName, dict, total, column) {
    let valueArray = [["Name", "Obtained", "Percentage"]]
    for (const [key, value] of Object.entries(dict)) {
        valueArray.push([ItemNames[key], value, value / total])
    }

    setFormatAndSort(outputSheetName, valueArray, column)
}

function setFormatAndSort(outputSheetName, valueArray, column) {
    let sheet = ActiveSpreadsheet.getSheetByName(outputSheetName)
    sheet.getRange(1, column, valueArray.length, 3).setValues(valueArray)
    sheet.getRange(column === 1 ? "C:C" : "J:J").setNumberFormat("##0.00%")
    sheet.setColumnWidth(column, 200)
    sheet.getRange(2, column, valueArray.length, 3).sort(column + 1)
}

function setTime() {
    let cell = ActiveSpreadsheet.getSheetByName("Info").getRange('F3')
    cell.setValue((new Date()).toUTCString())
}

function main() {
    cacheItemNames()

    getData("Coffer Data", "Grand Company", 36635)
    getData("Coffer Data", "Grand Company", 36636, 8)
    getData("Coffer Data", "Venture", 32161)
    getData("Sanctuary Data", "Sanctuary", 41667)

    getData("Bozja Data", "Bozja", 31357)
    getData("Bozja Data", "Bozja", 33797, 8)
    getData("Eureka Data", "Anemos", 22508)
    getData("Eureka Data", "Pagos", 23142)
    getData("Eureka Data", "Pagos", 23379, 8)
    getData("Eureka Data", "Pyros", 24141)
    getData("Eureka Data", "Pyros", 24142, 8)
    getData("Eureka Data", "Hydatos", 24848)
    getData("Eureka Data", "Hydatos", 24849, 8)

    setTime()
}