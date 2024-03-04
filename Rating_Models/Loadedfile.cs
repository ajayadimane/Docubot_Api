using System;
using System.Collections.Generic;

namespace DocuBot_Api.Rating_Models;

public partial class Loadedfile
{
    public int Id { get; set; }

    public string? Docname { get; set; }

    public string? Applno { get; set; }

    public virtual ICollection<ExtractTransactionDatum> ExtractTransactionData { get; set; } = new List<ExtractTransactionDatum>();

    public virtual ICollection<ExtractionKeyValue> ExtractionKeyValues { get; set; } = new List<ExtractionKeyValue>();
}
