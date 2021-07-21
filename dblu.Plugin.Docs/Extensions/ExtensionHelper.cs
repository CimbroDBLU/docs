using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.Extensions
{
    /// <summary>
    ///  Class that contains extension methods 
    /// </summary>
    public static class ExtensionHelper
    {
        /// <summary>
        /// Get the type of user of a ClaimsPrincipal
        /// </summary>
        /// <param name="User">Claims principal to check</param>
        /// <returns>
        /// True if it is an admin
        /// </returns>
        public static bool IsAdmin(this ClaimsPrincipal User)
        {
            if (User == null) return false;

            List<Claim> ret = User.Identities.First().Claims.Where(c => (c.Type == ClaimTypes.Role && c.Value == "*SuperAdmin*")).ToList();
            if (ret.Count > 0)
                return true;

            //camunda non permette asterischi su nomi
            ret = User.Identities.First().Claims.Where(c => (c.Type == ClaimTypes.Role && c.Value == "SuperAdmin")).ToList();
            if (ret.Count > 0)
                return true;

            if (User.Identities.First().Name == "*SuperAdmin*")
                return true;

            return false;
        }
    }
}
