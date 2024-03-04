using DocuBot_Api.Context;
using DocuBot_Api.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DocuBot_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly DocubotDbContext _context;

        public TokenController(IConfiguration config, DocubotDbContext context)
        {
            _configuration = config;
            _context = context;
        }

        private LoginModel Authentication(LoginModel userData)
        {
            LoginModel _user = null;
            if(userData.Username == "admin" && userData.Password == "int123$%^")
            {
                _user = new LoginModel { Username = "Ajayy" };
            }
            return _user;
        }

        private string GenerateToken(LoginModel userData)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], null,
                  expires: DateTime.Now.AddMinutes(10),
                  signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);

        }

    


        [AllowAnonymous]
        [HttpPost]

        public IActionResult Login(LoginModel userData)
        {
            IActionResult response = Unauthorized();
            var user = Authentication(userData);
            if (user != null)
            {
                var token = GenerateToken(user);
                response = Ok(new { Token = token });
            }
            return response;
        }


    //    [HttpPost]
    //    public async Task<IActionResult> Post(UserInfo _userData)
    //    {
    //        if (_userData != null && _userData.UserName != null && _userData.Password != null)
    //        {
    //            var user = await GetUser(_userData.UserName, _userData.Password);

    //            if (user != null)
    //            {
    //                //create claims details based on the user information
    //                var claims = new[] {
    //                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
    //                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    //                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
    //                    new Claim("UserId", user.UserId.ToString()),
    //                    new Claim("DisplayName", user.DisplayName),
    //                    new Claim("UserName", user.UserName),
    //                    new Claim("Email", user.Email)
    //                };

    //                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
    //                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    //                var token = new JwtSecurityToken(
    //                    _configuration["Jwt:Issuer"],
    //                    _configuration["Jwt:Audience"],
    //                    claims,
    //                    expires: DateTime.UtcNow.AddMinutes(10),
    //                    signingCredentials: signIn);

    //                return Ok(new JwtSecurityTokenHandler().WriteToken(token));

    //            }
    //            else
    //            {
    //                return BadRequest("Invalid credentials");
    //            }
    //        }
    //        else
    //        {
    //            return BadRequest();
    //        }
    //    }

    //    private async Task<UserInfo> GetUser(string username, string password)
    //    {
    //        return await _context.UserInfos.FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);
    //    }

    //    [HttpPost("Add User")]
    //    public async Task<IActionResult> AddUser(UserInfo userInfo)
    //    {
    //        var User = await _context.UserInfos.AddAsync(userInfo);
    //        _context.SaveChanges();
    //        return Ok(User);
    //    }



    }
}
