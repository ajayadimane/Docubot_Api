using System;
using System.Collections.Generic;

namespace DocuBot_Api.Rating_Models;

public partial class Critlookup
{
    public int Id { get; set; }

    public string? Critcat { get; set; }

    public int Rangelo { get; set; }

    public int Rangehi { get; set; }

    public decimal Points { get; set; }

    public decimal Weight { get; set; }
}
