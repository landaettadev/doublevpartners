using Application.Dtos;

namespace Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto invoiceDto);
    Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
    Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber);
    Task<PagedResult<InvoiceListItemDto>> GetInvoicesAsync(int page, int pageSize);
    Task<IEnumerable<InvoiceListItemDto>> SearchInvoicesAsync(string searchType, string searchValue);
    Task<bool> CheckInvoiceNumberExistsAsync(string invoiceNumber);
}
