using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Learning.Jwt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Learning.Jwt.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        public LoginController (IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [Authorize]
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value: " + id;
        }

        [HttpGet("userlogin/{username}/{pass}")]
        public IActionResult UserLogin(string username, string pass)
        {
            var login = new UserModel();
            login.UserName = username;
            login.Password = pass;
            IActionResult response = Unauthorized();


            var user = ActhenticateUser(login);
            if(user != null)
            {
                var tokenStr = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenStr });
            }

            return response;
        }

        [Authorize]
        [HttpPost("Post")]
        public string Post()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var claim = identity.Claims.ToList();
            var username = claim[0].Value;
            return username + " is logged";
        }

        private UserModel ActhenticateUser(UserModel login)
        {
            UserModel user = null;

            if (login.UserName.Equals("lapdc") && !string.IsNullOrEmpty(login.Password))
            {
                user = new UserModel
                {
                    UserName = "lapdc",
                    Email = "lapdc@gmail.com",
                    Name = "Cong Lap"
                };
            }

            return user;
        }

        private string GenerateJSONWebToken(UserModel userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserName),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials
                );

            var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodetoken;
        }
    }
}
