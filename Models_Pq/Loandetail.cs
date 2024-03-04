using System;
using System.Collections.Generic;

namespace DocuBot_Api.Models_Pq;

public partial class Loandetail
{
    public int Id { get; set; }

    public string Applno { get; set; } = null!;

    public int? Loanamt { get; set; }

    public int? Emi { get; set; }

    public int? Assetval { get; set; }

    public int? Tenor { get; set; }

    public DateTime? Approvaldate { get; set; }

    public DateTime? Disbdate { get; set; }

    public int? Owncontrib { get; set; }

    public string? Loantype { get; set; }

    public double? Income { get; set; }

    public double? Permth { get; set; }

    public double? Taxpaid { get; set; }

    public decimal? Rir { get; set; }

    public int? Othemi { get; set; }

    public decimal? Lvr { get; set; }

    public int? Cibil { get; set; }

    public int? Bounced { get; set; }

    public int? Delayed { get; set; }

    public string? Custtype { get; set; }

    public int? Ccbal { get; set; }

    public DateTime? Emistartdate { get; set; }

    public int? Sanctionedamt { get; set; }

    public double? Interestrate { get; set; }

    public int? Disposableinc { get; set; }

    public int? Rating { get; set; }

    public int? Dependents { get; set; }

    public double? Expenses { get; set; }

    public string? Ratingcalc { get; set; }
}
