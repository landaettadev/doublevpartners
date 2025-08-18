using Application.Dtos;

namespace Application.Interfaces;

public interface IInvoiceRepository
{
    Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto invoiceDto);
    Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
    Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber);
    Task<IEnumerable<InvoiceDto>> GetInvoicesAsync(int page = 1, int pageSize = 10);
    Task<IEnumerable<InvoiceDto>> SearchInvoicesAsync(InvoiceSearchRequest searchRequest);
    Task<bool> CheckInvoiceNumberExistsAsync(string invoiceNumber);
    Task<InvoiceNumberValidationResult> CheckInvoiceNumberWithDetailsAsync(string invoiceNumber);
}
