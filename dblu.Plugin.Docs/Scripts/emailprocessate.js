
var UrlActions = null;
var NomeServer = null;
var idItem = "";
var TipoAll = null;
var TipiOggetto = null;
var mailItem = null;
// eventi
gridEmailOnChange = function (e) {
    var data = this.dataItem(this.select());
    mailItem = data;
    PulisciDettaglio();

    $("#IdAllegato").val(data.Id);

    $.ajax({
        url: UrlActions.MailView_InArrivoCaricaDettaglio,
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

function InoltraMail() {
    if (mailItem != null) {
        $("#IdAllegato").val(mailItem.Id);

        var dialog = $("#wInoltra").data("kendoWindow");
        dialog.center().open();
    }
} 
function RiapriMail() {
    if (mailItem != null) {
        $("#IdAllegato").val(mailItem.Id);
        var obj = {
            IdAllegato: $("#IdAllegato").val()
        };
        $.ajax({
            url: UrlActions.MailView_Processate_Riapri,
            type: 'POST',
            cache: false,
            data: obj,
            success: function (data) {
                var ok = $.parseJSON(data);
                if (ok) {
                    $("#gridEmail").data("kendoGrid").dataSource.read();
                }
            },
            error: function (data) {
                var ok = $.parseJSON(data);
            }
        });
    }
} 
//function CancellaMail() {
//    if (mailItem != null) {
//        $("#dialog").data("kendoDialog").open();
//    }
//}
function onAnnulla() {
    $("#dialog").data("kendoDialog").close();
}
function onElimina() {
    var grid = $("#gridEmail").data("kendoGrid").dataSource.remove(mailItem);
}
function onInitOpen(e) {
  
}

function onOpen(e) {
    
}

function onShow(e) {
   
}

function onHide(e) {
    
}

function onClose(e) {
    $("#showDialogBtn").fadeIn();
  
}

function gridEmailOnRemove(e) {
    e.preventDefault();

    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    $("#IdAllegato").val(dataItem.Id);
    var dialog = $("#wCancella").data("kendoDialog");
    dialog.open();
}

function onSelectServer(e) {

    if (e.item) {
        var dataItem = this.dataItem(e.item.index());
        NomeServer = dataItem.Nome;
        $("#gridEmail").data("kendoGrid").dataSource.read();
    }
}

function Attachments_OnRowSelect(arg) {
    var data = this.dataItem(this.select());
    if (data != null) {

        var nomefile = data.NomeFile;
        var IdAllegato = $("#IdAllegato").val();

        try {
        }
        catch (err) {
            alert(err);
        }
    }

}

function inoltraOnClick(e) {
    e.preventDefault();
    var listMail = null;
    listMail = $("#multiMailInoltro").val().toString();

    var obj = {
        IdAllegato: $("#IdAllegato").val(),
        email: listMail
    };
    $.ajax({
        url: UrlActions.MailView_InArrivo_Inoltra,
        type: 'POST',
        cache: false,
        data: obj,
        success: function (data) {
            var ok = $.parseJSON(data);
            if (ok) {
                var dialog = $("#wInoltra").data("kendoWindow");
                dialog.close();
                //PulisciDettaglio();
                //var grid = $("#gridEmail").data("kendoGrid");
                //grid.dataSource.read();
            }
        },
        error: function (data) {
            var ok = $.parseJSON(data);
        }
    });
}

function wCancellaOk1() {
    wCancellaOk(false);
}


function wCancellaOk2() {
    wCancellaOk(true);
}

function wCancellaOk(fl) {

    var obj = {
        IdAllegato: $("#IdAllegato").val(),
        EliminaDaServer: fl
    };
    $.ajax({
        url: UrlActions.MailView_Processate_Cancella,
        type: 'POST',
        cache: false,
        data: obj,
        success: function (data) {
            //var ok = $.parseJSON(data);
            //if (ok) {
                //var dialog = $("#wCancella").data("kendoDialog");
                //dialog.close();
                var grid = $("#gridEmail").data("kendoGrid");
                grid.dataSource.read();
            //}
        },
        error: function (data) {
            var ok = $.parseJSON(data);
        }
    });
}

function wCancellaIgnora(e) {
     var dialog = $("#wCancella").data("kendoDialog");
    dialog.close();
}


function RiapriEmail(e) {
    e.preventDefault();

    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    $("#IdAllegato").val(dataItem.Id);
    var obj = {
        IdAllegato: $("#IdAllegato").val()
    };
    $.ajax({
        url: UrlActions.MailView_Processate_Riapri,
        type: 'POST',
        cache: false,
        data: obj,
        success: function (data) {
            var ok = $.parseJSON(data);
            if (ok) {
                $("#gridEmail").data("kendoGrid").dataSource.read();
            }
        },
        error: function (data) {
            var ok = $.parseJSON(data);
        }
    });

}

function viewHistory(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    idItem = dataItem.Id;
    $('#logs').data('kendoGrid').dataSource.read();
    var dialog = $("#wHistory").data("kendoWindow");
    dialog.center();
    dialog.open();

}

function GetLogsItem() {

    return {
        IdItem: idItem,
        TipoItem: TipiOggetto.ALLEGATO

    };
}

// Azioni

function ScaricaAllegato(e) {
    e.preventDefault();
    var data = this.dataItem($(e.currentTarget).closest("tr"));
    var nomefile = data.NomeFile;
    var idAllegato = $("#IdAllegato").val()
    var obj = {
        IdAllegato: idAllegato,
        NomeFile: nomefile
    }
    $.ajax({
        url: UrlActions.MailView_ApriFile,
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

function getEmails() {
    var server = "";
    if (NomeServer != null) {
        server = NomeServer;
    }
    else
        server = $("#emailServer").data("kendoComboBox").value();
    ;
    return {
        Tipo: TipoAll.Codice,
        NomeServer: server
    };
}

function getSoggetto() {
    var Soggetto = $("#CodiceSoggetto").val();
    return {
        CodiceSoggetto: Soggetto
    };
}

function InoltraEmail(e) {
    e.preventDefault();

    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    $("#IdAllegato").val(dataItem.Id);

    var dialog = $("#wInoltra").data("kendoWindow");
    dialog.center().open();

}



function CancellaEmail() {
    //e.preventDefault();   
    //var dataItem = this.dataItem($(e.currentTarget).closest("tr"));

    if (mailItem != null) {
        $("#IdAllegato").val(mailItem.Id);

        var dialog = $("#wCancella").data("kendoDialog");
        dialog.open();
    }
}

// Dettaglio

function PulisciDettaglio() {
    $("#IdAllegato").val("");
    $('#IdFascicolo').val("");
    $('#IdElemento').val("");
    $("#TestoEmail").val("");
    $('#emailAttachments').data('kendoGrid').dataSource.data("{}")
    $('#divFascicolo').hide();
    $('#divElemento').hide();
    $('#ApriDettaglio').hide();
    $('#AggiungiAElemento').hide();
    $('#Completa').hide();
}

function MostraDettaglio(dettaglio) {
    $("#TestoEmail").val(dettaglio.TestoEmail);

    var soggettoPrec = $('#CodiceSoggetto').val();
    $('#CodiceSoggetto').val(dettaglio.CodiceSoggetto);
    $('#NomeSoggetto').val(dettaglio.NomeSoggetto);


    $('#IdFascicolo').val(dettaglio.IdFascicolo);
    $('#IdElemento').val(dettaglio.IdElemento);
    //alert(dettaglio.DescrizioneElemento);
    $('#DescrizioneElemento').val(dettaglio.DescrizioneElemento);
    if (dettaglio.IdElemento != '') {
        $('#divElemento').show();
        //$('#ApriDettaglio').show();
        //$('#AggiungiAElemento').show();
        //alert(dettaglio.Stato);
        if (dettaglio.Stato > 1) {
            $('#Completa').show();
        }
    }
    $('#emailAttachments').data('kendoGrid').dataSource.data(dettaglio.FileAllegati);
}
function RispondiEmail() {
    if (mailItem != null) {
        $("#IdAllegato").val(mailItem.Id);
        $("#destinatarioRisposta").val(mailItem.Chiave1);
        $("#ccRisposta").val("");
        $("#oggettoRisposta").val("R: " + mailItem.Descrizione);
        $("#testoRisposta").val("");
        var dialog = $("#wRispondi").data("kendoWindow");

        dialog.center().open();

    }
}
function rispondiOnClick(e) {
    e.preventDefault();
    var server = "";
    if (NomeServer != null) {
        server = NomeServer;
    }
    else {
        server = $("#emailServer").data("kendoComboBox").value();
    };


    var obj = {
        IdAllegato: $("#IdAllegato").val(),
        NomeServer: server,
        to: $("#destinatarioRisposta").val(),
        cc: $("#ccRisposta").val(),
        oggetto: $("#oggettoRisposta").val(),
        testo: $("#testoRisposta").val(),
        allegaEmail: $("#chkemailRispondi").is(':checked'),
        chiudiEmail: $("#chkemailRispondiChiudi").is(':checked')
    };
    $.ajax({
        url: UrlActions.MailView_InArrivo_Rispondi,
        type: 'POST',
        cache: false,
        data: obj,
        success: function (data) {
            var ok = $.parseJSON(data);
            if (ok) {
                var dialog = $("#wRispondi").data("kendoWindow");
                dialog.close();
                var grid = $("#gridEmail").data("kendoGrid");
                gridRefresLastOp(grid, docsTipiOperazioni.RISPOSTO);
            }
        },
        error: function (data) {
            var ok = $.parseJSON(data);
        }
    });
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
    var grid = $("#emailAttachments").data("kendoGrid");
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

function ValidMail() {
    return {
        AsValidEmail: true,
    };
}

function onmultiMailInoltroDataBound(e) {
    $('.k-multiselect .k-input').unbind('keyup');
    $('.k-multiselect .k-input').on('keyup', onClickEnterInoltra);
}

function onClickEnterInoltra(e) {
    e.preventDefault();
    if (e.keyCode === 13) {
        var input = $(".k-multiselect .k-input[aria-describedby='multiMailInoltro_taglist'] ");
        var value = input.val().trim();
        var widget = $("#multiMailInoltro").getKendoMultiSelect();
        var dataSource = widget.dataSource;
        if (!value || value.length === 0) {
            return;
        }
        dataSource.add({
            Email: value,
            Name: null,
            LastName: null
        });
        dataSource.sync();
    }
}

function onmulticcRispostaDataBound(e) {
    $('.k-multiselect .k-input').unbind('keyup');
    $('.k-multiselect .k-input').on('keyup', onClickEnterRispondi);
}

function onClickEnterRispondi(e) {
    e.preventDefault();
    if (e.keyCode === 13) {
        var input = $(".k-multiselect .k-input[aria-describedby='multiccRisposta_taglist'] ");
        var value = input.val().trim();
        var widget = $("#multiccRisposta").getKendoMultiSelect();
        var dataSource = widget.dataSource;
        if (!value || value.length === 0) {
            return;
        }
        dataSource.add({
            Email: value,
            Name: null,
            LastName: null
        });
        dataSource.sync();
    }
}

$(function () {
    $('#formInoltra').on('keydown', 'input', function (event) {
        if (event.which == 13) {
            event.preventDefault();
        }
    });

    $('#formRispondi').on('keydown', 'input', function (event) {
        if (event.which == 13) {
            event.preventDefault();
        }
    });
})