using Application.Dtos;
using Infrastructure.Db;
using Microsoft.Data.SqlClient;
using System.Data;
using Application.Interfaces;

namespace Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InvoiceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto invoiceDto)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Crear encabezado de factura
            var invoiceId = await CreateInvoiceHeaderAsync(connection, transaction, invoiceDto);
            
            // Crear detalles de factura
            await CreateInvoiceDetailsAsync(connection, transaction, invoiceId, invoiceDto.Details);

            transaction.Commit();

            // Retornar la factura creada
            return await GetInvoiceByIdAsync(invoiceId) ?? 
                throw new InvalidOperationException("No se pudo recuperar la factura creada");
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        // Obtener encabezado
        var headerCommand = new SqlCommand("sp_GetInvoiceById", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        headerCommand.Parameters.AddWithValue("@InvoiceId", id);

        InvoiceDto? invoice = null;
        using (var reader = await headerCommand.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                invoice = new InvoiceDto
                {
                    Id = reader.GetInt32(0),
                    InvoiceNumber = reader.GetString(1),
                    ClientId = reader.GetInt32(2),
                    ClientName = reader.GetString(3),
                    InvoiceDate = reader.GetDateTime(4),
                    Subtotal = reader.GetDecimal(5),
                    TaxAmount = reader.GetDecimal(6),
                    Total = reader.GetDecimal(7),
                    Status = reader.GetString(8),
                    CreatedAt = reader.GetDateTime(9),
                    UpdatedAt = reader.GetDateTime(10),
                    Details = new List<InvoiceDetailDto>()
                };
            }
        }

        if (invoice != null)
        {
            // Obtener detalles
            var detailsCommand = new SqlCommand("sp_GetInvoiceDetailsByInvoiceId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            detailsCommand.Parameters.AddWithValue("@InvoiceId", id);

            using var detailsReader = await detailsCommand.ExecuteReaderAsync();
            while (await detailsReader.ReadAsync())
            {
                invoice.Details.Add(new InvoiceDetailDto
                {
                    ProductId = detailsReader.GetInt32(0),
                    Quantity = detailsReader.GetInt32(1),
                    UnitPrice = detailsReader.GetDecimal(2),
                    Total = detailsReader.GetDecimal(3)
                });
            }
        }

        return invoice;
    }

    public async Task<IEnumerable<InvoiceListItemDto>> GetInvoicesAsync(int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_GetInvoices", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        
        command.Parameters.AddWithValue("@PageSize", pageSize);
        command.Parameters.AddWithValue("@PageNumber", page);

        var invoices = new List<InvoiceListItemDto>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            invoices.Add(new InvoiceListItemDto
            {
                Id = reader.GetInt32(0),
                InvoiceNumber = reader.GetString(1),
                ClientId = reader.GetInt32(2),
                ClientName = reader.GetString(3),
                InvoiceDate = reader.GetDateTime(4),
                Subtotal = reader.GetDecimal(5),
                TaxAmount = reader.GetDecimal(6),
                Total = reader.GetDecimal(7),
                Status = reader.GetString(8),
                CreatedAt = reader.GetDateTime(9)
            });
        }

        return invoices;
    }

    public async Task<IEnumerable<InvoiceListItemDto>> SearchInvoicesAsync(string searchType, string searchValue)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_SearchInvoices", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        
        command.Parameters.AddWithValue("@SearchType", searchType);
        command.Parameters.AddWithValue("@SearchValue", searchValue);

        var invoices = new List<InvoiceListItemDto>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            invoices.Add(new InvoiceListItemDto
            {
                Id = reader.GetInt32(0),
                InvoiceNumber = reader.GetString(1),
                ClientId = reader.GetInt32(2),
                ClientName = reader.GetString(3),
                InvoiceDate = reader.GetDateTime(4),
                Subtotal = reader.GetDecimal(5),
                TaxAmount = reader.GetDecimal(6),
                Total = reader.GetDecimal(7),
                Status = reader.GetString(8),
                CreatedAt = reader.GetDateTime(9)
            });
        }

        return invoices;
    }

    public async Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        // Obtener encabezado por número de factura
        var headerCommand = new SqlCommand("sp_GetInvoiceByNumber", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        headerCommand.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);

        InvoiceDto? invoice = null;
        using (var reader = await headerCommand.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                invoice = new InvoiceDto
                {
                    Id = reader.GetInt32(0),
                    InvoiceNumber = reader.GetString(1),
                    ClientId = reader.GetInt32(2),
                    ClientName = reader.GetString(3),
                    InvoiceDate = reader.GetDateTime(4),
                    Subtotal = reader.GetDecimal(5),
                    TaxAmount = reader.GetDecimal(6),
                    Total = reader.GetDecimal(7),
                    Status = reader.GetString(8),
                    CreatedAt = reader.GetDateTime(9),
                    UpdatedAt = reader.GetDateTime(10),
                    Details = new List<InvoiceDetailDto>()
                };
            }
        }

        if (invoice != null)
        {
            // Obtener detalles
            var detailsCommand = new SqlCommand("sp_GetInvoiceDetailsByInvoiceId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            detailsCommand.Parameters.AddWithValue("@InvoiceId", invoice.Id);

            using var detailsReader = await detailsCommand.ExecuteReaderAsync();
            while (await detailsReader.ReadAsync())
            {
                invoice.Details.Add(new InvoiceDetailDto
                {
                    ProductId = detailsReader.GetInt32(0),
                    Quantity = detailsReader.GetInt32(1),
                    UnitPrice = detailsReader.GetDecimal(2),
                    Total = detailsReader.GetDecimal(3)
                });
            }
        }

        return invoice;
    }

    public async Task<bool> CheckInvoiceNumberExistsAsync(string invoiceNumber)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_CheckInvoiceNumberExists", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        
        command.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
        var existsParam = command.Parameters.Add("@Exists", SqlDbType.Bit);
        existsParam.Direction = ParameterDirection.Output;

        await command.ExecuteNonQueryAsync();
        return (bool)existsParam.Value;
    }

    public async Task<int> GetTotalInvoicesCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("SELECT COUNT(*) FROM Invoices", connection);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private async Task<int> CreateInvoiceHeaderAsync(SqlConnection connection, SqlTransaction transaction, InvoiceCreateDto invoiceDto)
    {
        var command = new SqlCommand("sp_CreateInvoice", connection, transaction)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@InvoiceNumber", invoiceDto.InvoiceNumber);
        command.Parameters.AddWithValue("@ClientId", invoiceDto.ClientId);
        command.Parameters.AddWithValue("@InvoiceDate", invoiceDto.InvoiceDate);
        command.Parameters.AddWithValue("@Subtotal", invoiceDto.Subtotal);
        command.Parameters.AddWithValue("@TaxAmount", invoiceDto.TaxAmount);
        command.Parameters.AddWithValue("@Total", invoiceDto.Total);

        // Crear tabla temporal para los detalles
        var detailsTable = new DataTable();
        detailsTable.Columns.Add("ProductId", typeof(int));
        detailsTable.Columns.Add("Quantity", typeof(int));
        detailsTable.Columns.Add("UnitPrice", typeof(decimal));
        detailsTable.Columns.Add("Total", typeof(decimal));

        foreach (var detail in invoiceDto.Details)
        {
            detailsTable.Rows.Add(detail.ProductId, detail.Quantity, detail.UnitPrice, detail.Total);
        }

        var detailsParam = command.Parameters.Add("@InvoiceDetails", SqlDbType.Structured);
        detailsParam.Value = detailsTable;
        detailsParam.TypeName = "InvoiceDetailTVP";

        var invoiceIdParam = command.Parameters.Add("@InvoiceId", SqlDbType.Int);
        invoiceIdParam.Direction = ParameterDirection.Output;

        await command.ExecuteNonQueryAsync();
        return (int)invoiceIdParam.Value;
    }

    private async Task CreateInvoiceDetailsAsync(SqlConnection connection, SqlTransaction transaction, int invoiceId, List<InvoiceDetailDto> details)
    {
        // Los detalles se insertan automáticamente en el stored procedure
        // Esta función se mantiene por consistencia pero no es necesaria
        await Task.CompletedTask;
    }
}
