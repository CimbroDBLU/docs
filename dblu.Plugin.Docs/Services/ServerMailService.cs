using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using Dapper;
using dblu.Portale.Core.Infrastructure.Identity.Classes;
using Microsoft.Extensions.Configuration;
using dblu.Portale.Core.Infrastructure.Identity.Services;

namespace dblu.Portale.Plugin.Docs.Services
{
    public class ServerMailService
    {
        private IConfiguration _config { get; set; }
        private string _connDocs = "";
        private IApplicationUsersManager _usr;
        //private string _connIdentity = "";

        public ServerMailService(IConfiguration config, IApplicationUsersManager usr)
        {
            this._config = config;
            try
            {
                _connDocs = _config.GetConnectionString("dblu.Docs");
                _usr = usr;
                //_connIdentity = _config.GetConnectionString("dblu.Docs");
            }
            catch { }
        }

        public IEnumerable<Role> GetAllRolesForServer(string ServerName)
        {
            IEnumerable<Role> tmpRuoli = new List<Role>();
            IEnumerable<string> xx;

            if (!string.IsNullOrEmpty(ServerName))
            {
                using (IDbConnection cn = new SqlConnection(_connDocs))
                {
                    string sql = "SELECT RoleId FROM ServersInRole where idServer='" + ServerName + "'";

                    xx = cn.Query<string>(sql);  //, new { idutente = idUtente });
                    cn.Close();
                }

                if (xx.Count() != 0)
                    return _usr.GetRoles().Where(d => xx.Contains(d.Code));

            }
            return tmpRuoli;

        }

        public IEnumerable<Role> RuoliNonAttivi(string ServerName)
        {
            IEnumerable<Role> tmpRuoli=new List<Role>();
            IEnumerable<string> xx;

            //(select roleid from UsersinRole where userid = '" + idUtente + "')"
            using (IDbConnection cn = new SqlConnection(_connDocs))
            {
                string sql = "SELECT RoleId FROM ServersInRole where idServer='" + ServerName + "'";

                xx = cn.Query<string>(sql);  //, new { idutente = idUtente });
                cn.Close();
            }

            if (xx.Count() != 0)
             return _usr.GetRoles().Where(d => !xx.Contains(d.Code));

            return  tmpRuoli;
        }

        public void RemoveFromRole(string RoleID, string ServerName)
        {
            string sql = "Delete from ServersInRole where idServer=@idServer and RoleId=@RoleId";


            using (IDbConnection conn = new SqlConnection(_connDocs))
            {
                conn.Execute(sql, new { RoleId = RoleID, idServer = ServerName });
                conn.Close();

            }
        }


        

        public void AddRoleToServer(string RoleID, string ServerName)
        {
        
            using (IDbConnection cn = new SqlConnection(_connDocs))
            {
                cn.Open();


                string sql = "INSERT INTO [ServersInRole] ([RoleId], [idServer]) Values(@Idruolo, @idServer)";

                cn.Execute(sql, new { Idruolo = RoleID, idServer = ServerName });
                cn.Close();
            }
        }


        public IEnumerable<Role> GetAllRolesForElemento(string TipoElemento)
        {
            IEnumerable<string> xx;

            using (IDbConnection cn = new SqlConnection(_connDocs))
            {
                string sql = "SELECT RoleId FROM [ElementiInRoles] where [Tipo]=@Tipo ";

                xx = cn.Query<string>(sql, new { Tipo = TipoElemento });  //, new { idutente = idUtente });
                cn.Close();
            }

            if (xx.Count() != 0)
                return _usr.GetRoles().Where(d => xx.Contains(d.Code)).ToList();

            return new List<Role>();
        }

        public IEnumerable<Role> RuoliNonAttivi_Elemento(string TipoElemento)
        {
             IEnumerable<string> xx;
            
            using (IDbConnection cn = new SqlConnection(_connDocs))
            {
                string sql = "SELECT RoleId FROM ElementiInRoles where Tipo=@Tipo";

                xx = cn.Query<string>(sql, new { Tipo = TipoElemento});  //, new { idutente = idUtente });
                cn.Close();
            }

            if (xx.Count() != 0)
                return _usr.GetRoles().Where(d => !xx.Contains(d.Code));
           
            return _usr.GetRoles();
           
        }

        public void RemoveElementoFromRole(string RoleID, string TipoElemento)
        {
            string sql = "Delete from ElementiInRoles where Tipo=@Tipo and RoleId=@RoleId";


            using (IDbConnection conn = new SqlConnection(_connDocs))
            {
                conn.Execute(sql, new { Tipo = TipoElemento , RoleId = RoleID});
                conn.Close();

            }
        }




        public void AddRoleToElemento(string RoleID, string TipoElemento)
        {

            using (IDbConnection cn = new SqlConnection(_connDocs))
            {
                cn.Open();


                string sql = "INSERT INTO [ElementiInRoles] ([RoleId], [Tipo]) Values (@Idruolo, @Tipo)";

                cn.Execute(sql, new { Idruolo = RoleID, Tipo = TipoElemento });
                cn.Close();
            }
        }


        public IEnumerable<Role> GetAllRolesForAllegato(string TipoAllegato)
        {
            IEnumerable<Role> tmpRuoli;
            IEnumerable<string> xx;

            using (IDbConnection cn = new SqlConnection(_connDocs))
            {
                string sql = "SELECT RoleId FROM [AllegatiInRoles] where [Tipo]=@Tipo ";
                xx = cn.Query<string>(sql, new { Tipo = TipoAllegato });
                cn.Close();
            }
            
            if (xx.Count() != 0) 
               return _usr.GetRoles().Where(d => xx.Contains(d.Code));
            return new List<Role>();
        }

        public IEnumerable<Role> RuoliNonAttivi_Allegato(string TipoAllegato)
        {
            IEnumerable<Role> tmpRuoli=new List<Role>();
            IEnumerable<string> xx;

            using (IDbConnection cn = new SqlConnection(_connDocs))
            {
                string sql = "SELECT RoleId FROM AllegatiInRoles where Tipo=@Tipo";

                xx = cn.Query<string>(sql, new { Tipo = TipoAllegato });  //, new { idutente = idUtente });
                cn.Close();
            }

            if (xx.Count() != 0)
                return _usr.GetRoles().Where(d => !xx.Contains(d.Code));

            return tmpRuoli;
        }

        public void RemoveAllegatoFromRole(string RoleID, string TipoAllegato)
        {
            string sql = "Delete from AllegatiInRoles where Tipo=@Tipo and RoleId=@RoleId";


            using (IDbConnection conn = new SqlConnection(_connDocs))
            {
                conn.Execute(sql, new { Tipo = TipoAllegato, RoleId = RoleID });
                conn.Close();

            }
        }




        public void AddRoleToAllegato(string RoleID, string TipoAllegato)
        {

            using (IDbConnection cn = new SqlConnection(_connDocs))
            {
                cn.Open();


                string sql = "INSERT INTO [AllegatiInRoles] ([RoleId], [Tipo]) Values (@Idruolo, @Tipo)";

                cn.Execute(sql, new { Idruolo = RoleID, Tipo = TipoAllegato });
                cn.Close();
            }
        }



    }


}


