using Application.Dtos;
using Application.Interfaces;
using Common.Errors;

namespace Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICatalogRepository _catalogRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository, ICatalogRepository catalogRepository)
    {
        _invoiceRepository = invoiceRepository;
        _catalogRepository = catalogRepository;
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto invoiceDto)
    {
        // Validar que el número de factura sea único
        var exists = await _invoiceRepository.CheckInvoiceNumberExistsAsync(invoiceDto.InvoiceNumber);
        if (exists)
        {
            throw new DomainException($"El número de factura {invoiceDto.InvoiceNumber} ya existe.");
        }

        // Validar que el cliente existe
        var client = await _catalogRepository.GetClientByIdAsync(invoiceDto.ClientId);
        if (client == null)
        {
            throw new DomainException($"El cliente con ID {invoiceDto.ClientId} no existe.");
        }

        // Validar que todos los productos existen
        foreach (var detail in invoiceDto.Details)
        {
            var product = await _catalogRepository.GetProductByIdAsync(detail.ProductId);
            if (product == null)
            {
                throw new DomainException($"El producto con ID {detail.ProductId} no existe.");
            }
        }

        // Crear la factura
        var invoice = await _invoiceRepository.CreateInvoiceAsync(invoiceDto);
        return invoice;
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
    {
        return await _invoiceRepository.GetInvoiceByIdAsync(id);
    }

    public async Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        return await _invoiceRepository.GetInvoiceByNumberAsync(invoiceNumber);
    }

    public async Task<PagedResult<InvoiceListItemDto>> GetInvoicesAsync(int page, int pageSize)
    {
        var invoices = await _invoiceRepository.GetInvoicesAsync(page, pageSize);
        var totalRecords = await _invoiceRepository.GetTotalInvoicesCountAsync();

        return new PagedResult<InvoiceListItemDto>
        {
            Items = invoices,
            TotalRecords = totalRecords,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<InvoiceListItemDto>> SearchInvoicesAsync(string searchType, string searchValue)
    {
        if (string.IsNullOrWhiteSpace(searchType) || string.IsNullOrWhiteSpace(searchValue))
        {
            throw new ArgumentException("SearchType y SearchValue son requeridos.");
        }

        if (searchType != "Client" && searchType != "InvoiceNumber")
        {
            throw new ArgumentException("SearchType debe ser 'Client' o 'InvoiceNumber'.");
        }

        return await _invoiceRepository.SearchInvoicesAsync(searchType, searchValue);
    }

    public async Task<bool> CheckInvoiceNumberExistsAsync(string invoiceNumber)
    {
        return await _invoiceRepository.CheckInvoiceNumberExistsAsync(invoiceNumber);
    }
}
