using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Registration.Models;

/// <summary>
/// Allows exchange of form information between the server and client sides of the app
/// </summary>
public class FormModel
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Cpf { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
}