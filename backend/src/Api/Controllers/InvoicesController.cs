using Api.Models;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers;

/// <summary>
/// Controlador para gestión de facturas
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize] // Deshabilitado para la prueba técnica
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] InvoiceCreateRequest request)
    {
        try
        {
            // Validar el modelo
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(new 
                { 
                    error = "Error de validación del modelo",
                    details = errors,
                    timestamp = DateTime.UtcNow
                });
            }

            var invoiceDto = MapToInvoiceCreateDto(request);
            var invoice = await _invoiceService.CreateInvoiceAsync(invoiceDto);
            
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, new
            {
                message = "Factura creada exitosamente",
                invoice = invoice,
                timestamp = DateTime.UtcNow
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new 
            { 
                error = "Error de validación",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                error = "Error al crear la factura",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new 
                { 
                    error = "ID inválido",
                    message = "El ID de la factura debe ser mayor a 0",
                    timestamp = DateTime.UtcNow
                });
            }

            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound(new 
                { 
                    error = "Factura no encontrada",
                    message = $"No se encontró la factura con ID {id}",
                    timestamp = DateTime.UtcNow
                });

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                error = "Error al obtener la factura",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoices(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page <= 0)
            {
                return BadRequest(new 
                { 
                    error = "Página inválida",
                    message = "El número de página debe ser mayor a 0",
                    timestamp = DateTime.UtcNow
                });
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                return BadRequest(new 
                { 
                    error = "Tamaño de página inválido",
                    message = "El tamaño de página debe estar entre 1 y 100",
                    timestamp = DateTime.UtcNow
                });
            }

            var result = await _invoiceService.GetInvoicesAsync(page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                error = "Error al obtener las facturas",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> SearchInvoices(
        [FromBody] Application.Dtos.InvoiceSearchRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(new 
                { 
                    error = "Error de validación del modelo",
                    details = errors,
                    timestamp = DateTime.UtcNow
                });
            }

            var invoices = await _invoiceService.SearchInvoicesAsync(request);
            return Ok(new
            {
                message = "Búsqueda realizada exitosamente",
                count = invoices.Count(),
                invoices = invoices,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                error = "Error al buscar facturas",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("check-number/{invoiceNumber}")]
    public async Task<ActionResult<object>> CheckInvoiceNumberExists(string invoiceNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                return BadRequest(new 
                { 
                    error = "Número de factura inválido",
                    message = "El número de factura no puede estar vacío",
                    timestamp = DateTime.UtcNow
                });
            }

            var exists = await _invoiceService.CheckInvoiceNumberExistsAsync(invoiceNumber);
            return Ok(new
            {
                invoiceNumber = invoiceNumber,
                exists = exists,
                message = exists ? "El número de factura ya existe" : "El número de factura está disponible",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                error = "Error al verificar el número de factura",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("validate-number/{invoiceNumber}")]
    public async Task<ActionResult<object>> ValidateInvoiceNumber(string invoiceNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                return BadRequest(new 
                { 
                    error = "Número de factura inválido",
                    message = "El número de factura no puede estar vacío",
                    timestamp = DateTime.UtcNow
                });
            }

            var validationResult = new
            {
                invoiceNumber = invoiceNumber,
                isValid = false,
                errors = new List<string>(),
                suggestions = new List<string>()
            };

            var errors = new List<string>();
            var suggestions = new List<string>();

            // Validar longitud
            if (invoiceNumber.Length < 3)
            {
                errors.Add("El número de factura debe tener al menos 3 caracteres");
                suggestions.Add("Agregue más caracteres al número de factura");
            }
            else if (invoiceNumber.Length > 20)
            {
                errors.Add("El número de factura no puede exceder 20 caracteres");
                suggestions.Add("Reduzca la longitud del número de factura");
            }

            // Validar formato
            if (!System.Text.RegularExpressions.Regex.IsMatch(invoiceNumber, @"^[A-Za-z0-9\-_]+$"))
            {
                errors.Add("El número de factura solo puede contener letras, números, guiones y guiones bajos");
                suggestions.Add("Remueva caracteres especiales no permitidos");
            }

            // Verificar si ya existe
            var exists = await _invoiceService.CheckInvoiceNumberExistsAsync(invoiceNumber);
            if (exists)
            {
                errors.Add($"El número de factura '{invoiceNumber}' ya existe");
                suggestions.Add("Use un número de factura diferente");
            }

            var isValid = !errors.Any();

            return Ok(new
            {
                invoiceNumber = invoiceNumber,
                isValid = isValid,
                errors = errors,
                suggestions = suggestions,
                exists = exists,
                message = isValid ? "El número de factura es válido y está disponible" : "El número de factura tiene errores de validación",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                error = "Error al validar el número de factura",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("by-number/{invoiceNumber}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoiceByNumber(string invoiceNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                return BadRequest(new 
                { 
                    error = "Número de factura inválido",
                    message = "El número de factura no puede estar vacío",
                    timestamp = DateTime.UtcNow
                });
            }

            var invoice = await _invoiceService.GetInvoiceByNumberAsync(invoiceNumber);
            if (invoice == null)
                return NotFound(new 
                { 
                    error = "Factura no encontrada",
                    message = $"No se encontró la factura con número {invoiceNumber}",
                    timestamp = DateTime.UtcNow
                });

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                error = "Error al obtener la factura",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    private static InvoiceCreateDto MapToInvoiceCreateDto(InvoiceCreateRequest request)
    {
        var subtotal = request.Details.Sum(d => d.Quantity * d.UnitPrice);
        var taxAmount = subtotal * 0.19m; // 19% IVA
        var total = subtotal + taxAmount;

        return new InvoiceCreateDto
        {
            InvoiceNumber = request.InvoiceNumber,
            ClientId = request.ClientId,
            InvoiceDate = request.InvoiceDate,
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            Total = total,
            Details = request.Details.Select(d => new InvoiceDetailDto
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Total = d.Quantity * d.UnitPrice
            }).ToList()
        };
    }
}
