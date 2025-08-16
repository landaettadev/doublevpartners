using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "API funcionando correctamente", timestamp = DateTime.UtcNow });
    }

    [HttpGet("catalog/clientes")]
    public IActionResult GetClientes()
    {
        var clientes = new[]
        {
            new { Id = 1, Name = "Cliente 1", Email = "cliente1@test.com" },
            new { Id = 2, Name = "Cliente 2", Email = "cliente2@test.com" }
        };
        
        return Ok(clientes);
    }

    [HttpPost("invoices")]
    public IActionResult CreateInvoice([FromBody] object payload)
    {
        return Ok(new { message = "Factura creada", payload, timestamp = DateTime.UtcNow });
    }

    [HttpGet("invoices/by-number/{numero}")]
    public IActionResult GetInvoiceByNumber(string numero)
    {
        var factura = new { 
            Id = 1, 
            InvoiceNumber = numero, 
            ClientName = "Cliente Test", 
            Total = 100.00m 
        };
        
        return Ok(factura);
    }
}
