using BPMClient;
using Dapper;
using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using dblu.Docs.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace dWorker.Plugin.Docs
{
    public class MailManager
    {

        private readonly ILogger _logger;
        private IConfiguration _conf;

        public string Connessione { get; set; }
        public string TipoAllegato { get; set; }
        public string Bpm_url { get; set; }
        public string Bpm_User { get; set; }
        public string Bpm_Password { get; set; }
        public dblu.Docs.Models.StatoAllegato StatoIniziale { get; set; }
        //'private static readonly NLog.Logger _nlogger = NLog.LogManager.GetCurrentClassLogger();
        public MailManager(ILogger logger, IConfiguration conf)
        {
            _logger = logger;
            _conf = conf;
        }

        public async Task<bool> ProcessaEmail(string elencoServer , CancellationToken cancel )
        {
            try
            {
                _logger.LogInformation($"MailManager.ProcessaEmail: Processing {elencoServer}...");
                using (SqlConnection cn = new SqlConnection(Connessione))
                {
                    var serverIn = cn.Query<EmailServer>($"select * from EmailServer where Attivo<>0 and InUscita =0 and TipoRecord in ({(int)TipiRecordServer.CartellaMail},{(int)TipiRecordServer.CartellaAltreMail})").ToList();

                    if (serverIn.Count > 0)
                    {

                        TipiAllegati tipo = cn.QueryFirstOrDefault<TipiAllegati>(
                            "SELECT * FROM TipiAllegati WHERE Codice=@Codice ",
                            new { Codice = this.TipoAllegato });

                        var allMan = new AllegatiManager(Connessione, _logger);

                        //bpm
                        var eng = new BPMClient.BPMEngine();
                        eng.Imposta(this.Bpm_url, this.Bpm_User, this.Bpm_Password, "");

                        foreach (EmailServer s in serverIn)
                        {
                            if (!(string.IsNullOrEmpty(elencoServer) || elencoServer.Contains(s.Nome)))
                                continue;

                            try
                            {
                                _logger.LogInformation($"Elaborazione di {s.Nome}");
                                using (var client = new ImapClient())
                                {
                                    client.CheckCertificateRevocation = false;
                                    await client.ConnectAsync(s.Server, s.Porta, s.Ssl, cancel);

                                    await client.AuthenticateAsync(s.Utente, s.Password, cancel);

                                    IMailFolder inbox = null;
                                    IMailFolder archivio = null;
                                    if (!string.IsNullOrEmpty(s.Cartella))
                                    {
                                        try
                                        {
                                            IList<IMailFolder> mf = client.GetFolders(client.PersonalNamespaces[0], true, cancel);
                                            inbox = mf.Where(c => c.Name.ToLower() == s.Cartella.ToLower()).FirstOrDefault();

                                            if (inbox == null)
                                            {
                                                inbox = mf.FirstOrDefault();
                                            }
                                            else
                                            {
                                                s.Cartella = inbox.FullName;
                                            }
                                            if (inbox == null)
                                            {
                                                inbox = client.GetFolder(s.Cartella, cancel);
                                            }
                                        }
                                        catch
                                        {
                                            inbox = client.Inbox;
                                            s.Cartella = inbox.FullName;
                                            _logger.LogError($"Cartella non valida ( {s.Cartella}).");

                                        }
                                    }
                                    else
                                    {
                                        inbox = client.Inbox;
                                        s.Cartella = inbox.FullName;
                                    }
                                    await inbox.OpenAsync(FolderAccess.ReadWrite, cancel);
                                    _logger.LogDebug($" email {inbox.Count}");

                                    //prendo i messaggi da leggere 
                                    var query = SearchQuery.NotSeen;
                                    bool archiviaEmail = !string.IsNullOrEmpty(s.CartellaArchivio);
                                    if (archiviaEmail)
                                    {
                                        try
                                        {
                                            IList<IMailFolder> mf = client.GetFolders(client.PersonalNamespaces[0], false, cancel);
                                            //
                                            if (mf.Where(c => c.Name.ToLower() == s.CartellaArchivio.ToLower()).Count() > 1)
                                                archivio = mf.Where(c => c.Name.ToLower() == s.CartellaArchivio.ToLower() && c.FullName.StartsWith($"{s.Cartella}")).FirstOrDefault();
                                            else
                                                archivio = mf.Where(c => c.Name.ToLower() == s.CartellaArchivio.ToLower()).FirstOrDefault();
                                            if (archivio == null)
                                            {
                                                mf = inbox.GetSubfolders(true, cancel);
                                                archivio = mf.Where(c => c.Name.ToLower() == s.CartellaArchivio.ToLower()).FirstOrDefault();
                                            }
                                            if (archivio == null)
                                            {
                                                archivio = client.GetFolder(s.CartellaArchivio, cancel);
                                            }
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                archivio = client.GetFolder($"{s.Cartella}/{s.CartellaArchivio}", cancel);
                                            }
                                            catch
                                            {
                                                archiviaEmail = false;
                                                _logger.LogError($"Cartella archivio non valida ( {s.CartellaArchivio},  {s.Cartella}/{s.CartellaArchivio} ).");
                                            }
                                        }
                                        if (archivio != null)
                                        {
                                            query = SearchQuery.All;
                                        }
                                    }

                                    // var i = 0;
                                    foreach (var uid in inbox.Search(query, cancel))
                                    {
                                        if (cancel.IsCancellationRequested)
                                        {
                                            break;
                                        }
                                        bool mailprocessata = false;
                                        var message = await inbox.GetMessageAsync(uid, cancel);
                                        //salvo il messaggio
                                        if (message != null)
                                        {
                                            var Nomefile = $"{message.MessageId}.eml";
                                            DateTime? T = null;
                                            if (message.Date.UtcDateTime != DateTime.MinValue)
                                                T = message.Date.UtcDateTime;
                                            if (Nomefile.Length > 255)
                                                Nomefile = Nomefile.Substring(Nomefile.Length - 254, 254);

                                            AllegatoEmail allm = cn.QueryFirstOrDefault<AllegatoEmail>(
                                               $"SELECT * , {AllegatoEmail.SqlAttributi()} FROM Allegati WHERE Tipo=@Tipo and NomeFile = @NomeFile " +
                                                 " AND ( Folder IS NULL OR Folder = @DestFolder) " +
                                                 " AND CASE WHEN isdate(JSON_VALUE(Attributi,'$.Data'))>0 THEN JSON_VALUE(Attributi,'$.Data') ELSE NULL END = @Data ",
                                                new { Tipo = tipo.Codice, NomeFile = Nomefile, Data = T,DestFolder=s.Cartella });
                                            var flIgnora = false;
                                            if (allm != null)
                                            {
                                                allm.TipoNavigation = allMan.GetTipo(allm.Tipo);
                                                if (allm.elencoAttributi == null)
                                                {
                                                    allm.elencoAttributi = tipo.Attributi;
                                                }
                                                flIgnora = allm.Stato > StatoAllegato.Attivo;
                                            }

                                            if (flIgnora)
                                            {
                                                inbox.SetFlags(uid, MessageFlags.Seen, false, cancel);
                                                mailprocessata = true;
                                                _logger.LogWarning($"Email ignorata perché già elaborata in stato {allm.Stato} ({allm.Id})");
                                            }
                                            else
                                            {
                                                var newall = false;

                                                var descr = "";
                                                string emailmitt = message.From.Mailboxes.FirstOrDefault()?.Address ?? "";
                                                if (!string.IsNullOrEmpty(message.Subject))
                                                {
                                                    descr = message.Subject;
                                                }
                                                if (descr.Length > 250)
                                                {
                                                    descr = descr.Substring(0, 250);
                                                }
                                                if (allm == null)
                                                {
                                                    allm = new AllegatoEmail()
                                                    {
                                                        Descrizione = descr,
                                                        NomeFile = Nomefile,
                                                        Tipo = tipo.Codice,
                                                        TipoNavigation = tipo,
                                                        Stato = this.StatoIniziale,
                                                        Origine = s.Nome
                                                    };
                                                    allm.elencoAttributi = tipo.Attributi;
                                                    newall = true;
                                                }
                                                else
                                                {
                                                    allm.Descrizione = descr;
                                                    allm.DataUM = DateTime.Now;
                                                }
                                                if (allm.elencoAttributi == null) { allm.elencoAttributi = tipo.Attributi; }
                                                try
                                                {
                                                    allm.Testo = message.TextBody;
                                                }
                                                catch { }

                                                allm.Mittente = emailmitt;
                                                allm.Destinatario = message.To.ToString();
                                                allm.Data = (DateTime?)message.Date.UtcDateTime;
                                                allm.Oggetto = message.Subject;
                                                allm.MessageId = message.MessageId;
                                                allm.Folder = s.Nome;
                                                allm.SetAttributo("CodiceSoggetto", "");

                                                // decodifica cliente
                                                try
                                                {

                                                    string sql = "SELECT CodiceSoggetto, Nome FROM EmailSoggetti JOIN soggetti ON soggetti.Codice = EmailSoggetti.CodiceSoggetto WHERE email ='" + emailmitt + "' ";

                                                    var xx = cn.Query(sql).ToList();
                                                    if (xx.Count == 1)
                                                    {
                                                        allm.SetAttributo("CodiceSoggetto", xx[0].CodiceSoggetto);
                                                        allm.SetAttributo("NomeSoggetto", xx[0].Nome);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogWarning($"sp_GetSoggettoEmail {ex.Message}");
                                                }

                                                MemoryStream file = new MemoryStream();
                                                await message.WriteToAsync(file, cancel);

                                                Allegati all = await allMan.SalvaAsync((Allegati)allm, file, newall);

                                                if (all != null)
                                                {
                                                    _logger.LogDebug($"email salvata {all.Id} {all.NomeFile} ");

                                                    if (!string.IsNullOrEmpty(s.NomeProcesso))
                                                    {
                                                        var pd = new BPMProcessDefinition(eng);

                                                        var pdi = await pd.Get("", s.NomeProcesso);
                                                        if (pdi == null || pdi.Id == null)
                                                        {
                                                            _logger.LogError($"Impossibile trovare la definizione di processo {s.NomeProcesso}.");
                                                        }
                                                        else
                                                        {
                                                            SubmitStartForm ssf = new SubmitStartForm();  
                                                            ssf.BusinessKey = message.MessageId;
                                                            if (ssf.BusinessKey.Length > 255)
                                                                ssf.BusinessKey = ssf.BusinessKey.Substring(Nomefile.Length - 254, 254);

                                                            //tolgo il testo per limitare la variabile jAllegato
                                                            all.Testo = "";

                                                            ssf.SetVariable("sMittente", emailmitt);
                                                            ssf.SetVariable("dData", message.Date.UtcDateTime.ToString("dd/MM/yyyy hh:mm"));
                                                            ssf.SetVariable("sOggetto", message.Subject);
                                                            ssf.SetVariable("sIdAllegato", all.Id.ToString());
                                                            ssf.SetVariable("jAllegato", JsonConvert.SerializeObject(all));

                                                            BPMProcessInstanceInfo pi = await pd.SubmitForm(pdi.Id, s.NomeProcesso, ssf);
                                                            if (pi == null)
                                                            {
                                                                //_logger.LogError($"Impossibile avviare il processo {s.NomeProcesso}.");
                                                            }
                                                            else
                                                            {
                                                                _logger.LogDebug($"Processo {s.NomeProcesso} avviato.");
                                                                // segna la mail come elaborata
                                                                inbox.SetFlags(uid, MessageFlags.Seen, false, cancel);
                                                                mailprocessata = true;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _logger.LogDebug($"nessun processo avviato.");
                                                        inbox.SetFlags(uid, MessageFlags.Seen, false, cancel);
                                                        mailprocessata = true;
                                                    }
                                                }
                                            }
                                        }

                                        //break;   
                                        if (mailprocessata && archiviaEmail)
                                        {
                                            try
                                            {
                                                await inbox.MoveToAsync(uid, archivio, cancel);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError($"archivia email : {ex.Message}");
                                                mailprocessata = false;
                                            }
                                        }
                                    }

                                    client.Disconnect(true, cancel);
                                }
                            }
                            catch (Exception ex)
                            {

                                _logger.LogError($" {s.Nome} Error: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"Error: {ex.Message}");
                return false;
            }
            return true;
        }

        public string RemoveSpecialChars(string input)
        {
            return Regex.Replace(input, @"[^0-9a-zA-Z\._@]", string.Empty);
        }

    }

}
