using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
//using dblu.Portale.Plugin.Docs.Models;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;

namespace dblu.Portale.Plugin.Docs.Services
{
    public class DocsService

    {
        public IConfiguration _config { get; set; }
        public CategorieServices categorie { get; set; }

        public string ServizioSoggetti = "";

        public DocsService(IConfiguration config)
        {
            this._config = config;
            try {
            string connstring = _config.GetConnectionString("dblu.Docs");
            this.categorie = new CategorieServices(connstring);
                ServizioSoggetti = _config["Docs:ServizioSoggetti"];
            }
            catch { }
        }

    }

    public class CategorieServices
    {

        private string _conn = "";

        public CategorieServices(string conn)
        {
            _conn = conn;
        }
        //public List<CategoriaFascicolo> Read()
        //{
        //    using (var cn = new SqlConnection(_conn))
        //    {
        //        return cn.QueryAsync<CategoriaFascicolo>("select * from Categorie").Result.ToList();
        //    }
        //}

        //public CategoriaFascicolo Read(string Codice)
        //{
        //    using (var cn = new SqlConnection(_conn))
        //    {
        //        return  cn.QueryFirstOrDefaultAsync<CategoriaFascicolo>("select * from Categorie where Codice=@Codice", new { Codice= Codice }).Result;
        //    }
        //}

        //public bool Update(CategoriaFascicolo c)
        //{
        //    using (var cn = new SqlConnection(_conn))
        //    {
        //        try {

        //            string sql = "if exists(select codice from categorie where codice=@Codice) ";
        //            sql += "UPDATE Categorie set Descrizione=@Descrizione, Attributi = @Attributi where Codice=@Codice ";
        //            sql += "else ";
        //            sql += " INSERT INTO Categorie (Codice, Descrizione, Attributi) Values (@Codice, @Descrizione , @Attributi )";

        //                cn.ExecuteAsync(sql, c);
        //          }
        //        catch (Exception ex)
        //        {
        //            return false;
        //        }
        //        return true;
        //    }
        //}
        public bool Delete(string Codice)
        {
            using (var cn = new SqlConnection(_conn))
            {
                try
                {

                    string sql = "DELETE from Categorie  where Codice=@Codice ";
                    cn.ExecuteAsync(sql, new { Codice= Codice});
                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }
        }


    }
}
