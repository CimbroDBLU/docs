
// variabili
var NomeRuolo = null;

var UrlActions = null;

 //  parametri 

function FascicoliData() {
    return {
        soggetto: $("#CodiceSoggetto").val()
    }
}

function getSoggetto() {
    var Soggetto = $("#CodiceSoggetto").val();
    return {
        CodiceSoggetto: Soggetto
    };
}

function getFascicolo() {
    var IdFascicolo = $("#IdFascicolo").val();
    return {
        IdFascicolo: IdFascicolo
    };
}

// tipoZip

function onSelectRuoloZip(e) {

    if (e.item) {
        var dataItem = this.dataItem(e.item.index());
        NomeRuolo = dataItem.Value;
        //alert(NomeRuolo);

        PulisciDettaglio();
        $("#gridZip").data("kendoGrid").dataSource.read();

    }
}

function getRuolo() {
    var ruolo = "";
    if (NomeRuolo != null) {
        ruolo = NomeRuolo;
    }
    else
        ruolo = $("#cmbRuoliZip").data("kendoComboBox").value();
    ;
    //alert(ruolo);
    return {
        Ruolo: ruolo
    };
}

// grid zip

gridZipOnChange = function (e) {
    var data = this.dataItem(this.select());
    PulisciDettaglio();

    $("#IdTask").val(data.id);

    $.ajax({
        url: UrlActions.ZipView_CaricaDettaglio,
        type: 'POST',
        cache: false,
        data: { Id: data.id },
        success: function (data) {
            var dettaglio = data;
            //alert(dettaglio.IdAllegato);
            MostraDettaglio(dettaglio);
            MostraPdfCompleto();
        },
        error: function (data) {
        }
    });

}

function gridZipOnRemove(e) {
    //$("#IdAllegato").val(e.model.Id);

    //var obj = {
    //    IdTask: $("#IdTask").val(),
    //    IdAllegato: $("#IdAllegato").val()
    //};
    //alert(e.model.id);

    $.ajax({
        url: UrlActions.ZipView_ZipFileAnnulla,
        type: 'POST',
        cache: false,
        data: { IdTask: e.model.id },
        success: function (data) {
            var ok = $.parseJSON(data);
            PulisciDettaglio();

        },
        error: function (data) {
            var ok = $.parseJSON(data);

        }
    });
}


function InoltraFile(e) {
    e.preventDefault();

    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    $("#IdAllegato").val(dataItem.Id);

    var dialog = $("#wInoltra").data("kendoWindow");

    dialog.open();
}

function SpostaZip(e) {

    e.preventDefault();

    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    $("#IdAllegato").val(dataItem.Id);

    var dialog = $("#wSposta").data("kendoWindow");
    dialog.center().open();
}



// dettaglio

function PulisciDettaglio() {
    $("#IdTask").val("");
    $("#IdAllegato").val("");
    $('#IdFascicolo').val("");
    $('#IdElemento').val("");

    $('#CodiceSoggetto').val("");
    $('#NomeSoggetto').val("");

    $("#TestoEmail").val("");
    $('#zipAttachments').data('kendoGrid').dataSource.data("{}")
    $('#divFascicolo').hide();

    $('#divElemento').hide();
     $('#Completa').hide();
    $('#StampaRiepilogo').hide();
    try {
        $('#gridZipElementi').data('kendoGrid').dataSource.data("{}");
    }
    catch (err) {

    }
    try {
        var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
        zippdfviewer.load('');
    }
    catch (err) {

    }

}
function MostraDettaglio(dettaglio) {

    $('#IdAllegato').val(dettaglio.IdAllegato);

    var soggettoPrec = $('#CodiceSoggetto').val();
    $('#CodiceSoggetto').val(dettaglio.CodiceSoggetto);
    $('#NomeSoggetto').val(dettaglio.NomeSoggetto);

    if (dettaglio.CodiceSoggetto != '') {
        $('#divFascicolo').show();
        if (dettaglio.CodiceSoggetto != soggettoPrec) {
            $('#gridSoggettoElementiAperti').data('kendoGrid').dataSource.read();
            $('#divSoggettoElementiAperti').show();
        }
    }
    else {
        $('#divSoggettoElementiAperti').hide();
        $('#gridSoggettoElementiAperti').data('kendoGrid').dataSource.data("{}");
    };

    $('#IdFascicolo').val(dettaglio.IdFascicolo);
    $('#IdElemento').val(dettaglio.IdElemento);
    $('#DescrizioneElemento').val(dettaglio.DescrizioneElemento);

    if (dettaglio.IdElemento != '') {
        $('#divElemento').show();
         if (dettaglio.Stato > 1) {
            $('#Completa').show();
            $('#StampaRiepilogo').show();
        }
    }
    $('#zipAttachments').data('kendoGrid').dataSource.data(dettaglio.FileAllegati);
    $('#gridZipElementi').data('kendoGrid').dataSource.read();
}

