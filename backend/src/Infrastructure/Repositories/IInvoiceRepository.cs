using Application.Dtos;

namespace Infrastructure.Repositories;

public interface IInvoiceRepository
{
    Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto invoiceDto);
    Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
    Task<IEnumerable<InvoiceListItemDto>> GetInvoicesAsync(int page, int pageSize);
    Task<IEnumerable<InvoiceListItemDto>> SearchInvoicesAsync(string searchType, string searchValue);
    Task<bool> CheckInvoiceNumberExistsAsync(string invoiceNumber);
    Task<int> GetTotalInvoicesCountAsync();
}
