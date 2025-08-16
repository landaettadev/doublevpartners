using Microsoft.AspNetCore.Mvc;

namespace SimpleApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    [HttpGet("clientes")]
    public IActionResult GetClientes()
    {
        var clientes = new[]
        {
            new { Id = 1, Name = "Cliente 1", Email = "cliente1@test.com", Phone = "123-456-7890" },
            new { Id = 2, Name = "Cliente 2", Email = "cliente2@test.com", Phone = "098-765-4321" },
            new { Id = 3, Name = "Cliente 3", Email = "cliente3@test.com", Phone = "555-123-4567" }
        };
        
        return Ok(clientes);
    }

    [HttpGet("productos")]
    public IActionResult GetProductos()
    {
        var productos = new[]
        {
            new { Id = 1, Name = "Producto 1", Description = "Descripción del producto 1", Price = 29.99m },
            new { Id = 2, Name = "Producto 2", Description = "Descripción del producto 2", Price = 49.99m },
            new { Id = 3, Name = "Producto 3", Description = "Descripción del producto 3", Price = 19.99m }
        };
        
        return Ok(productos);
    }
}

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateInvoice([FromBody] object payload)
    {
        var factura = new { 
            Id = new Random().Next(1000, 9999), 
            InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}",
            Message = "Factura creada exitosamente",
            Payload = payload,
            Timestamp = DateTime.UtcNow
        };
        
        return CreatedAtAction(nameof(GetInvoiceByNumber), new { numero = factura.InvoiceNumber }, factura);
    }

    [HttpGet("by-number/{numero}")]
    public IActionResult GetInvoiceByNumber(string numero)
    {
        var factura = new { 
            Id = 1, 
            InvoiceNumber = numero, 
            ClientName = "Cliente Test", 
            ClientId = 1,
            InvoiceDate = DateTime.Now,
            Subtotal = 100.00m,
            TaxAmount = 16.00m,
            Total = 116.00m,
            Status = "Active",
            Details = new[]
            {
                new { ProductId = 1, ProductName = "Producto 1", Quantity = 2, UnitPrice = 50.00m, Total = 100.00m }
            }
        };
        
        return Ok(factura);
    }

    [HttpGet]
    public IActionResult GetInvoices([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var facturas = new[]
        {
            new { Id = 1, InvoiceNumber = "INV-001", ClientName = "Cliente 1", Total = 116.00m, Status = "Active" },
            new { Id = 2, InvoiceNumber = "INV-002", ClientName = "Cliente 2", Total = 232.00m, Status = "Active" }
        };
        
        var result = new
        {
            Items = facturas,
            TotalRecords = 2,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = 1
        };
        
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            message = "API DoubleV Partners funcionando correctamente", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            endpoints = new[]
            {
                "GET /api/catalog/clientes",
                "GET /api/catalog/productos", 
                "POST /api/invoices",
                "GET /api/invoices/by-number/{numero}",
                "GET /api/invoices"
            }
        });
    }
}