function MostraPdfCompleto() {
    var IdAllegato = $("#IdAllegato").val();
    var IdElemento = $("#IdElemento").val();
    //alert(IdAllegato);
    //alert(IdElemento);
    var nome = 'zip-contenuto.pdf';
    var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
    var param = IdAllegato + ";" + IdElemento;
    if (nome.indexOf(".pdf") > 0) {
        zippdfviewer.load(param);
        zippdfviewer.downloadFileName = nome;
    }
    else {
        zippdfviewer.load('');
    }

}

function CaricaElemento(elemento) {
    $('#IdFascicolo').val(elemento.IdFascicolo);
    $('#IdElemento').val(elemento.Id);
    $('#DescrizioneElemento').val(elemento.Descrizione);
    $('#divElemento').show();

    $("#divFascicolo").find(":input").prop("disabled", true);
    $("#CollapseFascicolo").prop("disabled", false);
    $("#CollapseFascicolo").click();

    var dialog = $("#detElemento").data("kendoWindow");
    $.ajax({
        url: UrlActions.ZipView_editDettaglioElemento,
        type: 'POST',
        data: { IdElemento: $("#IdElemento").val() },
        success: function (data) {
            $("#IdElemento").val(data.Id);
            dialog.content(data);
            dialog.open();

        }
    });
}
function ApriElementoGrid(e) {
    e.preventDefault();
    $('body').addClass('waiting');
    $("#divFascicolo").find(":input").prop("disabled", true);
    $("#CollapseFascicolo").prop("disabled", false);
    $("#CollapseFascicolo").click();
    var data = this.dataItem($(e.currentTarget).closest("tr"));

    var dialog = $("#detElemento").data("kendoWindow");
    $.ajax({
        url: UrlActions.ZipView_editDettaglioElemento,
        type: 'POST',
        data: { IdElemento: data.Id },
        success: function (data) {
            $("#IdElemento").val(data.Id);
            dialog.content(data);
            dialog.open();

        }
    });
}

function AggiungiAElementoGrid(e) {
    e.preventDefault();
    var data = this.dataItem($(e.currentTarget).closest("tr"));

    var idElemento = data.Id;
    var IdElementoCorrente = $("#IdElemento").val();
    if (idElemento == IdElementoCorrente) {
        alert("La mail è già stata aggiunta a questo elemento.")
    }
    else {
        $('body').addClass('waiting');

        var gridall = $("#zipAttachments").data("kendoGrid");
        var items = '';
        var selectedElements = gridall.select();
        for (var j = 0; j < selectedElements.length; j++) {
            var item = gridall.dataItem(selectedElements[j]);
            items = items + item.NomeFile + ';';
        }
        var r = false;
        if (idElemento != "") {
            r = confirm("Confermi l'aggiunta all'elemento corrente?");
        }
        var obj = {
            IdAllegato: $("#IdAllegato").val(),
            IdFascicolo: $("#IdFascicolo").val(),
            IdElemento: idElemento,
            elencoFile: items,
            AllegaEmail: true,
            Descrizione: $("#DescrizioneElemento").val()
        };
        $.ajax({
            url: UrlActions.ZipView_AllegaAElementoFascicolo,
            type: 'POST',
            data: obj,
            success: function (res) {
                if (res) {
                    $("#IdElemento").val(data.Id);
                    $('#Completa').show();
                    $('#StampaRiepilogo').show();
                    MostraPdfCompleto();
                }
            },
            error: function () {

            }
        });

        $('body').removeClass('waiting');
    }
}


