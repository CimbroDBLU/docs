using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace dblu.Docs.Extensions
{
    public static class InheritHelper
    {
        public static void FillProperties<T, Tbase>(this T target, Tbase baseInstance)
        where T : Tbase
        {
            Type t = typeof(T);
            Type tb = typeof(Tbase);
            PropertyInfo[] properties = tb.GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                // Skip unreadable and writeprotected ones
                if (!pi.CanRead || !pi.CanWrite) continue;
                // Read value
                object value = pi.GetValue(baseInstance, null);
                // Get Property of target class
                PropertyInfo pi_target = t.GetProperty(pi.Name);
                // Write value to target
                pi_target.SetValue(target, value, null);
            }
        }

        public static void CopyProperties<T, TParent>(this T target, TParent sourceInstance)
            where TParent : class
            where T : class
        {
            var parentProperties = sourceInstance.GetType().GetProperties();
            var childProperties = target.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        childProperty.SetValue(target, parentProperty.GetValue(sourceInstance));
                        break;
                    }
                }
            }
        }
    }
}
