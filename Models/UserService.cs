//using DocumentFormat.OpenXml.Spreadsheet;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace DocuBot_Api.Models
//{
//    public class UserService : IUserService
//    {

//        public readonly IConfiguration _configuration;
//        private readonly UsersDB _db;
//        public UserService(IConfiguration configuration, UsersDB usersDB)
//        {
//            this._configuration = configuration;
//            _db = usersDB;
//        }
//        public List<string> Login(string email, string password)
//        {
//            List<string> details = new List<string>();

//            var authSettingsSection = _configuration.GetSection("AuthorizationSettings");
//            var secretkey = authSettingsSection.GetValue<string>("Secret");
//            var timeout = authSettingsSection.GetValue<int>("TokenTimeoutMinutes");

//            try
//            {
//                Users userdetails = _db.GetUserDetails(email, password);
//                // return null if user not found
//                if (userdetails != null)
//                {
//                    // authentication successful so generate jwt token
//                    var tokenHandler = new JwtSecurityTokenHandler();
//                    var key = Encoding.ASCII.GetBytes(secretkey);

//                    string sessionid = _db.RandomString(5);
//                    var tokenDescriptor = new SecurityTokenDescriptor
//                    {
//                        Subject = new ClaimsIdentity(new Claim[]
//                        {
//                            new Claim(ClaimTypes.Name, userdetails.UserName),
//                            new Claim(ClaimTypes.Role, userdetails.Role),
//                            new Claim("UserId", userdetails.UserId.ToString()),
//                            new Claim("SessionID", sessionid)
//                        }),
//                        Expires = DateTime.UtcNow.AddMinutes(timeout),
//                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//                    };

//                    var token = tokenHandler.CreateToken(tokenDescriptor);

//                    //insert generated token to DB
//                    _db.InsertIntoTokenDetailsTable(userdetails, token);
//                    string tokenstr = tokenHandler.WriteToken(token);
//                    details.Add(tokenstr);
//                    details.Add(userdetails.Role);
//                    details.Add(userdetails.UserId.ToString());
//                    details.Add(userdetails.Name);
//                }
//                return details;
//            }
//            catch (Exception ex)
//            {
//                details.Add(ex.ToString());
//                return details;
//            }
//        }
//    }

//}

