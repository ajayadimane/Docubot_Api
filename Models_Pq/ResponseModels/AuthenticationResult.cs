using DocuBot_Api.Models.User;

namespace DocuBot_Api.Models_Pq.ResponseModels
{
    public class AuthenticationResult
    {
        public LoginModel AuthenticatedUser { get; set; }
        public string ErrorMessage { get; set; }
    }
}
