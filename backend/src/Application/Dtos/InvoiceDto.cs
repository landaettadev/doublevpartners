namespace Application.Dtos;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<InvoiceDetailDto> Details { get; set; } = new();
}

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

public class InvoiceSearchRequest
{
    public string SearchType { get; set; } = string.Empty;
    public string SearchValue { get; set; } = string.Empty;
}

public class InvoiceNumberValidationResult
{
    public bool Exists { get; set; }
    public int InvoiceId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
}
