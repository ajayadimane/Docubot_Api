using System.ComponentModel.DataAnnotations;

namespace DocuBot_Api.Models
{
    public class MasterTable
    {
        [Key]
        public int Id { get; set; }
        public int DocId { get; set; }
        public string Keywords { get; set; }
        public string Value { get; set; }
        public string ReturnKey { get; set; }
        public bool? Editable { get; set; }
    }
}
