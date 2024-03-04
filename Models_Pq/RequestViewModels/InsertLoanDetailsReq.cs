namespace DocuBot_Api.Models_Pq.RequestViewModels
{
    public class InsertLoanDetailsReq
    {
        public string Applno { get; set; }
      
        public int Loanamt { get; set; }

        public string? Loantype { get; set; }

        public int Emi { get; set; }
        public int Assetval { get; set; }
        public int Tenor { get; set; }
        
        public DateOnly? Approvaldate { get; set; }

        public DateOnly? Disbdate { get; set; }
     
        public int? Owncontrib { get; set; }
      
        public int? Income { get; set; }
        public int? Permth { get; set; }
        public int? Taxpaid { get; set; }
     
        public int? Othemi { get; set; }
        public decimal? Lvr { get; set; }
        public int? Cibil { get; set; }
       
        public string? Custtype { get; set; }
        public int? Ccbal { get; set; }

        public DateOnly? Emistartdate { get; set; }
    }
}
