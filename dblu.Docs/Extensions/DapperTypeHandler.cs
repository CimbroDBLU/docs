#if Framework48

#else
using dbluTools.Extensions;
#endif
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace dblu.Docs.Extensions
{

#if Framework48

    public static class ExtAttributesHelpers
    {
        public static ExtAttributes FromJson(this string json) => JsonConvert.DeserializeObject<ExtAttributes>(json);
        public static string ToJson(this ExtAttributes Ext) => JsonConvert.SerializeObject(Ext);
    }

    public class ExtAttributes : Dictionary<string, object>
    {

        public new object this[string key]
        {
            get
            {
                if (Keys.Contains(key))
                    return base[key];
                return null;
            }
            set
            {
                if (!Keys.Contains(key))
                    this.Add(key, value);
                else base[key] = value;
            }
        }


        public string this[string key, string Default]
        {
            get
            {
                if (Keys.Contains(key))
                    return base[key].ToString();

                return Default;
            }
        }

        public int this[string key, int Default]
        {
            get
            {
                if (Keys.Contains(key))
                    if (int.TryParse(key, out int outval))
                        return outval;
                return Default;
            }
        }

        public double this[string key, double Default]
        {
            get
            {
                if (Keys.Contains(key))
                    if (double.TryParse(key, out double outval))
                        return outval;
                return Default;
            }
        }

        public bool this[string key, bool Default]
        {
            get
            {
                if (Keys.Contains(key))
                    if (bool.TryParse(key, out bool outval))
                        return outval;
                return Default;
            }
        }


        public DateTime this[string key, DateTime Default]
        {
            get
            {
                if (Keys.Contains(key))
                    if (DateTime.TryParse(key, out DateTime outval))
                        return outval;
                return Default;
            }
        }

        public bool Contains(ExtAttributes lista)
        {
            foreach (var a in lista)
            {
                if (!this.Contains(a))
                {
                    return false;
                }
            }
            return true;
        }

        public void Set(string key, object value)
        {
            if (this.ContainsKey(key))
            {
                this[key] = value;
            }
            else
            {
                this.Add(key, value);
            }
        }
    }
#endif

    class ExtAttributesTypeHandler : ITypeHandler
    {
        public object Parse(Type destinationType, object value)
        {
            return JsonConvert.DeserializeObject<ExtAttributes>(value.ToString());
        }

        public void SetValue(IDbDataParameter parameter, object value)
        {
            parameter.Value = (value == null)
                ? (object)DBNull.Value
                : JsonConvert.SerializeObject(value);
            parameter.DbType = DbType.String;
        }
    }
}
