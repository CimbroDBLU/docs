
// history   usare command History

function xIconLastOp(item) {
    let template = "";

    switch (item.LastOp) {
        case 1:
            template = " <a class='k-button k-button-icontext k-grid-History'><span class='k-icon k-i-folder-open'></span></a>";
            break;
        case 2:
            template = "<a class='k-button k-button-icontext k-grid-History'><span class='k-icon k-i-file-add'> </span></a>";
            break;
        case 3:
            template = "<a class='k-button k-grid-History'><span class='k-icon k-i-printer'> </span></a>";
            break;
        case 4:
            template = " <a class='k-button k-grid-History'><span class='k-icon k-i-delete'></span ></a>";
            break;
        case 5:
            template = "<a class='k-button k-grid-History'><span class='k-icon k-i-check'></span ></a>";
            break;
        case 6:
            template = "<a class='k-button k-grid-History'><span class='k-icon k-i-reset'></span ></a>";
            break;
        case 7:
            template = "<a class='k-button k-grid-History'><span class='k-icon k-i-folder-up'></span ></a>";
            break;
        case 8:
            template = "<a class='k-button k-grid-History'><span class='k-icon k-i-edit'></span ></a>";
            break;
        case 9:
            template = "<a class='k-button k-button-icon k-grid-History'><span class='k-icon k-i-close-outline'></span ></a>";
            break;
        case 10:
            template = "<a class='k-button k-grid-History'><span class='k-icon k-i-redo'></span ></a>";
            break;
        case 11:
            template = "<a class='k-button k-grid-History'><span class='k-icon k-i-undo'></span ></a>";
            break;        default:
            template = "";
            break;
    };


    return template;

}

const docsTipiOperazioni = {
    NESSUNA : 0,
    CREATO : 1,
    ELABORATO : 2,
    STAMPATO : 3,
    CANCELLATO : 4,
    CHIUSO : 5,
    RIAPERTO : 6,
    SPOSTATO : 7,
    MODIFICATO : 8,
    ANNULLATO : 9,
    INOLTRATO: 10,
    RISPOSTO: 11
}

function gridRefresLastOp(grid, lastOp) {

    var row = grid.select();
    var dataItem = grid.dataItem(row);
    dataItem.LastOp = lastOp;
    var rowHtml = grid.rowTemplate(dataItem);
    row.replaceWith(rowHtml);
    //grid.select(row);
}

//function gridRowRefresLastOp(row, grid, lastOp) {

//    var dataItem = grid.dataItem(row);
//    dataItem.LastOp = lastOp;
//    var rowHtml = grid.rowTemplate(dataItem);
//    row.replaceWith(rowHtml);

//}


//function kendoFastReDrawRow(grid, row) {
//    var dataItem = grid.dataItem(row);

//    var rowChildren = $(row).children('td[role="gridcell"]');

//    for (var i = 0; i < grid.columns.length; i++) {

//        var column = grid.columns[i];
//        var template = column.template;
//        var cell = rowChildren.eq(i);

//        if (template !== undefined) {
//            var kendoTemplate = kendo.template(template);
//            // Render using template
//            cell.html(kendoTemplate(dataItem));
//        } else {
//            var fieldValue = dataItem[column.field];
//            var format = column.format;
//            var values = column.values;

//            if (values !== undefined && values != null) {
//                // use the text value mappings (for enums)
//                for (var j = 0; j < values.length; j++) {
//                    var value = values[j];
//                    if (value.value == fieldValue) {
//                        cell.html(value.text);
//                        break;
//                    }
//                }
//            } else if (format !== undefined) {
//                // use the format
//                cell.html(kendo.format(format, fieldValue));
//            } else {
//                // Just dump the plain old value
//                cell.html(fieldValue);
//            }
//        }
//    }
//}