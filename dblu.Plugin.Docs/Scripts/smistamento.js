var UrlActions = null;
var TipoAll = null;
var NomeServer = null;
var idItem = "";
var tipoItem = null;
var TipiOggetto = null;
var gridEmailCurrentRow = null;
var IdNuovoElemento = null;
var mailId = null;
var mailChiave1 = null;
var mailDescrizione = null;
var mailItem = null;
var elementoItem = null;
var TipoElemento = null;

var PdfCorrente = {
    TipoAllegato : "EMAIL",
    IdAllegato : "",
    IdElemento : "",
    FilePdf : "",
    Pagina : 0,
    AggiungiFilePdf : "",
    NuovaPosizione : 0,
    iAzione: 0,
    IdAllegatoAElemento: "",
    Descrizione: "",
    FileAllegati: null
}

function SpostaEmail() {
    if (mailItem != null) {
    $("#IdAllegato").val(mailId);

    var dialog = $("#wSposta").data("kendoWindow");
    dialog.center().open();
}
}


// eventi
gridEmailOnChange = function (e) {
    if (this.select().length === 1) {
        gridEmailCurrentRow = this.select();
        var data = this.dataItem(this.select());
        mailItem = data;
        mailId = data.Id;
        mailChiave1 = data.Chiave1;
        mailDescrizione = data.Descrizione;

    elementoItem = null;
    

    PulisciDettaglio();

    $("#IdAllegato").val(data.Id);

    $.ajax({
        url: UrlActions.MailView_InArrivoCaricaDettaglio,
        type: 'POST',
        cache: false,
        data: { Id: data.Id },
        success: function (data) {
            var dettaglio = data;
            PdfCorrente.Descrizione = "  Contenuto email" 
            MostraDettaglio(dettaglio);
            //forzo visualizzazione della mail originale
            //alert("gridEmailOnChange " + IdNuovoElemento);
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
    
    
}


function CaricaElemento(elemento) {
    $('#IdFascicolo').val(elemento.IdFascicolo);
    $('#IdElemento').val(elemento.Id);
    IdNuovoElemento = elemento.Id;
    //alert(dettaglio.DescrizioneElemento);
    $('#DescrizioneElemento').val(elemento.Descrizione);
    $('#divElemento').show();
    //$('#ApriDettaglio').show();
    //$('#AggiungiAElemento').show();
    //$('#ApriDettaglio').click();

    $("#divFascicolo").find(":input").prop("disabled", true);
    $("#CollapseFascicolo").prop("disabled", false);
    $("#CollapseFascicolo").click();

    var dialog = $("#detElemento").data("kendoWindow");
    //$.ajax({
    //    url: UrlActions.MailView_editDettaglioElemento,
    //    type: 'POST',
    //    data: { IdElemento: $("#IdElemento").val() },
    //    success: function (data) {
    //        $("#IdElemento").val(data.Id);
    //        dialog.content(data);
    //        dialog.open();

    //    }
    //});
}


function InoltraEmail() {
    if (mailItem != null) { 
    $("#IdAllegato").val(mailId);
    var dialog = $("#wInoltra").data("kendoWindow");

    dialog.center().open();
    }
}

function inoltraOnClick(e) {
    e.preventDefault();
    var listMail = null;
    if ($("#emailInoltro").val() == "") listMail = $("#multiMailInoltro").val().toString();
    else {
        if ($("#multiMailInoltro").val().toString() == "") listMail = $("#emailInoltro").val();
        else listMail = $("#emailInoltro").val() + ';' + $("#multiMailInoltro").val();
    }
    var obj = {
        IdAllegato: $("#IdAllegato").val(),
        email: listMail,
        chiudi: $("#chkemailInoltro").is(':checked')
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
                var grid = $("#gridEmail").data("kendoGrid");
                if ($("#chkemailInoltro").is(':checked')) {
                    PulisciDettaglio();
                    grid.dataSource.read();
                }
                else {

                    gridRefresLastOp(grid, docsTipiOperazioni.INOLTRATO);

                }
            }
        },
        error: function (data) {
            var ok = $.parseJSON(data);
        }
    });
}

function RispondiEmail() {
    if (mailItem != null) {
    $("#IdAllegato").val(mailId);
    $("#destinatarioRisposta").val(mailChiave1);
    $("#ccRisposta").val("");
    $("#oggettoRisposta").val("R: " + mailDescrizione);
    $("#testoRisposta").val("");
    var dialog = $("#wRispondi").data("kendoWindow");

    dialog.center().open();

}
}

function onAnnulla() {
    if (mailItem != null) {
        $("#dialog").data("kendoDialog").close();
    }
}
function onElimina() {
    var grid = $("#gridEmail").data("kendoGrid");
    //alert(mailId);
    $("#IdAllegato").val(mailId);
    var obj = {
        Id: mailId
    };
    $.ajax({
        url: UrlActions.MailView_InArrivo_Cancella,
        type: 'POST',
        cache: false,
        data: obj,
        success: function (data) {
            //var ok = $.parseJSON(data);
            PulisciDettaglio();
            grid.dataSource.remove(mailItem);
            mailId = null;
            mailItem = null;        },
        error: function (data) {
            var ok = $.parseJSON(data);
        }
    });
   
        }
function CancellaEmail() {
    if (mailItem != null) {
        $("#dialog").data("kendoDialog").open();
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

function spostaOnClick(e) {
    e.preventDefault();
    var obj = {
        IdAllegato: $("#IdAllegato").val(),
        server: $("#emailServer_sposta").data("kendoComboBox").value()
    };
    $.ajax({
        url: UrlActions.MailView_InArrivo_Sposta,
        type: 'POST',
        cache: false,
        data: obj,
        success: function (data) {
            var ok = $.parseJSON(data);
            if (ok) {
                var dialog = $("#wSposta").data("kendoWindow");
                dialog.close();
                PulisciDettaglio();
                var grid = $("#gridEmail").data("kendoGrid");
                grid.dataSource.read();
            }
        },
        error: function (data) {
            var ok = $.parseJSON(data);
        }
    });
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



// gridElementi toolbar actions
function CancellaElemento() {
    if (elementoItem != null) {
        if (elementoItem.Stato > 1) {
            alert("Non è possibile eliminare un elemento già processato.");
        }
        else {
            var idElemento = elementoItem.Id;

            if (idElemento != "") {
        $("#dialogElemento").data("kendoDialog").open();
        }
}
    }
}
function onAnnullaElemento() {
    $("#dialogElemento").data("kendoDialog").close();
}
function onEliminaElemento() {

    if (elementoItem != null) {
    var grid = $("#gridemailElementi").data("kendoGrid").dataSource.remove(elementoItem);
        var obj = {
            IdElemento: elementoItem.Id,
            Revisione: elementoItem.Revisione
        };
        $.ajax({
            url: UrlActions.MailView_CancellaElemento,
            type: 'POST',
            data: obj,
            success: function (res) {
                if (res) {
                    var gridEl = $("#gridemailElementi").data("kendoGrid");
                    //gridEl.dataSource.read();
                    gridEl.dataSource.remove(elementoItem);
                }
            },
            error: function () {

            }
        });
    }

}


function AggiungiElementoGrid() {
    if (elementoItem != null) {
    var idElemento = elementoItem.Id;
    var IdElementoCorrente = $("#IdElemento").val();
    if (idElemento == IdElementoCorrente) {
        alert("La mail è già stata aggiunta a questo elemento.")
    }
    else {
        $('body').addClass('waiting');

        var gridall = $("#emailAttachments").data("kendoGrid");
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
            url: UrlActions.MailView_AllegaAElementoFascicolo,
            type: 'POST',
            data: obj,
            success: function (res) {
                if (res) {
                            //$("#IdElemento").val(data.Id);
                    $('#Completa').show();
                    $('#StampaRiepilogo').show();
                            MostraPdfCompleto($("#IdElemento").val());
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
function ApriElementoGrid() {
    if (elementoItem != null) {
        //alert("ApriElementoGrid:" +elemetoItem);
        $('body').addClass('waiting');
        $("#divFascicolo").find(":input").prop("disabled", true);
        $("#CollapseFascicolo").prop("disabled", false);
        $("#CollapseFascicolo").click();

        var dialog = $("#detElemento").data("kendoWindow");
        //$.ajax({
        //    url: UrlActions.MailView_editDettaglioElemento,
        //    type: 'POST',
        //    data: { IdElemento: elementoItem.Id },
        //    success: function (data) {
        //        $("#IdElemento").val(elementoItem.Id);
        //        dialog.content(data);
        //        dialog.open();

        //    }
        //});
        $('body').removeClass('waiting');
    }
}

//Attachments_OnRowSelect = function (e) {
//var NomeServer = null;
function Attachments_OnRowSelect(arg) {
    var data = this.dataItem(this.select());
    if (data != null) {

        var nomefile = data.NomeFile;
        var IdAllegato = $("#IdAllegato").val();
        //if (nomefile.indexOf(".pdf") > 0) {
        //    $('#anteprimapdf').show();
        //    $('#anteprimajpg').hide();
        //var pdfViewer = $("#pdfviewer").data("kendoPDFViewer");
        //if (!pdfViewer) {
        //    pdfViewer = $("#pdfviewer").kendoPDFViewer({
        //        pdfjsProcessing: {
        //            file: ""
        //        },
        //        width: "100%",
        //        height: 500
        //    }).data("kendoPDFViewer");
        //}
        //pdfViewer.width = "100%";
        //pdfViewer.height = 700;
        //var url = "IdAllegato=" + IdAllegato + "&NomeFile=" + nomefile;
        //var pdfHandlerUrl = "/MailView/GetPdf/data?" + url;
        //pdfViewer.fromFile(pdfHandlerUrl);

        //}
        //else {
        //    if (nomefile.indexOf(".jpg") > 0) {

        //        $('#anteprimajpg').show();
        //        $('#anteprimapdf').hide();

        //        var urlj = "IdAllegato=" + IdAllegato + "&NomeFile=" + nomefile;
        //        var jpgHandlerUrl = window.location.host + "/MailView/Getjpg/data?" + urlj;
        //        alert(jpgHandlerUrl);
        //        $('#imageviewer').src = jpgHandlerUrl;
        //    }
        //}
        try {}
        catch (err) {
            alert(err);
        }
    }

}


function onSelectServer(e) {

    if (e.item) {
        var dataItem = this.dataItem(e.item.index());
        NomeServer = dataItem.Nome;
        PulisciDettaglio();
        $("#gridEmail").data("kendoGrid").dataSource.read();
    }
}

function getEmails() {
    var server = "";
    if (NomeServer != null) {
        server = NomeServer;
    }
    else
        server = $("#emailServer").data("kendoComboBox").value();
    return {
        Tipo: TipoAll.Codice,
        NomeServer: server
    };
}


// dettaglio


function PulisciDettaglio() {
    $("#IdAllegato").val("");
    $('#IdFascicolo').val("");
    $('#IdElemento').val("");

    $('#CodiceSoggetto').val("");
    $('#NomeSoggetto').val("");
    $('#divSoggetto').hide();

    //$("#TestoEmail").val("");
    $('#emailAttachments').data('kendoGrid').dataSource.data("{}")
    $('#divFascicolo').hide();
    //$("#OggettoEmail").val("");

    $('#divElemento').hide();
    //$('#Completa').hide();
    //$('#StampaRiepilogo').hide();
    $('#gridemailElementi').data('kendoGrid').dataSource.data("{}");
    //$('#AssociaElemento').hide();
    try {
        //    var pdfViewer = $("#pdfviewer").data("kendoPDFViewer");
        //    pdfViewer.fromFile("");
        //    $("#divFascicolo").find(":input").prop("disabled", false);
        var emailpdfviewer = document.getElementById('emailpdfviewer').ej2_instances[0];
        emailpdfviewer.fileName ='';
        emailpdfviewer.load('');
        $('#tbdescrizione1').text("");
        $("#tbdescrizione").html('...');
    }
    catch (err) {

    }
}



function MostraDettaglio(dettaglio) {
    //$("#TestoEmail").val(dettaglio.TestoEmail);

    var soggettoPrec = $('#CodiceSoggetto').val();
    $('#CodiceSoggetto').val(dettaglio.CodiceSoggetto);
    $('#NomeSoggetto').val(dettaglio.NomeSoggetto);

    if (dettaglio.CodiceSoggetto != '') {
        //$('#divFascicolo').show();
        if (dettaglio.CodiceSoggetto != soggettoPrec) {
            //$('#gridSoggettoElementiAperti').data('kendoGrid').dataSource.read();
            //$('#divSoggettoElementiAperti').show();
        }
    }
    else {
        //$('#divSoggettoElementiAperti').hide();
        //$('#gridSoggettoElementiAperti').data('kendoGrid').dataSource.data("{}");
    };

    $('#IdFascicolo').val(dettaglio.IdFascicolo);
    $('#IdElemento').val(dettaglio.IdElemento);
    //alert(dettaglio.DescrizioneElemento);
    $('#DescrizioneElemento').val(dettaglio.DescrizioneElemento);


    if (dettaglio.IdFascicolo != '') {
        $('#divElemento').show();
        //$('#ApriDettaglio').show();
        $('#AggiungiAElemento').show();
        //alert(dettaglio.Stato);
    } else {
        // $('#AssociaElemento').show();
    }
    $('#AssociaElemento').show();
    //$('#emailAttachments').data('kendoGrid').dataSource.data(dettaglio.FileAllegati);
    $('#gridemailElementi').data('kendoGrid').dataSource.read();
}

function MostraPdfCompleto(idElemento) {

    PdfCorrente.IdAllegato = $("#IdAllegato").val();
    PdfCorrente.IdElemento = idElemento; // $("#IdElemento").val();
    PdfCorrente.iAzione = docsAzioniPdf.Carica;
    var emailpdfviewer = document.getElementById('emailpdfviewer').ej2_instances[0];
    PdfCorrente.Pagina = 1;
    emailpdfviewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";
    emailpdfviewer.fileName = JSON.stringify(PdfCorrente);
    emailpdfviewer.load(JSON.stringify(PdfCorrente));
   $("#tbdescrizione").html(PdfCorrente.Descrizione);

    //emailpdfviewer.importAnnotations();
    //$.ajax({
    //    url: UrlActions.MailPdfViewer_EditorPdf,
    //    type: 'POST',
    //    cache: false,
    //    data: { param: JSON.stringify(PdfCorrente) },
    //    success: function (data) {
    //        PdfCorrente = data;
    //        var param = PdfCorrente.FilePdf;
    //        if (param.indexOf(".pdf") > 0) {
    //            emailpdfviewer.load(param);
    //            //emailpdfviewer.downloadFileName = nome;
    //        }
    //        else {
    //            emailpdfviewer.load('');
    //        }
    //    },
    //    error: function (data) {
    //        emailpdfviewer.load('');
    //    }
    //});



}

function FascicoliData() {
    return {
        soggetto: $("#CodiceSoggetto").val()
    }
}


function AggiungiAElementoOnClick(e) {

    $('body').addClass('waiting');


    var idElemento = $('#IdElemento').val();
    var gridall = $("#emailAttachments").data("kendoGrid");
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
            url: UrlActions.MailView_AllegaAElementoFascicolo,
            type: 'POST',
            data: obj,
            success: function (res) {
                if (res) {
                    //var w = $(this).closest("[data-role=window]").data("kendoWindow");
                    //w.close();
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
                url: UrlActions.MailView_CancellaElemento,
                type: 'POST',
                data: obj,
                success: function (res) {
                    if (res) {
                        var gridEl = $("#gridemailElementi").data("kendoGrid");
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


function CercaElementiOnClick(e) {

    var myWindow = $("#wCercaElementi").data("kendoWindow");
    var codSoggetto = $('#CodiceSoggetto').val();
    myWindow.title("Cerca Elementi");

    IdNuovoElemento = null;
    myWindow.refresh({
        url: UrlActions.sggservice_UrlServizioRicercaElementi,
        type: "Post",
        data: { soggetto: codSoggetto }
    });
    myWindow.center().open();
}

function AssociaElementoOnClick(e) {

    var myWindow = $("#wCercaElementi").data("kendoWindow");
    var codSoggetto = $('#CodiceSoggetto').val();
    myWindow.title("Associa email");
    myWindow.refresh({
        url: UrlActions.sggservice_UrlServizioRicercaElementi,
        type: "Post",
        data: { soggetto: codSoggetto, associa: true, tipoelemento: TipoElemento }
    });
    //myWindow.height = window.innerHeight * 0.9 ;
    //myWindow.width = window.innerWidth * 0.9;
    myWindow.open();
    //myWindow.open().maximize();
}

GridElementi_OnRowSelect = function (e) {
    var data = this.dataItem(this.select());
    var myWindow = $("#wCercaElementi").data("kendoWindow");
    myWindow.close();
    $("#IdFascicolo").val($.trim(data.IdFascicolo));
    //$("#IdElemento").val($.trim(data.IdElemento));
    //$("#DescrizioneElemento").val($.trim(data.DscElemento));
}


//function ApriDettaglioOnClick(e) {
//    e.preventDefault();
//    alert("ApriDettaglioOnClick")
//        $('body').addClass('waiting');
//        //$("#divFascicolo *").prop('disabled', true);
//        $("#divFascicolo").find(":input").prop("disabled", true);

//        var dialog = $("#detElemento").data("kendoWindow");

//        //$.ajax({
//        //    url: UrlActions.MailView_editDettaglioElemento,
//        //    type: 'POST',
//        //        data: {IdElemento:  $("#IdElemento").val() },
//        //    success: function(data) {
//        //    $("#IdElemento").val(data.Id);
//        //        dialog.content(data);
//        //        dialog.open();

//        //    }
//        //});
    
//    }

    function detElementoOpen(e) {
            $('body').removeClass('waiting');
    }

    function detElementoClose(e) {

            //e.preventDefault();
            //$("#divFascicolo *").prop('disabled', false);
            $("#divFascicolo").find(":input").prop("disabled", false);
        $("#CollapseFascicolo").click();

        //$('#gridSoggettoElementiAperti').data('kendoGrid').dataSource.read();

        //                    @*          var grid = $("#gridEmail").data("kendoGrid");
        //var data = grid.dataItem(grid.select());

        ////PulisciDettaglio();

        //$("#IdAllegato").val(data.Id);

        //$.ajax({
        //    url: '@Url.Action("InArrivoCaricaDettaglio", "MailView")',
        //    type: 'POST',
        //    cache: false,
        //    data: {Id: data.Id },
        //    success: function (data) {
        //        var dettaglio = data;
        //        MostraDettaglio(dettaglio);

        //    },
        //    error: function (data) {

        //}
        // });*@
       //  alert($("#IdElemento").val());

        //MostraPdfCompleto();
        //$('#gridemailElementi').data('kendoGrid').dataSource.read();

        //$('#gridSoggettoElementiAperti').data('kendoGrid').dataSource.read();

        var grid = $("#gridEmail").data("kendoGrid");
        var rows = grid.select();

        grid.select(rows[0]);
}

function wCercaElementiClose(e) {

    var grid = $("#gridemailElementi").data("kendoGrid");
    grid.dataSource.read();

    elementoItem = null;

    }

//soggetto

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

function gridemailElementionChange(e) {
    //gridCurrentRow = this.select();
    // alert(gridCurrentRow);
    
    var emailpdfviewer = document.getElementById('emailpdfviewer').ej2_instances[0];
    emailpdfviewer.unload();
    var data = this.dataItem(this.select());
    if (data != null) {
 elementoItem = data;
        //$("#IdElemento").val(data.IdElemento);
        PdfCorrente.Descrizione = "  Allegato a: " + data.DscTipoElemento ;
        MostraPdfCompleto(data.IdElemento);
}
    else {
        $("#IdElemento").val("");
    }
}


function ApriDettaglioOnClick(e) {
    e.preventDefault();
    $('body').addClass('waiting');
    //$("#divFascicolo *").prop('disabled', true);
    $("#divFascicolo").find(":input").prop("disabled", true);

    var dialog = $("#detElemento").data("kendoWindow");

    $.ajax({
        url: UrlActions.MailView_editDettaglioElemento,
        type: 'POST',
        data: { IdElemento: $("#IdElemento").val() },
        success: function (data) {
            $("#IdElemento").val(data.Id);
            dialog.content(data);
            dialog.open();

        }
    });
}

//function gridemailElementionDataBound(e) {

//    alert(IdNuovoElemento);
//    if (IdNuovoElemento != null) { 
//        var grid = e.sender;
//        var dataItem = grid.dataSource.get(IdNuovoElemento);
//        var row = grid.tbody.find("tr[data-uid='" + dataItem.uid + "']");
//        grid.select(row);
//        IdNuovoElemento = null;
//    }
//}

function CodiceSoggettoOnChange() {
    $.ajax({
        url: UrlActions.MailView_GetSoggetto,
        type: 'GET',
        data: { codice: $('#CodiceSoggetto').val() },
        success: function (data) {
            $("#CodiceSoggetto").val(data.Codice);
            $("#NomeSoggetto").val(data.Nome);
            CaricaSoggetto(data.Codice);
            NotificaAssociazione(data.Codice);
        }
    });

}


function NotificaAssociazione(Soggetto) {
    IdAllegato = $("#IdAllegato").val();
    $.ajax({
        url: UrlActions.MailView_NotificaAssociazione,
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
    myWindow.center().open();
}

function CaricaSoggetto(codice) {
    if (codice == '') {
        $('#divFascicolo').hide();
        //$('#divSoggettoElementiAperti').hide();
    }
    else {
        //$('#divFascicolo').show();
        //$('#divSoggettoElementiAperti').show();
        //$('#gridSoggettoElementiAperti').data('kendoGrid').dataSource.read();

    }
    //$.post(window.location.origin + '/MailView/GetSoggetto?codice=' + codice, function (data, status) {

    //$("#inputNome").val(data.Nome);
    //$("#inputRag").val(data.Nome);
    //$("#inputIndirizzo").val(data.Indirizzo);
    //$("#inputLocalità").val(data.Localita);
    //$("#inputProvincia").val(data.Provincia);

    //$("#CodiceSoggetto").val($.trim(data.Codice));
    //$("#NomeSoggetto").val($.trim(data.Nome));

    //})
}

GridSoggetti_OnRowSelect = function (e) {
        var data = this.dataItem(this.select());
        $('#CodiceSoggetto').val(data.Codice);
        $('#NomeSoggetto').val(data.Nome);
        var myWindow = $("#cercasoggetti").data("kendoWindow");
        myWindow.close();
        CaricaSoggetto(data.Codice);
        NotificaAssociazione(data.Codice);
}



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


function OpenEmail(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    //$("#IdAllegato").val(dataItem.Id);

    var url = UrlActions.MailView_InArrivoApri + "/" + dataItem.Id;
    window.location.href = url;

}

function OnRemove(e) {
    $("#IdAllegato").val(mailId);
    $.ajax({
        url: UrlActions.MailView_InArrivo_Cancella,
        type: 'POST',
        cache: false,
        data: { Id: mailId },
        success: function (data) {
            var ok = $.parseJSON(data);
            PulisciDettaglio();

        },
        error: function (data) {
            var ok = $.parseJSON(data);

        }
    });
}


function kbEditPdfOnClick(e) {
    //var myWindow = $("#wCercaElementi").data("kendoWindow");
    //myWindow.title("Edit pdf");
    var IdElemento = $("#IdElemento").val();
    var flSalva = IdElemento != "" ;

    var obj = {
        TipoAllegato:'EMAIL',
        IdAllegato: $("#IdAllegato").val(),
        IdElemento: IdElemento,
        AbilitaSalva: flSalva  
    };

    //var dialog = $("#detElemento").data("kendoWindow");
    //$.ajax({
    //    url: UrlActions.Docs_EditPdf,
    //    type: 'POST',
    //    data: obj ,
    //    success: function (data) {
    //        dialog.content(data);
    //        dialog.open();
    //    }
    //});

    //var myWindow = $("#cercasoggetti").data("kendoWindow");
    //myWindow.title("pdf");
    //myWindow.refresh({
    //    url: UrlActions.Docs_EditPdf,
    //    type: "Post",
    //    data: obj
    //});
    //myWindow.open();


    window.open(UrlActions.Docs_EditPdf, 'Editor pdf');

    //$.post(UrlActions.Docs_EditPdf, obj, function (data) {
    //    var w = window.open('about:blank');
    //    w.document.open();
    //    w.document.write(data);
    //    w.document.close();
    //});

}


// pdf

function importAnnotations() {

    var emailpdfviewer = document.getElementById('emailpdfviewer').ej2_instances[0];

    //emailpdfviewer.fileName = JSON.stringify(PdfCorrente);
    //emailpdfviewer.importAnnotations('');
    //emailpdfviewer.exportFileName = JSON.stringify(PdfCorrente);
    emailpdfviewer.importAnnotation();
}

function annotationAdd(e) {
    // alert("The signature is added to the PDF document successfully");
}


function saveAnnotations() {

    var emailpdfviewer = document.getElementById('emailpdfviewer').ej2_instances[0];
    emailpdfviewer.exportFileName = JSON.stringify(PdfCorrente);
    emailpdfviewer.exportAnnotation();
}

function documentLoaded(args) {

    var emailpdfviewer = document.getElementById('emailpdfviewer').ej2_instances[0];
    emailpdfviewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";

    //alert("The document" + args.fileName + "is ready to view");
    $.ajax({
        url: UrlActions.MailPdfViewer_GetPdfEditAction,
        type: 'POST',
        cache: false,
        data: { param: JSON.stringify(PdfCorrente)},
        success: function (data) {
            PdfCorrente = data;
            $('#emailAttachments').data('kendoGrid').dataSource.data(PdfCorrente.FileAllegati);
        },
        error: function (data) {
            emailpdfviewer.load('');
        }
    });

        //alert(PdfCorrente.Pagina);
    try {
    //    var emailpdfviewer = document.getElementById('emailpdfviewer').ej2_instances[0];
        emailpdfviewer.navigation.goToFirstPage();
        if (PdfCorrente.Pagina > 1) {
            emailpdfviewer.navigation.goToPage(PdfCorrente.Pagina);
        }
    //    //alert(PdfCorrente.Pagina);
    }
    catch (err) {

    }

    emailpdfviewer.importAnnotation(JSON.stringify(PdfCorrente));

}


function documentPrinted() {

    $.ajax({
        url: UrlActions.MailView_InArrivo_Stampato,
        type: 'POST',
        cache: false,
        data: { IdAllegato: $("#IdAllegato").val(), IdElemento: $("#IdElemento").val() },
        success: function (data) {

    var grid = $("#gridEmail").data("kendoGrid");
    gridRefresLastOp(grid, docsTipiOperazioni.STAMPATO);
            //grid.select(gridEmailCurrentRow);

            //var gridE = $("#gridemailElementi").data("kendoGrid");
            //var idEl = $("#IdElemento").val();
            //gridE.items().each(function () {
            //    var di = gridE.dataItem(this);
            //    if (di.Id == idEl) {
            //        //gride.select(this);
            //        gridRowRefresLastOp(this, gridE, docsTipiOperazioni.STAMPATO);   
            //    }
            //});
        }
    });
}

function exportSuccess(args) {

    if (PdfCorrente.iAzione == docsAzioniPdf.Salva) {

        var pdfEditorViewer = document.getElementById('emailpdfviewer').ej2_instances[0];
        pdfEditorViewer.load(JSON.stringify(PdfCorrente))
        pdfEditorViewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";

    }
}
// full pdf Ctr+Maiusc
$(document).keydown(function (e) {
    if (e.keyCode == 16 && e.ctrlKey) {
        var emailpdfviewer = document.getElementById('emailpdfviewer').ej2_instances[0];
    var gridEl = $("#gridemailElementi").data("kendoGrid");

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

function tbpdf_click(e) {

    var emailpdfviewer = document.getElementById('emailpdfviewer').ej2_instances[0];
    var gridEl = $("#gridemailElementi").data("kendoGrid");

    PdfCorrente.IdAllegato = $("#IdAllegato").val();
    //PdfCorrente.IdElemento = $("#IdElemento").val();
    PdfCorrente.Pagina = emailpdfviewer.currentPageNumber;

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

        //emailpdfviewer.fileName = JSON.stringify(PdfCorrente);
        //emailpdfviewer.exportFileName = JSON.stringify(PdfCorrente);
        //emailpdfviewer.exportAnnotationFileName = JSON.stringify(PdfCorrente);
        if (PdfCorrente.iAzione == docsAzioniPdf.Salva && emailpdfviewer.annotationCollection != undefined &&  emailpdfviewer.annotationCollection.length > 0) {

            emailpdfviewer.exportAnnotation(JSON.stringify(PdfCorrente));
            //var myPromise = emailpdfviewer.exportAnnotationsAsObject();
            //myPromise.then(response => {
            //    emailpdfviewer.load(JSON.stringify(PdfCorrente))
            //    emailpdfviewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";
            //},
            //    () => {
            //        alert("Errore nel documento, riprovare.");
            //    }
            //);
        }
        else {
            emailpdfviewer.load(JSON.stringify(PdfCorrente))
            emailpdfviewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";
        }
    }

    //alert(PdfCorrente.iAzione);


                //emailpdfviewer.downloadFileName = ;
//        url: UrlActions.MailPdfViewer_EditorPdf,
//        type: 'POST',
//        cache: false,
//        data: { param: JSON.stringify(PdfCorrente)},
//        success: function (data) {
//            PdfCorrente = data;
//            var param = PdfCorrente.FilePdf;
//            if (param.indexOf(".pdf") > 0) {
//                emailpdfviewer.load(param);

//                //emailpdfviewer.downloadFileName = ;
//            }
//            else {
//                emailpdfviewer.load('');
//            }
//        },
//        error: function (data) {
//            emailpdfviewer.load('');
//        }
//    });
}



function wPdfEditorClose(e) {

    if (MustReloadPdf) {

        PdfCorrente.iAzione = docsAzioniPdf.Ricarica;
        var emailpdfviewer = document.getElementById('emailpdfviewer').ej2_instances[0];
        emailpdfviewer.load(JSON.stringify(PdfCorrente));
        emailpdfviewer.downloadFileName = PdfCorrente.IdAllegato + ".pdf";

    }

}

function NuovoElemento(e) {

    if (mailItem == null) {
        alert("Seleziona una mail");
    }
    //e, Categoria, TipoElemento, Aggrega
    var Aggrega = this.element.attr("aggrega");
    var Categoria = this.element.attr("categoria");
    TipoElemento = this.element.attr("codice");
    var view = this.element.attr("viewattributi");

    var fNameDiv = "div" + TipoElemento;
   
    listaTipiElementi.forEach(function (tipo) {
        //if (tipo.AggregaAElemento == false) { 
        try {
            $('#div' + tipo.Codice).hide();
        }
        catch { };
    });
    $('body').addClass('waiting');
    if (Aggrega === 'True') {
        if (view === '') {
        $('#divSoggetto').show();
        //  $('#divElemento').show();
        //$('#divFascicolo').show();
    }
    else {
        $('#divSoggetto').hide();
            $('#' + fNameDiv).show();
        }
    }
    else {
        $('#divSoggetto').hide();
        $('#divElemento').hide();
        //$('#divFascicolo').hide();
        $('#' + fNameDiv).show();
    }
    $('body').removeClass('waiting')
}


function SalvaAttributi(caller) {
    //caller.preventDefault();
    //TipoElemento = this.element.attr("codice");
    var fNameForm = "form" + TipoElemento;
    var frm = $("#" + fNameForm);

    var items = [];
    var grid = $("#gridEmail").data("kendoGrid");


    if (grid === undefined) {

    }
    else {
        var selectedElements = grid.select();
        for (var j = 0; j < selectedElements.length; j++) {
            var item = grid.dataItem(selectedElements[j]);
            items[j] = item.Id;
            
        }

        var obj = {
            IdAllegato: items,
            form: frm.serialize()
        };
    }
  

    //var obj = {
    //    IdAllegato: $("#IdAllegato").val(),
    //    form: frm.serialize()
    //};
    $.ajax(
        {
            url: UrlActions.Spostamento_salvaAttAgg,
            type: 'POST',
            cache: false,
            data: obj,
            success: function (data) {
                //var grid = $("#gridEmail").data("kendoGrid");
                PulisciDettaglio();
                grid.dataSource.read();
            },
            error: function (data) {
                alert(data);
            }
        });
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

