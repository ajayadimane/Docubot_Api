namespace DocuBot_Api.Models_Pq.RequestViewModels
{
    public class InsertLoanDetailsReq
    {
        public int Id { get; set; }
        public string Applno { get; set; }
        //public int Loantypeid { get; set; }
        public int Loanamt { get; set; }
        public int Emi { get; set; }
        public int Assetval { get; set; }
        public int Tenor { get; set; }
        //public int? Appid { get; set; }


        public DateTime? Approvaldate { get; set; }

        public DateTime? Disbdate { get; set; }
        //public string? Status { get; set; }
        public int? Owncontrib { get; set; }
        //public double? Intrate { get; set; }
        // public string? Loanacno { get; set; }
        public string? Loantype { get; set; }
        public int? Income { get; set; }
        public int? Permth { get; set; }
        public int? Taxpaid { get; set; }
        public decimal? Rir { get; set; }
        public int? Othemi { get; set; }
        public decimal? Lvr { get; set; }
        public int? Cibil { get; set; }
        public int? Bounced { get; set; }
        public int? Delayed { get; set; }
        public string? Custtype { get; set; }
        public int? Ccbal { get; set; }
        public DateTime? Emistartdate { get; set; }
        public int? Rating { get; set; }
        public int MaxRating { get; set; }
        public string RatingCalculatedDate { get; set; }
        public int? Dependents { get; set; }
        public int? Expenses { get; set; }
        

    }
}
