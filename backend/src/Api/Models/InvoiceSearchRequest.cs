using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class InvoiceSearchRequest
{
    [Required(ErrorMessage = "El tipo de búsqueda es requerido")]
    [RegularExpression(@"^(Client|InvoiceNumber)$", ErrorMessage = "El tipo de búsqueda debe ser 'Client' o 'InvoiceNumber'")]
    public string SearchType { get; set; } = string.Empty;

    [Required(ErrorMessage = "El valor de búsqueda es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El valor de búsqueda debe tener entre 2 y 100 caracteres")]
    public string SearchValue { get; set; } = string.Empty;
}
