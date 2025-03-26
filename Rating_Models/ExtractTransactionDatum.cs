using System;
using System.Text.Json.Serialization;

namespace DocuBot_Api.Rating_Models
{
    public partial class ExtractTransactionDatum
    {
        public int Id { get; set; }

        public int Docid { get; set; }

        [JsonPropertyName("SeriaNo")]
        public string? SerialNo { get; set; }

        [JsonPropertyName("TransactionId")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("TxnDate")]
        public DateTime? TxnDate { get; set; }

        [JsonPropertyName("ValueDate")]
        public string? ValueDate { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("ChequeNumber")]
        public string? ChequeNumber { get; set; }

        [JsonPropertyName("Amount")]
        public string? Amount { get; set; }

        [JsonPropertyName("Debit")]
        public string? Debit { get; set; }

        [JsonPropertyName("Credit")]
        public string? Credit { get; set; }

        [JsonPropertyName("Balance")]
        public string? Balance { get; set; }

        [JsonPropertyName("InitBr")]
        public string? InitBr { get; set; }

        [JsonPropertyName("BankName")]
        public string? Bankname { get; set; }

        [JsonPropertyName("RemitterBranch")]
        public string? RemitterBranch { get; set; }

        [JsonPropertyName("Mode")]
        public string? Mode { get; set; }

        [JsonPropertyName("Type")]
        public string? Type { get; set; }

        public virtual Loadedfile Doc { get; set; } = null!;
    }
}
