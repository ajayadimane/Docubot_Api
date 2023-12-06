namespace DocuBot_Api.Models.RatingEngine_Models
{
    public partial class LoanDetails
    {
        public int Id { get; set; }
        public string Applno { get; set; }
        public int Loantypeid { get; set; }
        public int Loanamt { get; set; }
        public int Emi { get; set; }
        public int Assetval { get; set; }
        public int Tenor { get; set; }
        public int? Appid { get; set; }
        public DateTime? Approvaldt { get; set; }
        public DateTime? Disbdate { get; set; }
        public string? Status { get; set; }
        public int? Owncontrib { get; set; }
        public double? Intrate { get; set; }
        public string? Loanacno { get; set; }
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
        public int? Dependents { get; set; }
        public int? Expenses { get; set; }
        public string? RatingCalc { get; set;}

        //public int? Sanctionedamt { get; set; }
        //public double? Interestrate { get; set; }
        //public DateTime? Emistartdate { get; set; }
        //public int? Disposableinc { get; set; }
        //public int? Fixedexp { get; set; }
        //public int? Discretionexp { get; set; }
        //public int? Rating { get; set; }
        //public int? Prodtypeid { get; set; }
        //public string Profession { get; set; }
        //public int? Incomerangeid { get; set; }
        //public string Education { get; set; }
        //public int? Residencetypeid { get; set; }
        //public int? Age { get; set; }
        //public int? Dependents { get; set; }
        //public int? Yrsinresidence { get; set; }
        //public int? Yrsofearning { get; set; }
        //public double? Landholding { get; set; }
        //public double? Giloanratio { get; set; }
        //public double? Nondiscexpratio { get; set; }
        //public int? Loandefaults { get; set; }
        //public string Loanpurpose { get; set; }
    }
}
