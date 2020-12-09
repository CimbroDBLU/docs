using Dapper;
using Dapper.Contrib.Extensions;
using dblu.Docs.Models;
using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace dblu.Docs.Classi
{
    public class ServerEmailManager 
    {
        //private dbluDocsContext _context;
        private string StringaConnessione;
        private readonly ILogger _logger;

        public ServerEmailManager(string connessione, ILogger logger) {
            //_context = context;
             StringaConnessione = connessione;
            _logger = logger;
        }

        public EmailServer GetServer(string Nome)
        {
            EmailServer es = null;
            try
            {
                //es = _context.EmailServer
                //    .Where(t => t.Nome == Nome).FirstOrDefault();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    es = cn.Get<EmailServer>(Nome);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            return es;
        }


       public List<EmailServer> GetAllServersEmail()
        {
            List<EmailServer> l = new List<EmailServer>();
            try
            {
                //l = _context.EmailServer
                //    .ToList<EmailServer>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = cn.GetAll<EmailServer>().ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllServersEmail: {ex.Message}");
            }
            return l;
        }

        public List<EmailServer> GetServerEmailInIngresso()
        {
            List<EmailServer> l = new List<EmailServer>();
            try
            {
                //l = _context.EmailServer
                //    .ToList<EmailServer>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                        string sql = $"Select * FROM [EmailServer] where TipoRecord = {(int)TipiRecordServer.CartellaMail} and InUscita = 0 ";
                        l = cn.Query<EmailServer>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetServerEmailInIngresso: {ex.Message}");
            }
            return l;
        }


        public List<EmailServer> GetServerEmailInUscita()
        {
            List<EmailServer> l = new List<EmailServer>();
            try
            {
                //l = _context.EmailServer
                //    .ToList<EmailServer>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    string sql = $"Select * FROM [EmailServer] where  TipoRecord = {(int)TipiRecordServer.CartellaMail} and InUscita = 1 ";
                    l = cn.Query<EmailServer>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetServerEmailInUscita: {ex.Message}");
            }
            return l;
        }

        public List<EmailServer> GetServersEmailinRoles(IEnumerable<Claim> Roles, TipiRecordServer Tipo)
        {
            string xRol = "'";
            foreach (Claim x in Roles)
            {
               if (x.Type == ClaimTypes.Role) xRol = xRol + x.Value + "','";
            }
            xRol = xRol.Substring(0, xRol.Length - 2);
            List<EmailServer> l = new List<EmailServer>();
            try
            {
               
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    string sql = $"Select * FROM [EmailServer] where  TipoRecord = {(int)Tipo} and InUscita = 0 AND [Nome] IN (Select idServer from[ServersInRole] where [RoleId] IN(" + xRol + ") Group by idServer)";
                    l = cn.Query<EmailServer>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetServersEmailinRoles: {ex.Message}");
            }
            return l;
        }


        public List<EmailServer> GetServersEmailinRoles(List<string> Ruoli, TipiRecordServer Tipo)
        {
        //    string xRol = "'";
        //    foreach (Claim x in Roles)
        //    {
        //        if (x.Type == ClaimTypes.Role) xRol = xRol + x.Value + "','";
        //    }
        //    xRol = xRol.Substring(0, xRol.Length - 2);
            List<EmailServer> l = new List<EmailServer>();
            try
            {

                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {

                    string sql = $"Select * FROM [EmailServer] where  TipoRecord = {(int)Tipo} and InUscita = 0 AND [Nome] IN (Select idServer from[ServersInRole] where [RoleId] IN ('" + string.Join("','", Ruoli) + "') Group by idServer)";
                    l = cn.Query<EmailServer>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetServersEmailinRoles: {ex.Message}");
            }
            return l;
        }

        public bool SalvaServerEmail(EmailServer obj)
        {
            var bres = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    //var c = _context.EmailServer
                    //.FirstOrDefault(e => e.Nome == obj.Nome);

                    var c = cn.Get<EmailServer>(obj.Nome);

                if (c == null)
                {
                        //_context.EmailServer.Add(obj);
                        bres = cn.Insert<EmailServer>(obj) > 0;
                }
                else
                {
                        cn.Update<EmailServer>(obj);

                        //c.Intervallo = obj.Intervallo;
                        //c.Attivo = obj.Attivo;
                        //c.Cartella = obj.Cartella;
                        //c.Email = obj.Email;
                        //c.InUscita = obj.InUscita;
                        //c.NomeProcesso = obj.NomeProcesso;
                        //c.Password = obj.Password;
                        //c.Porta = obj.Porta;
                        //c.Server = obj.Server;
                        //c.Ssl = obj.Ssl;
                        //c.Utente = obj.Utente;
                }
                    //_context.SaveChanges();
                bres = true;
            }
            }
            catch (Exception ex)
            {
                _logger.LogError($"SalvaServerEmail: {ex.Message}");

            }
            return bres;
        }

        public bool CancellaServerEmail(EmailServer obj)
        {
            var bres = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    cn.Execute($"Delete from EmailServer where Nome=@Nome", new { Nome = obj.Nome });
                }
                //var c = _context.EmailServer
                //    .FirstOrDefault(e => e.Nome == obj.Nome);

                //if (c != null)
                //{
                //    _context.EmailServer.Remove(obj);

                //}
                //_context.SaveChanges();

                bres = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CancellaEmailServer: {ex.Message}");

            }
            return bres;
        }

    }
}
