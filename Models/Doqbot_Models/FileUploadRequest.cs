namespace DocuBot_Api.Models.Doqbot_Models
{
    public class FileUploadRequest
    {
        public string Username { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
