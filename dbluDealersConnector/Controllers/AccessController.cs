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
    [Route("/Accesses")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        private readonly IConfiguration conf;
        private readonly ILogger<AccessController> log;

        public AccessController(ILogger<AccessController> nLog,IConfiguration nConf )
        {
            log = nLog;
            conf = nConf;
        }

        [HttpGet("Login")]
        public HttpResponseMessage Login(string UserName, string Password)
        {
            log.LogInformation($"Access.Login: >> Requested login for {UserName}");            
            string TK = GetToken(UserName);
            log.LogInformation($"Access.Login: << Token is {TK}");

            dynamic result = new ExpandoObject(); 
            result.Code = 0;
            result.Payload = TK;
            return Ok(result);
        }
        
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

        [HttpGet("TestMe")]
        [Authorize]
        public void TestMe()
        {           
            log.LogInformation($"Access.TestMe: >> Requested from { User.FindFirst(ClaimTypes.NameIdentifier)}");
        }
    }
}
