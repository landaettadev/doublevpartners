using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class InvoiceCreateRequest
{
    [Required(ErrorMessage = "El número de factura es requerido")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "El número de factura debe tener entre 3 y 20 caracteres")]
    [RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "El número de factura solo puede contener letras, números, guiones y guiones bajos")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cliente es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del cliente debe ser válido")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "La fecha de factura es requerida")]
    [DataType(DataType.Date)]
    public DateTime InvoiceDate { get; set; }

    [Required(ErrorMessage = "Los detalles de la factura son requeridos")]
    [MinLength(1, ErrorMessage = "La factura debe tener al menos un producto")]
    public List<InvoiceDetailRequest> Details { get; set; } = new();
}

public class InvoiceDetailRequest
{
    [Required(ErrorMessage = "El producto es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser válido")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "El precio unitario es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")]
    public decimal UnitPrice { get; set; }
}
