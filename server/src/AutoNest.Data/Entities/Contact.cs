using System.ComponentModel.DataAnnotations;

namespace AutoNest.Data.Entities;

public sealed class Contact
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    [MaxLength(40)]
    public string Type { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Value { get; set; } = string.Empty;
}
