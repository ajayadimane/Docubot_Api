using System;
using System.Collections.Generic;

namespace DocuBot_Api.Rating_Models;

using System;
using System.Text.Json.Serialization;

public partial class Loandetail
{
    public int Id { get; set; }

    [JsonPropertyName("applno")]
    public string Applno { get; set; } = null!;

    [JsonPropertyName("loanamt")]
    public int? Loanamt { get; set; }

    [JsonPropertyName("emi")]
    public int? Emi { get; set; }

    [JsonPropertyName("assetval")]
    public int? Assetval { get; set; }

    [JsonPropertyName("tenor")]
    public int? Tenor { get; set; }

    [JsonPropertyName("approvaldate")]
    public DateTime? Approvaldate { get; set; }

    [JsonPropertyName("disbdate")]
    public DateTime? Disbdate { get; set; }

    [JsonPropertyName("owncontrib")]
    public int? Owncontrib { get; set; }

    [JsonPropertyName("loantype")]
    public string? Loantype { get; set; }

    [JsonPropertyName("income")]
    public double? Income { get; set; }

    [JsonPropertyName("permth")]
    public double? Permth { get; set; }

    [JsonPropertyName("taxpaid")]
    public double? Taxpaid { get; set; }

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

    [JsonPropertyName("sanctionedamt")]
    public int? Sanctionedamt { get; set; }

    [JsonPropertyName("interestrate")]
    public double? Interestrate { get; set; }

    [JsonPropertyName("disposableinc")]
    public int? Disposableinc { get; set; }

    [JsonPropertyName("rating")]
    public int? Rating { get; set; }

    [JsonPropertyName("dependents")]
    public int? Dependents { get; set; }

    [JsonPropertyName("expenses")]
    public double? Expenses { get; set; }

    [JsonPropertyName("ratingcalc")]
    public string? Ratingcalc { get; set; }
}

