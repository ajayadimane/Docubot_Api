using System;
using System.Collections.Generic;

namespace DocuBot_Api.Models_Pq;

public partial class ExtractTransactionDatum
{
    public int Id { get; set; }

    public int Docid { get; set; }

    public string? SerialNo { get; set; }

    public string? TransactionId { get; set; }

    public DateTime? TxnDate { get; set; }

    public string? ValueDate { get; set; }

    public string? Description { get; set; }

    public string? ChequeNumber { get; set; }

    public string? Amount { get; set; }

    public string? Debit { get; set; }

    public string? Credit { get; set; }

    public string? Balance { get; set; }

    public string? InitBr { get; set; }

    public virtual Loadedfile Doc { get; set; } = null!;
}
