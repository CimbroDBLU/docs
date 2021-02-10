using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace dbluDealersConnector.Controllers
{
    /// <summary>
    /// Controller for managing accessing using Bearer\JWT tokens
    /// </summary>
    [Route("/Accesses")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        /// <summary>
        /// Injected configuration
        /// </summary>
        private readonly IConfiguration conf;
        /// <summary>
        /// Injected logger
        /// </summary>
        private readonly ILogger<AccessController> log;

        /// <summary>
        /// List of accepted users
        /// </summary>
        private readonly Dictionary<string, string> users;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nLog">Injected configuration</param>
        /// <param name="nConf"> List of accepted users</param>
        public AccessController(ILogger<AccessController> nLog,IConfiguration nConf )
        {
            log = nLog;
            conf = nConf;

            users = new Dictionary<string, string>();
            for (int i=1; i<=100;i++)
            {
                string user = conf[$"ExternalAccesses:Login_{i:D2}"];
                string pass = conf[$"ExternalAccesses:Password_{i:D2}"];
                if(user==null)
                    break;                
                users.Add(user, pass);
            }
            
            
        }

        /// <summary>
        /// Authenticate the corrent user and pass
        /// </summary>
        /// <param name="Login">Login of the user</param>
        /// <param name="Password">Password</param>
        /// <returns>A json object with code=0 (if ok) and the token under filed "Payload"</returns>
        [HttpGet("Login")]
        [Produces("application/json", "application/xml")]
        public IActionResult Login(string Login, string Password)
        {
            dynamic result = new ExpandoObject();

            log.LogInformation($"Access.Login: >> Requested login for {Login}");
            if(!users.ContainsKey(Login))
                {
                result.Code = 1;
                result.Payload = "User unknown";
                return Ok(result);
                }

            if(users[Login] !=Password)
            {
                result.Code = 2;
                result.Payload = "Wrong password";
                return Ok(result);
            }
            string TK = GetToken(Login);            
            result.Code = 0;
            result.Payload = TK;
            return Ok(result);
        }
        
        /// <summary>
        /// Aux method for creating token
        /// </summary>
        /// <param name="UserName">Username to insert into token</param>
        /// <returns>The token created</returns>
        private string GetToken(string UserName)
        {
            
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddDays(1),
            Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, UserName) }),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(conf["SigningKey"])), SecurityAlgorithms.HmacSha256Signature)
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
        }

        /// <summary>
        /// Test method that shows UserName in LOG if exsisting
        /// </summary>
        [HttpGet("TestMe")]
        [Authorize]
        public void TestMe()
        {           
            log.LogInformation($"Access.TestMe: >> Requested from { User.FindFirst(ClaimTypes.NameIdentifier)}");
        }
    }
}
