using System;
using System.Collections.Generic;

namespace DocuBot_Api.Rating_Models;

public partial class ExtractionKeyValue
{
    public int Id { get; set; }

    public int Docid { get; set; }

    public string? Bankname { get; set; }

    public string Accountholder { get; set; } = null!;

    public string? Address { get; set; }

    public DateTime? Date { get; set; }

    public string Accountno { get; set; } = null!;

    public string? Accountdescription { get; set; }

    public string? Branch { get; set; }

    public float? Drawingpower { get; set; }

    public float? Interestrate { get; set; }

    public float? Modbalance { get; set; }

    public string? Cifno { get; set; }

    public string? Ifsc { get; set; }

    public string? Micrcode { get; set; }

    public string? Nominationregistered { get; set; }

    public string? Balanceason { get; set; }

    public string? Balanceamount { get; set; }

    public string? Statementperiod { get; set; }

    public string? Cif { get; set; }

    public string? Bankaddress { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Opendate { get; set; }

    public string? Accountstatus { get; set; }

    public string? Branchcode { get; set; }

    public string? Productcode { get; set; }

    public string? Nomination { get; set; }

    public string? Statementperiodfrom { get; set; }

    public string? Statementperiodto { get; set; }

    public string? Bankaddress1 { get; set; }

    public string? Accounttype { get; set; }

    public string? Mobileno { get; set; }

    public string? Pincode { get; set; }

    public string? Currency { get; set; }

    public string? Statementdate { get; set; }

    public string? Bankaddress2 { get; set; }

    public string? Address3 { get; set; }

    public string? Bankaddress3 { get; set; }

    public string? Pan { get; set; }

    public string? Scheme { get; set; }

    public string? Jointholder { get; set; }

    public virtual Loadedfile Doc { get; set; } = null!;
}
