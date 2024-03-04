using System;
using System.Collections.Generic;

namespace DocuBot_Api.Rating_Models;

public partial class Lbcriterion
{
    public int Id { get; set; }

    public string? CritSource { get; set; }

    public string? CritCat { get; set; }

    public string? CritRegex { get; set; }

    public string? Descripn { get; set; }

    public string? Formula { get; set; }

    public string? Cycle { get; set; }

    public decimal? Multiplier { get; set; }
}
