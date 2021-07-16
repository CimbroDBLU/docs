
var UrlActions = null;
var NomeCartella = null;
var idItem = "";
var tipoItem = null;
var TipoAll = null;
var TipiOggetto = null;
var zipItem = null;


// eventi

function onSelectCartella(e) {

    if (e.item) {
        var dataItem = this.dataItem(e.item.index());
        NomeCartella = dataItem.Cartella;
        $("#gridZip").data("kendoGrid").dataSource.read();
    }
}


function getOrigine() {
    var cartella = "";
    if (NomeCartella != null) {
        cartella = NomeCartella;
    }
    else
        cartella = $("#cmbCartelleZip").data("kendoDropDownList").value();
    ;
    return {
        Tipo: TipoAll.Codice,
        Origine: cartella
    };
}

function viewZipHistory(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    idItem = dataItem.Id;
    tipoItem = TipiOggetto.ALLEGATO
    $('#logs').data('kendoGrid').dataSource.read();
    var dialog = $("#wHistory").data("kendoWindow");
    dialog.center();
    dialog.open();
}



function gridZipOnChange(e) {
    var data = this.dataItem(this.select());
    zipItem = data;
    PulisciDettaglio();

    $("#IdAllegato").val(data.Id);

    $.ajax({
        url: UrlActions.ZipView_InArrivoCaricaDettaglio,
        type: 'POST',
        cache: false,
        data: { Id: data.Id },
        success: function (data) {
            var dettaglio = data;
            MostraDettaglio(dettaglio);
        },
        error: function (data) {

        }
    });
}

// Dettaglio
function PulisciDettaglio() {
    $("#IdAllegato").val("");
    $('#IdFascicolo').val("");
    $('#IdElemento').val("");
    $("#TestoZip").val("");
    $('#zipAttachments').data('kendoGrid').dataSource.data("{}")
    //$('#ApriDettaglio').hide();
    //$('#AggiungiAElemento').hide();
}

function MostraDettaglio(dettaglio) {
    $("#TestoZip").val(dettaglio.DescrizioneElemento);

    var soggettoPrec = $('#CodiceSoggetto').val();
    $('#CodiceSoggetto').val(dettaglio.CodiceSoggetto);
    $('#NomeSoggetto').val(dettaglio.NomeSoggetto);


    $('#IdFascicolo').val(dettaglio.IdFascicolo);
    $('#IdElemento').val(dettaglio.IdElemento);
    //alert(dettaglio.DescrizioneElemento);
    //$('#DescrizioneElemento').val(dettaglio.DescrizioneElemento);
    //if (dettaglio.IdElemento != '') {
        //$('#divElemento').show();
        //$('#ApriDettaglio').show();
        //$('#AggiungiAElemento').show();
        //alert(dettaglio.Stato);

    //}
    $('#zipAttachments').data('kendoGrid').dataSource.data(dettaglio.FileAllegati);
}

function RiapriZip() {
 
    if (zipItem != null) {
        $("#IdAllegato").val(zipItem.Id);
        var obj = {
            IdAllegato: $("#IdAllegato").val()
        };
        $.ajax({
            url: UrlActions.ZipView_Processati_Riapri,
            type: 'POST',
            cache: false,
            data: obj,
            success: function (data) {
                if (data.Successo) {
                    var grid = $("#gridZip").data("kendoGrid");
                    grid.dataSource.remove(zipItem);
                    zipItem = null;
                }
            },
            error: function (data) {
                var ok = $.parseJSON(data);
            }
        });
    }
}

function CancellaZip() {

    if (zipItem != null) {
        $("#IdAllegato").val(zipItem.Id);

        var dialog = $("#wElimina").data("kendoDialog");
        dialog.open();
    }
}

function onAnnulla() {
    $("#wElimina").data("kendoDialog").close();
}

function onElimina() {

    var obj = {
        IdAllegato: $("#IdAllegato").val()
    };
    $.ajax({
        url: UrlActions.ZipView_Processati_Cancella,
        type: 'POST',
        cache: false,
        data: obj,
        success: function (data) {
            if (data.Successo) { 
                var grid = $("#gridZip").data("kendoGrid");
                grid.dataSource.remove(zipItem);
                zipItem = null;
            }
        },
        error: function (data) {
            var ok = $.parseJSON(data);
        }
    });

}

function ScaricaFile(e) {
    e.preventDefault();
    var data = this.dataItem($(e.currentTarget).closest("tr"));
    var nomefile = data.NomeFile;
    var idAllegato = $("#IdAllegato").val()
    var obj = {
        IdAllegato: idAllegato,
        NomeFile: nomefile
    }
    $.ajax({
        url: UrlActions.ZipView_ApriFile,
        method: 'GET',
        xhrFields: {
            responseType: 'blob'
        },
        data: obj,
        success: function (data) {
            var a = document.createElement('a');
            var url = window.URL.createObjectURL(data);
            a.href = url;
            a.download = nomefile;
            document.body.append(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        }
    });
}


function GetLogsItem() {

    return {
        IdItem: idItem,
        TipoItem: TipiOggetto.ALLEGATO

    };
}


//errori

function error_handler(e) {
    if (e.errors) {
        var message = "Errors:\n";
        $.each(e.errors, function (key, value) {
            if ('errors' in value) {
                $.each(value.errors, function () {
                    message += this + "\n";
                });
            }
        });
        alert(message);
    }
}
function ApriAllegato(e) {
    //chiamata alal view della preview immagine
    var data = this.dataItem($(e.currentTarget).closest("tr"));
    var nomefile = data.NomeFile;
    var idAllegato = $("#IdAllegato").val();
    var obj = {
        IdAllegato: idAllegato,
        NomeFile: nomefile,
        IsRelated: true
    }
    $.ajax({
        url: UrlActions.ImagePreview,
        method: 'POST',
        contentType: "application/json",
        accepts: "application/json",
        dataType: 'json',
        data: JSON.stringify(obj),
        success: function (data) {
            var myWindow = window.open("", "_blank");
            myWindow.document.write(data.responseText);
        },
        error: function (data) {
            var myWindow = window.open("", "_blank");
            myWindow.document.write(data.responseText);
        }

    });
}
function onDataBoundAttachments(e) {
    // mostra il bottone anteprima immagine solo se il record è un file immagine
    var grid = $("#zipAttachments").data("kendoGrid");
    var gridData = grid.dataSource.view();

    for (var i = 0; i < gridData.length; i++) {
        var currentUid = gridData[i].uid;
        if (gridData[i].NomeFile != undefined) {
            if (!(gridData[i].NomeFile.toLowerCase().includes(".jpg") || gridData[i].NomeFile.toLowerCase().includes(".png"))) {

                var currenRow = grid.table.find("tr[data-uid='" + currentUid + "']");
                var editButton = $(currenRow).find(".k-grid-anteprimaImg");
                editButton.hide();
            }
        }
    }
}