using System;
using System.Collections.Generic;

namespace DocuBot_Api.Rating_Models;

public partial class Intrate
{
    public int Rating { get; set; }

    public decimal? Hl { get; set; }

    public decimal? Cl { get; set; }

    public decimal? Twl { get; set; }

    public decimal? Pl { get; set; }

    public decimal? El { get; set; }
}
