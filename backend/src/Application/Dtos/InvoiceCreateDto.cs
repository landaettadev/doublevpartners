namespace Application.Dtos;

public class InvoiceCreateDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public List<InvoiceDetailDto> Details { get; set; } = new();
}

public class InvoiceDetailDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}