function RimuoviElementoGrid(e) {
    e.preventDefault();
    var data = this.dataItem($(e.currentTarget).closest("tr"));
    if (data.Stato > 1) {
        alert("Non è possibile eliminare un elemento già processato.");
    }
    else {

        $('body').addClass('waiting');

        var idElemento = data.Id;

        if (idElemento != "") {
            r = confirm("Confermi la cancellazione all'elemento corrente?");
        }
        if (r) {
            var obj = {
                IdElemento: idElemento,
                Revisione: data.Revisione
            };
            $.ajax({
                url: UrlActions.ZipView_CancellaElemento,
                type: 'POST',
                data: obj,
                success: function (res) {
                    if (res) {
                        var gridEl = $("#gridZipElementi").data("kendoGrid");
                        gridEl.dataSource.read();
                    }
                },
                error: function () {

                }
            });
        }
        $('body').removeClass('waiting');
    }
}


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

function ApriDettaglioOnClick(e) {
    e.preventDefault();
    $('body').addClass('waiting');
    //$("#divFascicolo *").prop('disabled', true);
    $("#divFascicolo").find(":input").prop("disabled", true);

    var dialog = $("#detElemento").data("kendoWindow");

    $.ajax({
        url: UrlActions.ZipView_editDettaglioElemento,
        type: 'POST',
        data: { IdElemento: $("#IdElemento").val() },
        success: function (data) {
            $("#IdElemento").val(data.Id);
            dialog.content(data);
            dialog.open();

        }
    });
}

function detElementoOpen(e) {
    $('body').removeClass('waiting');
}

function detElementoClose(e) {

    $("#divFascicolo").find(":input").prop("disabled", false);
    $("#CollapseFascicolo").click();
    var grid = $("#gridZip").data("kendoGrid"),
        rows = grid.select();

    grid.select(rows[0]);

}

function wCercaElementiClose(e) {
    $('#gridZipElementi').data('kendoGrid').dataSource.read();
}

function AggiungiAElementoOnClick(e) {

    $('body').addClass('waiting');

    var idElemento = $('#IdElemento').val();
    var gridall = $("#zipAttachments").data("kendoGrid");
    var items = '';
    var selectedElements = gridall.select();
    for (var j = 0; j < selectedElements.length; j++) {
        var item = gridall.dataItem(selectedElements[j]);
        items = items + item.NomeFile + ';';
    }
    var r = false;
    if (idElemento != "") {
        r = confirm("Confermi l'aggiunta all'elemento corrente?");
    }
    if (r) {
        var obj = {
            IdAllegato: $("#IdAllegato").val(),
            IdFascicolo: $("#IdFascicolo").val(),
            IdElemento: idElemento,
            elencoFile: items,
            AllegaEmail: true,
            Descrizione: $("#DescrizioneElemento").val()
        };
        $.ajax({
            url: UrlActions.ZipView_AllegaAElementoFascicolo,
            type: 'POST',
            data: obj,
            success: function (res) {
                if (res) {
                    $('#Completa').show();
                    $('#StampaRiepilogo').show();
                }
            },
            error: function () {

            }
        });
    }
    $('body').removeClass('waiting');
}


function StampaRiepilogo(e) {
    var IdAllegato = $("#IdAllegato").val();
    //var url = '@Url.Action("StampaRiepilogo","MailView" )' + '?IdAllegato=' + $("#IdAllegato").val();
    //alert(url);
    var url = UrlActions.ZipView_StampaRiepilogo + '?IdAllegato=' + $("#IdAllegato").val();
    window.open(url, 'Riepilogo');

}



