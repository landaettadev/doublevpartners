using Microsoft.AspNetCore.Mvc;
using Infrastructure.Db;
using Application.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseTestController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICatalogRepository _catalogRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public DatabaseTestController(
        IDbConnectionFactory connectionFactory,
        ICatalogRepository catalogRepository,
        IInvoiceRepository invoiceRepository)
    {
        _connectionFactory = connectionFactory;
        _catalogRepository = catalogRepository;
        _invoiceRepository = invoiceRepository;
    }

    [HttpGet("connection")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var serverVersion = connection.ServerVersion;
            var databaseName = connection.Database;
            
            return Ok(new
            {
                message = "Conexión exitosa a la base de datos",
                serverVersion = serverVersion,
                databaseName = databaseName,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al conectar con la base de datos",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("catalog-test")]
    public async Task<IActionResult> TestCatalogRepository()
    {
        try
        {
            var clients = await _catalogRepository.GetClientsAsync();
            var products = await _catalogRepository.GetProductsAsync();
            
            return Ok(new
            {
                message = "Repositorio de catálogo funcionando correctamente",
                clientsCount = clients.Count(),
                productsCount = products.Count(),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error en el repositorio de catálogo",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("invoice-test")]
    public async Task<IActionResult> TestInvoiceRepository()
    {
        try
        {
            var invoices = await _invoiceRepository.GetInvoicesAsync(1, 5);
            
            return Ok(new
            {
                message = "Repositorio de facturas funcionando correctamente",
                invoicesCount = invoices.Count(),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error en el repositorio de facturas",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        var results = new List<object>();
        
        // Test database connection
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            results.Add(new { service = "Database Connection", status = "Healthy", database = connection.Database });
        }
        catch (Exception ex)
        {
            results.Add(new { service = "Database Connection", status = "Unhealthy", error = ex.Message });
        }

        // Test catalog repository
        try
        {
            var clients = await _catalogRepository.GetClientsAsync();
            results.Add(new { service = "Catalog Repository", status = "Healthy", clientsCount = clients.Count() });
        }
        catch (Exception ex)
        {
            results.Add(new { service = "Catalog Repository", status = "Unhealthy", error = ex.Message });
        }

        // Test invoice repository
        try
        {
            var invoices = await _invoiceRepository.GetInvoicesAsync(1, 5);
            results.Add(new { service = "Invoice Repository", status = "Healthy", invoicesCount = invoices.Count() });
        }
        catch (Exception ex)
        {
            results.Add(new { service = "Invoice Repository", status = "Unhealthy", error = ex.Message });
        }

        var allHealthy = results.All(r => r.GetType().GetProperty("status")?.GetValue(r)?.ToString() == "Healthy");
        
        return Ok(new
        {
            status = allHealthy ? "Healthy" : "Unhealthy",
            timestamp = DateTime.UtcNow,
            services = results
        });
    }
}
