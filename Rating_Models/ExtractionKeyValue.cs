using System;
using System.Text.Json.Serialization;

namespace DocuBot_Api.Rating_Models;

public partial class ExtractionKeyValue
{
    public int Id { get; set; }

    public int Docid { get; set; }

    [JsonPropertyName("BankName")]
    public string? Bankname { get; set; }

    [JsonPropertyName("AccountHolder")]
    public string Accountholder { get; set; } = null!;

    [JsonPropertyName("Address")]
    public string? Address { get; set; }

    [JsonPropertyName("Date")]
    public DateTime? Date { get; set; }

    [JsonPropertyName("AccountNo")]
    public string? Accountno { get; set; }

    [JsonPropertyName("AccountDescription")]
    public string? Accountdescription { get; set; }

    [JsonPropertyName("Branch")]
    public string? Branch { get; set; }

    [JsonPropertyName("DrawingPower")]
    public float? Drawingpower { get; set; }

    [JsonPropertyName("InterestRate")]
    public float? Interestrate { get; set; }

    [JsonPropertyName("ModBalance")]
    public float? Modbalance { get; set; }

    [JsonPropertyName("CIFNo")]
    public string? Cifno { get; set; }

    [JsonPropertyName("IFSC")]
    public string? Ifsc { get; set; }

    [JsonPropertyName("MICRCode")]
    public string? Micrcode { get; set; }

    [JsonPropertyName("NominationRegistered")]
    public string? Nominationregistered { get; set; }

    [JsonPropertyName("BalanceAsOn")]
    public string? Balanceason { get; set; }

    [JsonPropertyName("BalanceAmount")]
    public string? Balanceamount { get; set; }

    [JsonPropertyName("StatementPeriod")]
    public string? Statementperiod { get; set; }

    [JsonPropertyName("CIF")]
    public string? Cif { get; set; }

    [JsonPropertyName("BankAddress")]
    public string? Bankaddress { get; set; }

    [JsonPropertyName("Address1")]
    public string? Address1 { get; set; }

    [JsonPropertyName("Address2")]
    public string? Address2 { get; set; }

    [JsonPropertyName("City")]
    public string? City { get; set; }

    [JsonPropertyName("State")]
    public string? State { get; set; }

    [JsonPropertyName("Phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("Email")]
    public string? Email { get; set; }

    [JsonPropertyName("DOB")]
    public string? DOB { get; set; }

    [JsonPropertyName("OpenDate")]
    public string? Opendate { get; set; }

    [JsonPropertyName("AccountStatus")]
    public string? Accountstatus { get; set; }

    [JsonPropertyName("BranchCode")]
    public string? Branchcode { get; set; }

    [JsonPropertyName("ProductCode")]
    public string? Productcode { get; set; }

    [JsonPropertyName("Nomination")]
    public string? Nomination { get; set; }

    [JsonPropertyName("StatementPeriodFrom")]
    public string? Statementperiodfrom { get; set; }

    [JsonPropertyName("StatementPeriodTo")]
    public string? Statementperiodto { get; set; }

    [JsonPropertyName("BankAddress1")]
    public string? Bankaddress1 { get; set; }

    [JsonPropertyName("AccountType")]
    public string? Accounttype { get; set; }

    [JsonPropertyName("MobileNo")]
    public string? Mobileno { get; set; }

    [JsonPropertyName("PinCode")]
    public string? Pincode { get; set; }

    [JsonPropertyName("Currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("StatementDate")]
    public string? Statementdate { get; set; }

    [JsonPropertyName("BankAddress2")]
    public string? Bankaddress2 { get; set; }

    [JsonPropertyName("Address3")]
    public string? Address3 { get; set; }

    [JsonPropertyName("BankAddress3")]
    public string? Bankaddress3 { get; set; }

    [JsonPropertyName("PAN")]
    public string? Pan { get; set; }

    [JsonPropertyName("Scheme")]
    public string? Scheme { get; set; }

    [JsonPropertyName("JointHolder")]
    public string? Jointholder { get; set; }

    [JsonPropertyName("ODLimit")]
    public string? Odlimit { get; set; }

    [JsonPropertyName("Doc")]
    public virtual Loadedfile Doc { get; set; } = null!;
}
