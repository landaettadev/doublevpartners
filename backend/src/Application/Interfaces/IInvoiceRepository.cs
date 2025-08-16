using Application.Dtos;

namespace Application.Interfaces;

public interface IInvoiceRepository
{
    Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto invoiceDto);
    Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
    Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber);
    Task<IEnumerable<InvoiceListItemDto>> GetInvoicesAsync(int page, int pageSize);
    Task<IEnumerable<InvoiceListItemDto>> SearchInvoicesAsync(string searchType, string searchValue);
    Task<bool> CheckInvoiceNumberExistsAsync(string invoiceNumber);
    Task<int> GetTotalInvoicesCountAsync();
}
