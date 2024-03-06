using DocuBot_Api.Rating_Models;
using System;
using System.Collections.Generic;

namespace DocuBot_Api.Models_Pq;

public partial class Loadedfile
{
    public int Id { get; set; }

    public string? Docname { get; set; }

    public string? Applno { get; set; }

    public virtual ICollection<ExtractTransactionDatum> ExtractTransactionData { get; set; } = new List<ExtractTransactionDatum>();

    public virtual ICollection<ExtractionKeyValue> ExtractionKeyValues { get; set; } = new List<ExtractionKeyValue>();
}

public class GetLoadedFilesModel
{
    public int Id { get; set; }

    public string? Docname { get; set; }

    public string? Applno { get; set; }
}
