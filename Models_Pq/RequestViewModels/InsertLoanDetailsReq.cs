using System.Text.Json.Serialization;

namespace DocuBot_Api.Models_Pq.RequestViewModels
{
    public class InsertLoanDetailsReq
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("applno")]
        public string Applno { get; set; }

        [JsonPropertyName("loanamt")]
        public int Loanamt { get; set; }

        [JsonPropertyName("emi")]
        public int Emi { get; set; }

        [JsonPropertyName("assetval")]
        public int Assetval { get; set; }

        [JsonPropertyName("tenor")]
        public int Tenor { get; set; }

        [JsonPropertyName("approvaldate")]
        public DateTime? Approvaldate { get; set; }

        [JsonPropertyName("disbdate")]
        public DateTime? Disbdate { get; set; }

        [JsonPropertyName("owncontrib")]
        public int? Owncontrib { get; set; }

        [JsonPropertyName("loantype")]
        public string? Loantype { get; set; }

        [JsonPropertyName("income")]
        public int? Income { get; set; }

        [JsonPropertyName("permth")]
        public int? Permth { get; set; }

        [JsonPropertyName("taxpaid")]
        public int? Taxpaid { get; set; }

        [JsonPropertyName("rir")]
        public decimal? Rir { get; set; }

        [JsonPropertyName("othemi")]
        public int? Othemi { get; set; }

        [JsonPropertyName("lvr")]
        public decimal? Lvr { get; set; }

        [JsonPropertyName("cibil")]
        public int? Cibil { get; set; }

        [JsonPropertyName("bounced")]
        public int? Bounced { get; set; }

        [JsonPropertyName("delayed")]
        public int? Delayed { get; set; }

        [JsonPropertyName("custtype")]
        public string? Custtype { get; set; }

        [JsonPropertyName("ccbal")]
        public int? Ccbal { get; set; }

        [JsonPropertyName("emistartdate")]
        public DateTime? Emistartdate { get; set; }

        [JsonPropertyName("rating")]
        public int? Rating { get; set; }

        [JsonPropertyName("maxRating")]
        public int MaxRating { get; set; }

        [JsonPropertyName("ratingCalculatedDate")]
        public string RatingCalculatedDate { get; set; }

        [JsonPropertyName("dependents")]
        public int? Dependents { get; set; }

        [JsonPropertyName("expenses")]
        public int? Expenses { get; set; }
    }
}
