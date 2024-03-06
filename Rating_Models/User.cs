using System;
using System.Collections.Generic;

namespace DocuBot_Api.Rating_Models;

public partial class User
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }
}
