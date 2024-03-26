using DocuBot_Api.Context;
using DocuBot_Api.Models.User;
using DocuBot_Api.Rating_Models;
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
        private readonly RatingContext _context;
        

        public TokenController(IConfiguration config, RatingContext context)
        {
            _configuration = config;
            _context = context;
        }

        private LoginModel Authentication(LoginModel userData)
        {
            // Query the database for the user with the provided username and password
            var userFromDatabase = _context.Users
                .FirstOrDefault(u => u.Username == userData.Username && u.Password == userData.Password);

            // If a matching user is found, create a LoginModel object for the authenticated user
            if (userFromDatabase != null)
            {
                return new LoginModel { Username = userFromDatabase.Username };
            }

            // If no matching user is found, return null or throw an exception indicating authentication failure
            return null;
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
        [HttpPost("Login")]

        public IActionResult Login(LoginModel userData)
        {
            IActionResult response = Unauthorized();
            var user = Authentication(userData);
            if (user != null)
            {
                var token = GenerateToken(user);
                response = Ok(new { code = "1", Token = token, message = "Login Success", status = "Success" });
            }
            else
            {
                // If user is not found, return a failure response
                response = new JsonResult(new { code = "0", message = "Invalid UserName or Password", status = "Failure" });
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

        [HttpPost("AddUser")]
        public async Task<ActionResult> InsertUser([FromBody] User user)
        {
            try
            {
                // Check if a user with the same username or email already exists
                if (await _context.Users.AnyAsync(u => u.Username == user.Username || u.Email == user.Email))
                {
                    return Conflict("User with the same username or email already exists.");
                }

                // Add the user to the context and save changes
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok("User details inserted successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception and return an internal server error response
                //_logger.LogError($"Exception occurred: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}
