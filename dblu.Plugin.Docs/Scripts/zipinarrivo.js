
// variabili
var NomeCartella = null;

var UrlActions = null;

var zipItem = null;
var idItem = "";
var tipoItem = null;
var elementoItem = null;
var IdNuovoElemento = null;
//  parametri 

var PdfCorrente = {
    TipoAllegato: "ZIP",
    IdAllegato: "",
    IdElemento: "",
    FilePdf: "",
    Pagina: 0,
    AggiungiFilePdf: "",
    NuovaPosizione: 0,
    iAzione: 0,
    IdAllegatoAElemento: "",
    Descrizione: "",
    FileAllegati: null,
    Printer: ''
}


// elenco file
function onSelectCartella(e) {

    if (e.item) {
        var dataItem = this.dataItem(e.item.index());
        NomeCartella = dataItem.Cartella;
        //alert(NomeCartella);
        PulisciDettaglio();
        $("#gridZip").data("kendoGrid").dataSource.read();

    }
}
// full pdf Ctr+Maiusc
$(document).keydown(function (e) {
    if (e.keyCode == 16 && e.ctrlKey) {
        var emailpdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
        //var gridEl = $("#gridemailElementi").data("kendoGrid");

        PdfCorrente.IdAllegato = $("#IdAllegato").val();
        //PdfCorrente.IdElemento = $("#IdElemento").val();
        PdfCorrente.Pagina = emailpdfviewer.currentPageNumber;
        var pdfWindow = $("#wPdfEditor").data("kendoWindow");
        pdfWindow.refresh({
            url: UrlActions.Pdf_Editor,
            type: "Post",
            data: { pdf: JSON.stringify(PdfCorrente) }
        });
        pdfWindow.open().maximize();
    }
}); 

function getOrigine() {
    var cartella = "";
    if (NomeCartella != null) {
        cartella = NomeCartella;
    }
    else
        cartella = $("#cmbCartelleZip").data("kendoDropDownList").value();
    ;
    //alert(cartella);
    return {
        Tipo: TipoAll.Codice,
        Origine: cartella
    };
}


//gridZip

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

function GetLogsItem() {

    return {
        IdItem: idItem,
        TipoItem: tipoItem
    };
}

function gridZipOnChange(e) {
    var data = this.dataItem(this.select());
    zipItem = data;
    elementoItem = null;
  
    PulisciDettaglio();

    $("#IdAllegato").val(data.id);

    $.ajax({
        url: UrlActions.ZipView_CaricaDettaglio,
        type: 'POST',
        cache: false,
        data: { Id: data.id },
        success: function (data) {
            var dettaglio = data;
           
            PdfCorrente.Descrizione = "  Contenuto zip " 
            MostraDettaglio(dettaglio);
            if (IdNuovoElemento != null) {

                $("#IdElemento").val(IdNuovoElemento);
                IdNuovoElemento = null;
            }
            else {
                $("#IdElemento").val("");
            }
            MostraPdfCompleto($("#IdElemento").val());
        },
        error: function (data) {
        }
    });
}


function CancellaZip(e) {
    e.preventDefault();
    if (zipItem != null) {
        $("#dialogElimina").data("kendoDialog").open();
    }
}


function onAnnullaElimina() {
    if (zipItem != null) {
        $("#dialogElimina").data("kendoDialog").close();
    }
}

function onConfermaElimina() {
    var grid = $("#gridZip").data("kendoGrid");

    $("#IdAllegato").val(zipItem.Id);

    $.ajax({
        url: UrlActions.ZipView_InArrivo_Cancella,
        type: 'POST',
        cache: false,
        data: { Id: zipItem.Id },
        success: function (data) {

            //var ok = $.parseJSON(data);
            PulisciDettaglio();
            grid.dataSource.remove(zipItem);
            zipItem = null;
        },
        error: function (data) {
            var ok = $.parseJSON(data);
        }
    });

}

function SpostaZip(e) {
    e.preventDefault();
    if (zipItem != null) {
        $("#IdAllegato").val(zipItem.Id);
        var dialog = $("#wSposta").data("kendoWindow");
        dialog.center().open();
    }
}

