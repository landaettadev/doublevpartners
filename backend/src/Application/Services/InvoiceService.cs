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

    public async Task<IEnumerable<InvoiceDto>> GetInvoicesAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            if (page < 1)
            {
                throw new ValidationException(
                    "El número de página debe ser mayor a 0",
                    "Page",
                    "El número de página debe ser un valor positivo.",
                    new { Page = page, MinPage = 1 }
                );
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ValidationException(
                    "El tamaño de página debe estar entre 1 y 100",
                    "PageSize",
                    "El tamaño de página debe ser un valor entre 1 y 100.",
                    new { PageSize = pageSize, MinPageSize = 1, MaxPageSize = 100 }
                );
            }

            var invoices = await _invoiceRepository.GetInvoicesAsync(page, pageSize);
            return invoices;
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al obtener facturas", "GetInvoices", ex.Message, new { Page = page, PageSize = pageSize, Operation = "GetInvoices" });
        }
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ValidationException(
                    "El ID de la factura debe ser mayor a 0",
                    "Id",
                    "El ID debe ser un valor positivo.",
                    new { Id = id, MinId = 1 }
                );
            }

            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                throw new NotFoundException(
                    $"No se encontró la factura con ID {id}",
                    "Invoice",
                    id,
                    $"La factura con ID {id} no existe en el sistema.",
                    new { InvoiceId = id, Operation = "GetInvoiceById" }
                );
            }

            return invoice;
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al obtener factura por ID", "GetInvoiceById", ex.Message, new { InvoiceId = id, Operation = "GetInvoiceById" });
        }
    }

    public async Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                throw new ValidationException(
                    "El número de factura no puede estar vacío",
                    "InvoiceNumber",
                    "El número de factura es obligatorio.",
                    new { InvoiceNumber = invoiceNumber }
                );
            }

            var invoice = await _invoiceRepository.GetInvoiceByNumberAsync(invoiceNumber);
            return invoice; // Puede ser null si no existe
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al obtener factura por número", "GetInvoiceByNumber", ex.Message, new { InvoiceNumber = invoiceNumber, Operation = "GetInvoiceByNumber" });
        }
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto invoiceDto)
    {
        try
        {
            // Validar la factura antes de crearla
            await ValidateInvoiceForCreationAsync(invoiceDto);

            // Crear la factura
            var invoice = await _invoiceRepository.CreateInvoiceAsync(invoiceDto);
            return await GetInvoiceByIdAsync(invoice.Id) ?? 
                throw new DatabaseException("Error al crear la factura", "CreateInvoice", "La factura se creó pero no se pudo recuperar", new { InvoiceData = invoiceDto });
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al crear la factura", "CreateInvoice", ex.Message, new { InvoiceData = invoiceDto, Operation = "CreateInvoice" });
        }
    }

    public async Task<IEnumerable<InvoiceDto>> SearchInvoicesAsync(InvoiceSearchRequest searchRequest)
    {
        try
        {
            if (searchRequest == null)
            {
                throw new ValidationException(
                    "La solicitud de búsqueda no puede estar vacía",
                    "SearchRequest",
                    "Debe proporcionar criterios de búsqueda válidos.",
                    new { SearchRequest = searchRequest }
                );
            }

            if (string.IsNullOrWhiteSpace(searchRequest.SearchValue))
            {
                throw new ValidationException(
                    "El valor de búsqueda no puede estar vacío",
                    "SearchValue",
                    "Debe proporcionar un valor para buscar.",
                    new { SearchValue = searchRequest.SearchValue }
                );
            }

            var invoices = await _invoiceRepository.SearchInvoicesAsync(searchRequest);
            return invoices;
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al buscar facturas", "SearchInvoices", ex.Message, new { SearchRequest = searchRequest, Operation = "SearchInvoices" });
        }
    }

    public async Task<bool> CheckInvoiceNumberExistsAsync(string invoiceNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                throw new ValidationException(
                    "El número de factura no puede estar vacío",
                    "InvoiceNumber",
                    "El número de factura es obligatorio.",
                    new { InvoiceNumber = invoiceNumber }
                );
            }

            return await _invoiceRepository.CheckInvoiceNumberExistsAsync(invoiceNumber);
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al verificar existencia del número de factura", "CheckInvoiceNumberExists", ex.Message, new { InvoiceNumber = invoiceNumber, Operation = "CheckInvoiceNumberExists" });
        }
    }

    public async Task<InvoiceNumberValidationResult> CheckInvoiceNumberWithDetailsAsync(string invoiceNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                throw new ValidationException(
                    "El número de factura no puede estar vacío",
                    "InvoiceNumber",
                    "El número de factura es obligatorio.",
                    new { InvoiceNumber = invoiceNumber }
                );
            }

            return await _invoiceRepository.CheckInvoiceNumberWithDetailsAsync(invoiceNumber);
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al verificar existencia del número de factura", "CheckInvoiceNumberWithDetails", ex.Message, new { InvoiceNumber = invoiceNumber, Operation = "CheckInvoiceNumberWithDetails" });
        }
    }

    private async Task ValidateInvoiceForCreationAsync(InvoiceCreateDto invoiceDto)
    {
        var validationErrors = new List<ValidationError>();

        // Validar número de factura
        if (string.IsNullOrWhiteSpace(invoiceDto.InvoiceNumber))
        {
            validationErrors.Add(new ValidationError("InvoiceNumber", "El número de factura es obligatorio", "REQUIRED_FIELD"));
        }
        else if (invoiceDto.InvoiceNumber.Length < 3 || invoiceDto.InvoiceNumber.Length > 20)
        {
            validationErrors.Add(new ValidationError("InvoiceNumber", "El número de factura debe tener entre 3 y 20 caracteres", "INVALID_LENGTH", invoiceDto.InvoiceNumber, "Use un número de factura entre 3 y 20 caracteres"));
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(invoiceDto.InvoiceNumber, @"^[A-Za-z0-9\-_]+$"))
        {
            validationErrors.Add(new ValidationError("InvoiceNumber", "El número de factura solo puede contener letras, números, guiones y guiones bajos", "INVALID_FORMAT", invoiceDto.InvoiceNumber, "Use solo letras, números, guiones (-) y guiones bajos (_)"));
        }
        else if (await CheckInvoiceNumberExistsAsync(invoiceDto.InvoiceNumber))
        {
            throw new ConflictException(
                $"Ya existe una factura con el número '{invoiceDto.InvoiceNumber}'",
                "DUPLICATE_INVOICE_NUMBER",
                $"El número de factura '{invoiceDto.InvoiceNumber}' ya está en uso. Por favor, use un número diferente.",
                new { InvoiceNumber = invoiceDto.InvoiceNumber }
            );
        }

        // Validar cliente
        if (invoiceDto.ClientId <= 0)
        {
            validationErrors.Add(new ValidationError("ClientId", "El ID del cliente debe ser mayor a 0", "INVALID_VALUE", invoiceDto.ClientId, "Seleccione un cliente válido"));
        }
        else
        {
            var client = await _catalogRepository.GetClientByIdAsync(invoiceDto.ClientId);
            if (client == null)
            {
                validationErrors.Add(new ValidationError("ClientId", $"No existe un cliente con ID {invoiceDto.ClientId}", "CLIENT_NOT_FOUND", invoiceDto.ClientId, "Seleccione un cliente que exista en el sistema"));
            }
        }

        // Validar fecha de factura
        if (invoiceDto.InvoiceDate > DateTime.Today)
        {
            validationErrors.Add(new ValidationError("InvoiceDate", "La fecha de factura no puede ser futura", "INVALID_DATE", invoiceDto.InvoiceDate, "Use una fecha actual o pasada"));
        }
        else if (invoiceDto.InvoiceDate < DateTime.Today.AddYears(-10))
        {
            validationErrors.Add(new ValidationError("InvoiceDate", "La fecha de factura no puede ser muy antigua", "INVALID_DATE", invoiceDto.InvoiceDate, "Use una fecha no anterior a 10 años"));
        }

        // Validar detalles
        if (invoiceDto.Details == null || !invoiceDto.Details.Any())
        {
            validationErrors.Add(new ValidationError("Details", "La factura debe tener al menos un detalle", "REQUIRED_FIELD", invoiceDto.Details, "Agregue al menos un producto a la factura"));
        }
        else
        {
            foreach (var detail in invoiceDto.Details)
            {
                if (detail.ProductId <= 0)
                {
                    validationErrors.Add(new ValidationError($"Details[{detail.ProductId}].ProductId", "El ID del producto debe ser mayor a 0", "INVALID_VALUE", detail.ProductId, "Seleccione un producto válido"));
                }
                else
                {
                    var product = await _catalogRepository.GetProductByIdAsync(detail.ProductId);
                    if (product == null)
                    {
                        validationErrors.Add(new ValidationError($"Details[{detail.ProductId}].ProductId", $"No existe un producto con ID {detail.ProductId}", "PRODUCT_NOT_FOUND", detail.ProductId, "Seleccione un producto que exista en el sistema"));
                    }
                    else if (!product.IsActive)
                    {
                        validationErrors.Add(new ValidationError($"Details[{detail.ProductId}].ProductId", $"El producto '{product.Name}' no está activo", "INACTIVE_PRODUCT", detail.ProductId, "Solo puede usar productos activos"));
                    }
                    else if (detail.UnitPrice != product.Price)
                    {
                        validationErrors.Add(new ValidationError($"Details[{detail.ProductId}].UnitPrice", $"El precio unitario debe ser {product.Price:C}", "PRICE_MISMATCH", detail.UnitPrice, $"Use el precio actual del producto: {product.Price:C}"));
                    }
                }

                if (detail.Quantity <= 0)
                {
                    validationErrors.Add(new ValidationError($"Details[{detail.ProductId}].Quantity", "La cantidad debe ser mayor a 0", "INVALID_VALUE", detail.Quantity, "La cantidad debe ser un valor positivo"));
                }
                else if (detail.Quantity > 1000)
                {
                    validationErrors.Add(new ValidationError($"Details[{detail.ProductId}].Quantity", "La cantidad no puede exceder 1000", "INVALID_VALUE", detail.Quantity, "La cantidad máxima permitida es 1000"));
                }
            }
        }

        // Validar cálculos
        var calculatedSubtotal = invoiceDto.Details?.Sum(d => d.UnitPrice * d.Quantity) ?? 0;
        var calculatedTaxAmount = calculatedSubtotal * 0.19m;
        var calculatedTotal = calculatedSubtotal + calculatedTaxAmount;

        if (Math.Abs(invoiceDto.Subtotal - calculatedSubtotal) > 0.01m)
        {
            validationErrors.Add(new ValidationError("Subtotal", $"El subtotal debe ser {calculatedSubtotal:C}", "CALCULATION_ERROR", invoiceDto.Subtotal, $"El subtotal correcto es {calculatedSubtotal:C}"));
        }

        if (Math.Abs(invoiceDto.TaxAmount - calculatedTaxAmount) > 0.01m)
        {
            validationErrors.Add(new ValidationError("TaxAmount", $"El impuesto debe ser {calculatedTaxAmount:C}", "CALCULATION_ERROR", invoiceDto.TaxAmount, $"El impuesto correcto es {calculatedTaxAmount:C}"));
        }

        if (Math.Abs(invoiceDto.Total - calculatedTotal) > 0.01m)
        {
            validationErrors.Add(new ValidationError("Total", $"El total debe ser {calculatedTotal:C}", "CALCULATION_ERROR", invoiceDto.Total, $"El total correcto es {calculatedTotal:C}"));
        }

        // Si hay errores de validación, lanzar excepción
        if (validationErrors.Any())
        {
            throw new ValidationException(
                "La factura no cumple con las validaciones requeridas",
                validationErrors,
                new { InvoiceData = invoiceDto }
            );
        }
    }
}
