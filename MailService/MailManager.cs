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

namespace dbluMailService
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

        public async Task<bool> ProcessaEmail(CancellationToken cancel)
        {
            try
            {

                //var context = new dbluDocsContext(Connessione);

                //var serverIn =  context.EmailServer
                //            .Where(s => s.Attivo == true && s.InUscita == false)
                //            .ToList();
                using (SqlConnection cn = new SqlConnection(Connessione))
                {
                    var serverIn = cn.Query<EmailServer>("select * from EmailServer where Attivo<>0 and InUscita =0").ToList();

                    if (serverIn.Count > 0)
                    {
                        //TipiAllegati tipo = context.TipiAllegati
                        //.Where(t => t.Codice == this.TipoAllegato)
                        //.FirstOrDefault();

                        TipiAllegati tipo = cn.QueryFirstOrDefault<TipiAllegati>(
                            "SELECT * FROM TipiAllegati WHERE Codice=@Codice ",
                            new { Codice = this.TipoAllegato });

                        var allMan = new AllegatiManager(Connessione, _logger);

                        //bpm
                        var eng = new BPMClient.BPMEngine();

                        eng.Imposta(this.Bpm_url, this.Bpm_User, this.Bpm_Password, "");

                        foreach (EmailServer s in serverIn)
                        {
                            _logger.LogInformation($"Elaborazione di {s.Nome}");
                            using (var client = new ImapClient())
                            {
                                await client.ConnectAsync(s.Server, s.Porta, s.Ssl, cancel);

                                // If you want to disable an authentication mechanism,
                                // you can do so by removing the mechanism like this:
                                // client.AuthenticationMechanisms.Remove("XOAUTH");

                                await client.AuthenticateAsync(s.Utente, s.Password, cancel);

                                IMailFolder inbox = null;
                                IMailFolder archivio = null;
                                if (!string.IsNullOrEmpty(s.Cartella))
                                {
                                    inbox = client.GetFolder(s.Cartella, cancel);
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
                                        IList<IMailFolder> mf = client.GetFolders(client.PersonalNamespaces[0], true, cancel);
                                        //
                                        archivio = mf.Where(c => c.Name == s.CartellaArchivio).FirstOrDefault();
                                        if (archivio == null)
                                        {
                                            mf = inbox.GetSubfolders(true, cancel);
                                            archivio = mf.Where(c => c.Name == s.CartellaArchivio).FirstOrDefault();
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

                                        Allegati all = cn.QueryFirstOrDefault<Allegati>(
                                           @"SELECT * FROM Allegati WHERE Tipo=@Tipo and NomeFile = @NomeFile 
                                               and CASE WHEN isdate(JSON_VALUE(Attributi,'$.Data'))>0 THEN JSON_VALUE(Attributi,'$.Data') ELSE NULL END = @Data ",
                                            new { Tipo = tipo.Codice, NomeFile = Nomefile, Data = message.Date.UtcDateTime });
                                        var flIgnora = false;
                                        if (all != null)
                                        {
                                            all = allMan.Get(all.Id);
                                            flIgnora = all.Stato > StatoAllegato.Attivo;
                                        }

                                        if (flIgnora)
                                        {
                                            inbox.SetFlags(uid, MessageFlags.Seen, false, cancel);
                                            mailprocessata = true;
                                            _logger.LogWarning($"Email ignorata perché già elaborata in stato {all.Stato} ({all.Id})");
                                        }
                                        else
                                        {
                                            var newall = false;

                                            var descr = "";
                                            if (!string.IsNullOrEmpty(message.Subject))
                                            {
                                                descr = message.Subject;
                                            }
                                            if (descr.Length > 250)
                                            {
                                                descr = descr.Substring(0, 250);
                                            }
                                            if (all == null)
                                            {
                                                all = new Allegati()
                                                {
                                                    Descrizione = descr,
                                                    NomeFile = Nomefile,
                                                    Tipo = tipo.Codice,
                                                    TipoNavigation = tipo,
                                                    Stato = this.StatoIniziale,
                                                    Origine = s.Nome
                                                };
                                                all.elencoAttributi = tipo.Attributi;
                                                //context.Allegati.Add(all);
                                                newall = true;
                                            }
                                            else
                                            {
                                                all.Descrizione = descr;
                                                all.DataUM = DateTime.Now;
                                            }
                                            if (all.elencoAttributi == null) { all.elencoAttributi = tipo.Attributi; }
                                            try
                                            {
                                                all.Testo = message.TextBody;
                                            }
                                            catch { }

                                            string emailmitt = message.From.Mailboxes.First().Address;

                                            all.SetAttributo("Mittente", emailmitt);
                                            all.SetAttributo("Data", message.Date.UtcDateTime);
                                            all.SetAttributo("Oggetto", message.Subject);
                                            all.SetAttributo("MessageId", message.MessageId);
                                            all.SetAttributo("CodiceSoggetto", "");

                                            // decodifica cliente
                                            try
                                            {

                                                //using (SqlConnection cn = new SqlConnection(Connessione))
                                                //{
                                                var p = new DynamicParameters();

                                                p.Add("@Mittente", emailmitt, dbType: DbType.String, direction: ParameterDirection.Input);
                                                p.Add("@Codice", "", dbType: DbType.String, direction: ParameterDirection.Output);
                                                p.Add("@Nome", "", dbType: DbType.String, direction: ParameterDirection.Output);

                                                var sql = "exec dbo.sp_GetSoggettoEmail @Mittente, @Codice OUT, @Nome OUT";
                                                cn.Execute(sql, p);

                                                all.SetAttributo("CodiceSoggetto", p.Get<string>("@Codice"));
                                                all.SetAttributo("NomeSoggetto", p.Get<string>("@Nome"));

                                                //}
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogWarning($"sp_GetSoggettoEmail {ex.Message}");
                                            }

                                            MemoryStream file = new MemoryStream();
                                            await message.WriteToAsync(file, cancel);

                                            all = await allMan.SalvaAsync(all, file, newall);

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


        public async Task<bool> ResetMail(CancellationToken cancel)
        {
            try
            {
                //var context = new dbluDocsContext(Connessione);
                using (SqlConnection cn = new SqlConnection(Connessione))
                {

                    //var serverIn = context.EmailServer
                    //           .Where(s => s.Attivo == true && s.InUscita == false)
                    //           .ToList();
                    var serverIn = cn.Query<EmailServer>(
                        "select * from EmailServer where Attivo<>0 and InUscita =0").ToList();

                    //TipiAllegati tipo = context.TipiAllegati
                    //.Where(t => t.Codice == this.TipoAllegato)
                    //.FirstOrDefault();
                    TipiAllegati tipo = cn.Get<TipiAllegati>(this.TipoAllegato);

                    if (tipo.Attributi.Count() == 0)
                    {
                        var a = new Attributo
                        {
                            Nome = "Mittente",
                            Alias = "Chiave1",
                            Descrizione = "Mittente",
                            SystemType = typeof(string),
                            Obbligatorio = true,
                            ValorePredefinito = "",
                            Visibilità = Visibilita_Attributi.VISIBLE,
                        };
                        tipo.Attributi.Add(a);
                        a = new Attributo
                        {
                            Nome = "Data",
                            Alias = "Chiave2",
                            Descrizione = "Data",
                            SystemType = typeof(DateTime),
                            Obbligatorio = true,
                            Visibilità = Visibilita_Attributi.VISIBLE
                        };
                        tipo.Attributi.Add(a);
                        a = new Attributo
                        {
                            Nome = "CodiceSoggetto",
                            Alias = "Chiave3",
                            Descrizione = "CodiceSoggetto",
                            SystemType = typeof(string),
                            Obbligatorio = true,
                            ValorePredefinito = "",
                            Visibilità = Visibilita_Attributi.EDITABLE
                        };
                        tipo.Attributi.Add(a);
                        a = new Attributo
                        {
                            Nome = "Oggetto",
                            Alias = "Chiave4",
                            Descrizione = "Oggetto",
                            SystemType = typeof(string),
                            Obbligatorio = true,
                            ValorePredefinito = "",
                            Visibilità = Visibilita_Attributi.VISIBLE
                        };
                        tipo.Attributi.Add(a);
                        a = new Attributo
                        {
                            Nome = "MessageId",
                            Alias = "Chiave5",
                            Descrizione = "MessageId",
                            SystemType = typeof(string),
                            Obbligatorio = true,
                            ValorePredefinito = "",
                            Visibilità = Visibilita_Attributi.VISIBLE
                        };
                        tipo.Attributi.Add(a);

                        //context.SaveChanges();
                        cn.Update<TipiAllegati>(tipo);
                    }

                    if (serverIn.Count > 0)
                    {
                        foreach (EmailServer s in serverIn)
                        {
                            using (var client = new ImapClient())
                            {
                                await client.ConnectAsync(s.Server, s.Porta, s.Ssl, cancel);
                                await client.AuthenticateAsync(s.Utente, s.Password, cancel);
                                IMailFolder inbox = null;
                                if (!string.IsNullOrEmpty(s.Cartella))
                                {
                                    inbox = client.GetFolder(s.Cartella, cancel);
                                }
                                else
                                {
                                    inbox = client.Inbox;
                                }
                                await inbox.OpenAsync(FolderAccess.ReadWrite, cancel);
                                var query = SearchQuery.Seen;
                                foreach (var uid in inbox.Search(query, cancel))
                                {
                                    inbox.SetFlags(uid, MessageFlags.None, false, cancel);
                                }

                                client.Disconnect(true, cancel);
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



        public async Task<bool> TestProcess(string NomeProcesso, string Id, CancellationToken cancel)
        {

            try
            {

                //var context = new dbluDocsContext(Connessione);
                using (SqlConnection cn = new SqlConnection(Connessione))
                {
                    var allMan = new AllegatiManager(Connessione, _logger);

                    var all = allMan.Get(Id);//"B8852801-5C20-4E54-BB6B-12A4ED60B578"
                    var m = await allMan.GetFileAsync(all.Id.ToString());
                    MimeKit.MimeMessage message = MimeKit.MimeMessage.Load(m, cancel);

                    var eng = new BPMClient.BPMEngine();
                    eng.Imposta(this.Bpm_url, this.Bpm_User, this.Bpm_Password, "");
                    var pd = new BPMProcessDefinition(eng);

                    var pdi = await pd.Get("", NomeProcesso); //"RicezioneEmailClienti"
                    if (pdi == null)
                    {
                        _logger.LogError($"Impossibile trovare la definizione di processo {NomeProcesso}.");
                    }
                    else
                    {
                        SubmitStartForm ssf = new SubmitStartForm();
                        ssf.BusinessKey = message.MessageId;

                        string emailmitt = this.RemoveSpecialChars(message.From.Mailboxes.First().Address);

                        ssf.SetVariable("sMittente", emailmitt);
                        ssf.SetVariable("dData", message.Date.ToString("dd/MM/yyyy"));
                        ssf.SetVariable("sOggetto", message.Subject);
                        ssf.SetVariable("sIdAllegato", all.Id.ToString());
                        ssf.SetVariable("jAllegato", JsonConvert.SerializeObject(all));

                        BPMProcessInstanceInfo pi = await pd.SubmitForm(pdi.Id, NomeProcesso, ssf);
                        if (pi == null)
                        {
                            _logger.LogError($"Impossibile avviare il processo {NomeProcesso}.");
                            return false;
                        }
                        else
                        {
                            _logger.LogDebug($"Processo {NomeProcesso} avviato.");
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
