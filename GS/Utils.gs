function setTime() {
    let cell = ActiveSpreadsheet.getSheetByName("Info").getRange('F3')
    cell.setValue((new Date()).toUTCString())
}