function spostaOnClick(e) {
    e.preventDefault();
    var obj = {
        IdAllegato: $("#IdAllegato").val(),
        Cartella: $("#zipServer_sposta").data("kendoDropDownList").value()
    };
    $.ajax({
        url: UrlActions.ZipView_InArrivo_Sposta,
        type: 'POST',
        cache: false,
        data: obj,
        success: function (data) {
            var ok = $.parseJSON(data);
            if (ok) {
                var dialog = $("#wSposta").data("kendoWindow");
                dialog.close();
                PulisciDettaglio();
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



// dettaglio

function PulisciDettaglio() {

    $("#IdAllegato").val("");
    $('#IdFascicolo').val("");
    $('#IdElemento').val("");

    $('#CodiceSoggetto').val("");
    $('#NomeSoggetto').val("");

    $("#TestoEmail").val("");
    try {
        $('#zipAttachments').data('kendoGrid').dataSource.data("{}")
    }
    catch (err) { }
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
        zippdfviewer.fileName = '';
        zippdfviewer.load('');
    }
    catch (err) {

    }

}
function MostraDettaglio(dettaglio) {

    //$('#IdAllegato').val(dettaglio.IdAllegato);

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

    if (dettaglio.IdFascicolo != '') {
        $('#divElemento').show();
        if (dettaglio.StatoZip > 1) {
            $('#Completa').show();
            $('#StampaRiepilogo').show();
        }
    }
    //$('#zipAttachments').data('kendoGrid').dataSource.data(dettaglio.FileAllegati);
    try {
        $('#zipAttachments').data('kendoGrid').dataSource.data("{}");
    } catch (err) { }
    try {
        $('#gridZipElementi').data('kendoGrid').dataSource.read();
    } catch (err) { }
}

var loadingTimes = 0;

function MostraPdfCompleto(idElemento) {

    PdfCorrente.IdAllegato = $("#IdAllegato").val();
    PdfCorrente.IdElemento = idElemento; // $("#IdElemento").val();
    PdfCorrente.iAzione = docsAzioniPdf.Carica;
    var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
   
    PdfCorrente.Pagina = 1;   
    zippdfviewer.fileName = JSON.stringify(PdfCorrente);
    if (loadingTimes++ != 0)
        zippdfviewer.magnification.zoomTo(110);

    zippdfviewer.load(JSON.stringify(PdfCorrente));
    $("#tbdescrizione").html(PdfCorrente.Descrizione);
    zippdfviewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";
}


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
    var IdAllegato = $("#IdAllegato").val();
    return {
        IdFascicolo: IdFascicolo,
        IdAllegato: IdAllegato
    };
}

// tipoZip



// grid zip



//function InoltraFile(e) {
//    e.preventDefault();

//    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
//    $("#IdAllegato").val(dataItem.Id);

//    var dialog = $("#wInoltra").data("kendoWindow");

//    dialog.open();
//}


function CaricaElemento(elemento) {
    $('#IdFascicolo').val(elemento.IdFascicolo);
    $('#IdElemento').val(elemento.Id);
    IdNuovoElemento = elemento.Id;
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
    if (elementoItem != null) {
        $('body').addClass('waiting');
        $("#divFascicolo").find(":input").prop("disabled", true);
        $("#CollapseFascicolo").prop("disabled", false);
        $("#CollapseFascicolo").click();

        var dialog = $("#detElemento").data("kendoWindow");
        $.ajax({
            url: UrlActions.ZipView_editDettaglioElemento,
            type: 'POST',
            data: { IdElemento: elementoItem.Id },
            success: function (data) {
                $("#IdElemento").val(elementoItem.Id);
                dialog.content(data);
                dialog.open();

            }
        });
    }
}

function AggiungiAElementoGrid(e) {
    e.preventDefault();
    if (elementoItem != null) {

        var idElemento = elementoItem.Id;
        var IdElementoCorrente = $("#IdElemento").val();
        if (idElemento == IdElementoCorrente) {
            alert("Il documento è già stato aggiunto a questo elemento.")
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
            if (r) {
                $("#IdElemento").val(idElemento);
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
                            //$("#IdElemento").val(data.Id);
                            $('#Completa').show();
                            $('#StampaRiepilogo').show();
                            MostraPdfCompleto(idElemento);
                        }
                    },
                    error: function () {

                    }
                });
            }
            $('body').removeClass('waiting');
        }
    }
}


