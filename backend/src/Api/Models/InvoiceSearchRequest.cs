using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class InvoiceSearchRequest
{
    [Required]
    [RegularExpression("^(Client|InvoiceNumber)$", ErrorMessage = "SearchType must be either 'Client' or 'InvoiceNumber'")]
    public string SearchType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string SearchValue { get; set; } = string.Empty;
}
