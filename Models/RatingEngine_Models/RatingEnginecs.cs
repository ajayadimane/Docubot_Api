using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace DocuBot_Api.Models.RatingEngine_Models
{
    public class RatingEnginecs
    {
    }


    public class Requestforrating
    {
        //public int DocumentId { get; set; }
        public string ApplNo { get; set; }
    }

  
    public class LoanDocuments
    {
      
        public List<int> DocId { get; set; }
        public string LoanApplNo { get; set; }
    }


    public class LoanApplicationNo
    {
        public string ApplNo { get; set; }
    }


    public class ExtractionRequest
    {
        public int DocumentId { get; set; }
    }



    public class LoanSchedule
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("period")]
        public int Period { get; set; }

        [JsonPropertyName("payDate")]
        public DateTime PayDate { get; set; }

        [JsonPropertyName("payment")]
        public decimal Payment { get; set; }

        [JsonPropertyName("currentBalance")]
        public decimal Current_Balance { get; set; }

        [JsonPropertyName("interest")]
        public decimal Interest { get; set; }

        [JsonPropertyName("principal")]
        public decimal Principal { get; set; }
    }


    public class LoanDetailsMod
    {
        public string Applno { get; set; }
        // public int Loantypeid { get; set; }
        public int Loanamt { get; set; }

        public string? Loantype { get; set; }

        public int Emi { get; set; }
        public int Assetval { get; set; }
        public int Tenor { get; set; }
        //public int? Appid { get; set; }
           
        public DateTime? Approvaldate { get; set; }

        public DateTime? Disbdate { get; set; }
        //public string? Status { get; set; }
        public int? Owncontrib { get; set; }
        //public double? Intrate { get; set; }
        //public string? Loanacno { get; set; }
       
        public int? Income { get; set; }
        public int? Permth { get; set; }
        public int? Taxpaid { get; set; }
        //public decimal? Rir { get; set; }
        public int? Othemi { get; set; }
        public decimal? Lvr { get; set; }
        public int? Cibil { get; set; }
        //public int? Bounced { get; set; }
        //public int? Delayed { get; set; }
        public string? Custtype { get; set; }
        public int? Ccbal { get; set; }

        public DateTime? Emistartdate { get; set; }
        //public int? Rating { get; set; }
        //public int? Dependents { get; set; }
        //public int? Expenses { get; set; }
       // public string? RatingCalc { get; set; }


        //public int? Sanctionedamt { get; set; }
        //public double? Interestrate { get; set; }
        //public DateTime? Emistartdate { get; set; }
        //public int? Disposableinc { get; set; }
        //public int? Fixedexp { get; set; }
        //public int? Discretionexp { get; set; }

        //public int? Prodtypeid { get; set; }
        //public string Profession { get; set; }
        //public int? Incomerangeid { get; set; }
        //public string Education { get; set; }
        //public int? Residencetypeid { get; set; }
        //public int? Age { get; set; }

        //public int? Yrsinresidence { get; set; }
        //public int? Yrsofearning { get; set; }
        //public double? Landholding { get; set; }
        //public double? Giloanratio { get; set; }
        //public double? Nondiscexpratio { get; set; }
        //public int? Loandefaults { get; set; }
        //public string Loanpurpose { get; set; }
    }


    public class XmlKeyVal
    {
        public string XmlParentNode { get; set; }
        public string XmlKey { get; set; }
        public string XmlValue { get; set; }
    }

    public class XmlTransDetails
    {
        public decimal Amount { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Mode { get; set; }
        public string Narration { get; set; }
        public string Reference { get; set; }
        public string TransactionTimestamp { get; set; }
        public string Txnid { get; set; }
        public string TxnType { get; set; }
        public string Valuedate { get; set; }
    }

    public class XmlProcessorResult
    {
        //public List<XmlKeyVal> KeyValues { get; set; }
        public List<XmlTransDetails> TransactionDetails { get; set; }
    }

    public class UpdateLoadDetINsql
    {
        public string Applno { get; set; }
        public int rating { get; set; }
        public int income { get; set; }
        public int expenses { get; set; }
    }


}