function CancellaElemento(e) {
    e.preventDefault();
    if (elementoItem != null) {
        if (elementoItem.Stato > 1) {
            alert("Non è possibile eliminare un elemento già processato.");
        }
        else {

            $('body').addClass('waiting');

            var idElemento = elementoItem.Id;

            if (idElemento != "") {
                r = confirm("Confermi la cancellazione all'elemento corrente?");
            }
            if (r) {
                var obj = {
                    IdElemento: elementoItem.Id,
                    Revisione: elementoItem.Revisione
                };
                $.ajax({
                    url: UrlActions.ZipView_CancellaElemento,
                    type: 'POST',
                    data: obj,
                    success: function (res) {
                        if (res) {
                            var gridEl = $("#gridZipElementi").data("kendoGrid");
                            //gridEl.dataSource.read();
                            gridEl.dataSource.remove(elementoItem);
                        }
                    },
                    error: function () {

                    }
                });
            }
            $('body').removeClass('waiting');
        }
    }
}

function NuovoElemento(e, Categoria, TipoElemento) {

    $('body').addClass('waiting');

    var idElemento = $('#IdElemento').val();
    var IdFascicolo = $("#IdFascicolo").val();
    var gridall = $("#zipAttachments").data("kendoGrid");
    var items = '';
    var selectedElements = gridall.select();
    for (var j = 0; j < selectedElements.length; j++) {
        var item = gridall.dataItem(selectedElements[j]);
        items = items + item.NomeFile + ';';
    }
    var r = true;
    if (IdFascicolo != "") {
        r = confirm("Confermi la creazione di un nuovo elemento nel fascicolo corrente?");
    }
    if (r) {
        var obj = {
            IdAllegato: $("#IdAllegato").val(),
            IdFascicolo: IdFascicolo,
            IdElemento: idElemento,
            Categoria: Categoria,
            TipoElemento: TipoElemento,
            CodiceSoggetto: $("#CodiceSoggetto").val(),
            NomeSoggetto: $("#NomeSoggetto").val(),
            elencoFile: items,
            AllegaZip: $("#allegamail").val(),
            Descrizione: $("#DescrizioneElemento").val(),
        };
        $.ajax({
            url: UrlActions.ZipView_CreaElementoFascicolo,
            type: 'POST',
            data: obj,
            success: function (elemento) {
                CaricaElemento(elemento);
            },
            error: function () {
                $('body').removeClass('waiting');
            }
        });
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

    $("#divSoggetto").CardWidget("collapse");
    $("#divFascicolo").CardWidget("collapse");
    $("#divAllegati").CardWidget("collapse");
}

function detElementoClose(e) {

    $("#divFascicolo").find(":input").prop("disabled", false);
   // $("#CollapseFascicolo").click();
    $("#divSoggetto").CardWidget("expand");
    $("#divFascicolo").CardWidget("expand");
    $("#divAllegati").CardWidget("expand");

    var grid = $("#gridZip").data("kendoGrid"),
        rows = grid.select();

    grid.select(rows[0]);

}

function wCercaElementiClose(e) {
    $('#gridZipElementi').data('kendoGrid').dataSource.read();
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
function gridZipElementiOnChange(e) {

    var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
    zippdfviewer.unload();
    var data = this.dataItem(this.select());
    if (data != null) {
        elementoItem = data;
        //$("#IdElemento").val(data.IdElemento);
        PdfCorrente.Descrizione = "  Allegato a: " + data.DscTipoElemento;
        MostraPdfCompleto(data.IdElemento);
    }
    else {
        $("#IdElemento").val("");
    }


}

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

    var IdAllegato = $("#IdAllegato").val();
    $.ajax({
        url: UrlActions.ZipView_ZipFileCompleto,
        type: 'POST',
        cache: false,
        data: { IdTask: null, IdAllegato: IdAllegato },
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
    zippdfviewer.importAnnotation();
}

function annotationAdd(e) {

}

function saveAnnotations() {

    var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
    zippdfviewer.exportFileName = JSON.stringify(PdfCorrente);
    zippdfviewer.exportAnnotation();
}



function documentLoaded(args) {

    var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
    zippdfviewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";

    //carico lista file allegati
    $.ajax({
        url: UrlActions.PdfEditor_GetPdfEditAction,
        type: 'POST',
        cache: false,
        data: { param: JSON.stringify(PdfCorrente) },
        success: function (data) {
            PdfCorrente = data;
            $('#zipAttachments').data('kendoGrid').dataSource.data(PdfCorrente.FileAllegati);
        },
        error: function (data) {
            zippdfviewer.load('');
        }
    });

    //riposiziono la pagina
    try {
        zippdfviewer.navigation.goToFirstPage();
        if (PdfCorrente.Pagina > 1) {
            zippdfviewer.navigation.goToPage(PdfCorrente.Pagina);
        }
    }
    catch (err) {

    }

    zippdfviewer.importAnnotation(JSON.stringify(PdfCorrente));
    zippdfviewer.magnification.zoomTo(100);
}


function documentPrint(e) {
    if (PdfCorrente.Printer != '') {
        PdfCorrente.iAzione = docsAzioniPdf.Stampa;
        if (elementoItem == null) {
            PdfCorrente.IdElemento = null;
        } else {
            PdfCorrente.IdElemento = elementoItem.Id;
        }

        $.ajax({
            url: UrlActions.ZipView_InArrivo_Stampa,
            type: 'POST',
            cache: false,
            data: { pdf: JSON.stringify(PdfCorrente) },
            success: function (data) {

                var grid = $("#gridZip").data("kendoGrid");
                gridRefresLastOp(grid, docsTipiOperazioni.STAMPATO);

            }
        });

        e.cancel = true;
    }
}

function documentPrinted() {

    $.ajax({
        url: UrlActions.ZipView_InArrivo_Stampato,
        type: 'POST',
        cache: false,
        data: { IdAllegato: $("#IdAllegato").val(), IdElemento: $("#IdElemento").val() },
        success: function (data) {

            var grid = $("#gridZip").data("kendoGrid");
            gridRefresLastOp(grid, docsTipiOperazioni.STAMPATO);

        }
    });
}


function exportSuccess(args) {

    if (PdfCorrente.iAzione == docsAzioniPdf.Salva) {

        var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
        zippdfviewer.load(JSON.stringify(PdfCorrente))
        zippdfviewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";

    }
}



function tbpdf_click(e) {

    var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];
    var gridEl = $("#gridZipElementi").data("kendoGrid");

    PdfCorrente.IdAllegato = $("#IdAllegato").val();
    PdfCorrente.Pagina = zippdfviewer.currentPageNumber;

    if (e.id == "tbruotadx") {
        PdfCorrente.iAzione = docsAzioniPdf.RuotaPagina90;
    } else if (e.id == "tbruotasx") {
        PdfCorrente.iAzione = docsAzioniPdf.RuotaPagina270;
    } else if (e.id == "tbcanc") {
        PdfCorrente.iAzione = docsAzioniPdf.CancellaPagina;
    } else if (e.id == "tbrefresh") {
        PdfCorrente.iAzione = docsAzioniPdf.Ricarica;
    } else if (e.id == "tbsalva") {
        PdfCorrente.iAzione = docsAzioniPdf.Salva;
    } else {
        PdfCorrente.iAzione = docsAzioniPdf.Carica;
    }

    if (e.id == "tbespandi") {

        var pdfWindow = $("#wPdfEditor").data("kendoWindow");
        pdfWindow.refresh({
            url: UrlActions.Pdf_Editor,
            type: "Post",
            data: { pdf: JSON.stringify(PdfCorrente) }
        });
        pdfWindow.open().maximize();;

    } else {

        if (PdfCorrente.iAzione == docsAzioniPdf.Salva && zippdfviewer.annotationCollection != undefined && zippdfviewer.annotationCollection.length > 0) {

            zippdfviewer.exportAnnotation(JSON.stringify(PdfCorrente));

        }
        else {
            zippdfviewer.load(JSON.stringify(PdfCorrente))
            zippdfviewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";
        }
    }
}

function wPdfEditorClose(e) {

    if (MustReloadPdf) {

        PdfCorrente.iAzione = docsAzioniPdf.Ricarica;
        var zippdfviewer = document.getElementById('zippdfviewer').ej2_instances[0];

        zippdfviewer.load(JSON.stringify(PdfCorrente));
        zippdfviewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";

    }

}


// log

function viewHistory(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    idItem = dataItem.Id;
    tipoItem = TipiOggetto.ALLEGATO
    $('#logs').data('kendoGrid').dataSource.read();
    var dialog = $("#wHistory").data("kendoWindow");
    dialog.center();
    dialog.open();

}

function viewHistoryEl(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    idItem = dataItem.Id;
    tipoItem = TipiOggetto.ELEMENTO
    $('#logs').data('kendoGrid').dataSource.read();
    var dialog = $("#wHistory").data("kendoWindow");
    dialog.center();
    dialog.open();

}

function GetLogsItem() {

    return {
        IdItem: idItem,
        TipoItem: tipoItem
    };
}

function templateAvvisi(dataItem) {
    if (dataItem.Avvisi != undefined && dataItem.Avvisi != '') {
        return "<span style=\"background-color:yellow; \">" + dataItem.Avvisi + "</span>"
    }
    return "<span/>"

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