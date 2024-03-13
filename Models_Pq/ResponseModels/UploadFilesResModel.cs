namespace DocuBot_Api.Models_Pq.ResponseModels
{
    public class UploadFilesResModel
    {
        public string Appno { get; set; }
        public int Docid { get; set; }
        public string Filename { get; set; }
    }

    public class ExtractkeyvalResponse
    {

    }

    public class ExtractTransResModel 
    {
        public DateTime? TxnDate { get; set; }

        public string? ValueDate { get; set; }

        public string? Description { get; set; }

        public string? ChequeNumber { get; set; }

        public string? Amount { get; set; }

        public string? Debit { get; set; }

        public string? Credit { get; set; }

        public string? Balance { get; set; }
    }

    public class KeyvalRes
    {
        public string? Bankname { get; set; }

        public string Accountholder { get; set; }

        public string Address { get; set; }

        public DateTime? Date { get; set; }

        public string Accountno { get; set; }

        public string Accountdescription { get; set; }

        public string Branch { get; set; }

        public float? Modbalance { get; set; }

       public string? Cifno { get; set; }

        public string Ifsc { get; set; }

        public string Micrcode { get; set; }
        public float Drawingpower { get; set; }
        public float Interestrate { get; set; }
        public string? Cif { get; set; }

        public string Nominationregistered { get; set; }

        public string Balanceason { get; set; }

        public string Balanceamount { get; set; }

        public string Statementperiod { get; set; }

        
    }

    public class BankConfiguration
    {
        public List<string> Fields { get; set; }
    }

    public class TransBankConfig
    {
        public List<string> Fields { get; set; }
    }



}