// Soggetto

function CodiceSoggettoOnChange() {
    $.ajax({
        url: UrlActions.ZipView_GetSoggetto,
        type: 'GET',
        data: { codice: $('#CodiceSoggetto').val() },
        success: function (data) {
            $("#CodiceSoggetto").val(data.Codice);
            $("#NomeSoggetto").val(data.Nome);
            CaricaSoggetto(data.Codice);
            //NotificaAssociazione(data.Codice);
        }
    });

}

function NotificaAssociazione(Soggetto) {
    IdAllegato = $("#IdAllegato").val();
    $.ajax({
        url: UrlActions.ZipView_NotificaAssociazione,
        type: 'POST',
        cache: false,
        data: { CodiceSoggetto: Soggetto, IdAllegato: IdAllegato },
        success: function (data) {

        },
        error: function (data) {

        }
    });
}

function CercaSoggettiOnClick(e) {
    var myWindow = $("#cercasoggetti").data("kendoWindow");
    myWindow.title("Cerca Soggetti");
    myWindow.refresh({
        url: UrlActions.sggservice_UrlServizio,
        type: "Post"
    });
    myWindow.open();
}

function CaricaSoggetto(codice) {
    if (codice == '') {
        $('#divFascicolo').hide();
        $('#divSoggettoElementiAperti').hide();
    }
    else {
        $('#divFascicolo').show();
        $('#divSoggettoElementiAperti').show();
        $('#gridSoggettoElementiAperti').data('kendoGrid').dataSource.read();

    }
}

GridSoggetti_OnRowSelect = function (e) {
    var data = this.dataItem(this.select());
    var myWindow = $("#cercasoggetti").data("kendoWindow");
    myWindow.close();
    $('#CodiceSoggetto').val(data.Codice);
    $('#NomeSoggetto').val(data.Nome);

    CaricaSoggetto(data.Codice);
    NotificaAssociazione(data.Codice);
}




// zip elementi
function gridZipElementiOnChange(e) { }

function CercaElementiOnClick(e) {

    var myWindow = $("#wCercaElementi").data("kendoWindow");
    var codSoggetto = $('#CodiceSoggetto').val();
    myWindow.title("Cerca Elementi");
    myWindow.refresh({
        url: UrlActions.sggservice_UrlServizioRicercaElementi,
        type: "Post",
        data: { soggetto: codSoggetto }
    });
    myWindow.open().maximize();
}

GridElementi_OnRowSelect = function (e) {
    var data = this.dataItem(this.select());
    var myWindow = $("#wCercaElementi").data("kendoWindow");
    myWindow.close();
    $("#IdFascicolo").val($.trim(data.IdFascicolo));
    $("#IdElemento").val($.trim(data.IdElemento));
    $("#DescrizioneElemento").val($.trim(data.DscElemento));
}

//allegati

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

// azioni

function ZipCompleto(e) {

    var IdTask = $("#IdTask").val();
    var IdAllegato = $("#IdAllegato").val();
    $.ajax({
        url: UrlActions.ZipView_ZipFileCompleto,
        type: 'POST',
        cache: false,
        data: { IdTask: IdTask , IdAllegato: IdAllegato },
        success: function (data) {
            var ok = $.parseJSON(data);
            if (ok) {
                PulisciDettaglio();
                var grid = $("#gridZip").data("kendoGrid");
                grid.dataSource.read();
            }
        },
        error: function (data) {
            var ok = $.parseJSON(data);

        }
    });
}


// pdf 

function importAnnotations() {

    var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
    zippdfviewer.importAnnotation('');
}

function annotationAdd(e) {

}

function saveAnnotations() {

    var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
    zippdfviewer.exportAnnotation();
}

function documentLoaded(args) {
    importAnnotations()
}