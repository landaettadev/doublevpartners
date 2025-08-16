using Api.Models;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
            var invoice = await _invoiceService.CreateInvoiceAsync(request);
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
    {
        try
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<InvoiceListItemDto>>> GetInvoices(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _invoiceService.GetInvoicesAsync(page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<InvoiceListItemDto>>> SearchInvoices(
        [FromBody] InvoiceSearchRequest request)
    {
        try
        {
            var invoices = await _invoiceService.SearchInvoicesAsync(request.SearchType, request.SearchValue);
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("check-number/{invoiceNumber}")]
    public async Task<ActionResult<bool>> CheckInvoiceNumberExists(string invoiceNumber)
    {
        try
        {
            var exists = await _invoiceService.CheckInvoiceNumberExistsAsync(invoiceNumber);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
