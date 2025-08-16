using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class InvoiceCreateRequest
{
    [Required]
    [StringLength(20)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public int ClientId { get; set; }

    [Required]
    public DateTime InvoiceDate { get; set; }

    [Required]
    public List<InvoiceDetailRequest> Details { get; set; } = new();
}

public class InvoiceDetailRequest
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}
