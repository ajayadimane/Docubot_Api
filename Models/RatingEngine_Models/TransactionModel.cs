namespace DocuBot_Api.Models.RatingEngine_Models
{
    public class TransactionModel
    {
        public DateTime TransactionTimestamp { get; set; }
        public DateTime ValueDate { get; set; }
        public string Narration { get; set; }
        public string Reference { get; set; }
        public string Amount { get; set; }
        public string CurrentBalance { get; set; }
        public string TxnId { get; set; }
        public string Mode { get; set; }
    }



}
