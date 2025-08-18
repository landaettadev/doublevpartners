using Application.Dtos;
using Application.Interfaces;
using Common.Errors;
using Infrastructure.Db;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InvoiceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<InvoiceDto>> GetInvoicesAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_GetInvoices", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@PageNumber", page);
            command.Parameters.AddWithValue("@PageSize", pageSize);

            var invoices = new List<InvoiceDto>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                invoices.Add(new InvoiceDto
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
        catch (SqlException ex)
        {
            throw new DatabaseException(
                "Error al obtener facturas desde la base de datos",
                "GetInvoices",
                ex.Message,
                new { Page = page, PageSize = pageSize, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new DatabaseException(
                "Error inesperado al obtener facturas",
                "GetInvoices",
                ex.Message,
                new { Page = page, PageSize = pageSize, Error = ex.Message }
            );
        }
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_GetInvoiceById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@InvoiceId", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new InvoiceDto
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
                    UpdatedAt = reader.GetDateTime(10)
                };
            }

            return null;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException(
                "Error al obtener factura por ID desde la base de datos",
                "GetInvoiceById",
                ex.Message,
                new { InvoiceId = id, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new DatabaseException(
                "Error inesperado al obtener factura por ID",
                "GetInvoiceById",
                ex.Message,
                new { InvoiceId = id, Error = ex.Message }
            );
        }
    }

    public async Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_GetInvoiceByNumber", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new InvoiceDto
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
                    UpdatedAt = reader.GetDateTime(10)
                };
            }

            return null;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException(
                "Error al obtener factura por número desde la base de datos",
                "GetInvoiceByNumber",
                ex.Message,
                new { InvoiceNumber = invoiceNumber, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new DatabaseException(
                "Error inesperado al obtener factura por número",
                "GetInvoiceByNumber",
                ex.Message,
                new { InvoiceNumber = invoiceNumber, Error = ex.Message }
            );
        }
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto invoiceDto)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Crear la factura
                var invoiceCommand = new SqlCommand("sp_CreateInvoice", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };

                invoiceCommand.Parameters.AddWithValue("@InvoiceNumber", invoiceDto.InvoiceNumber);
                invoiceCommand.Parameters.AddWithValue("@ClientId", invoiceDto.ClientId);
                invoiceCommand.Parameters.AddWithValue("@InvoiceDate", invoiceDto.InvoiceDate);
                invoiceCommand.Parameters.AddWithValue("@Subtotal", invoiceDto.Subtotal);
                invoiceCommand.Parameters.AddWithValue("@TaxAmount", invoiceDto.TaxAmount);
                invoiceCommand.Parameters.AddWithValue("@Total", invoiceDto.Total);

                // Crear TVP para detalles
                var detailsTable = new DataTable();
                detailsTable.Columns.Add("ProductId", typeof(int));
                detailsTable.Columns.Add("Quantity", typeof(int));
                detailsTable.Columns.Add("UnitPrice", typeof(decimal));
                detailsTable.Columns.Add("Total", typeof(decimal));

                if (invoiceDto.Details != null)
                {
                    foreach (var d in invoiceDto.Details)
                    {
                        detailsTable.Rows.Add(d.ProductId, d.Quantity, d.UnitPrice, d.Total);
                    }
                }

                var detailsParam = invoiceCommand.Parameters.AddWithValue("@InvoiceDetails", detailsTable);
                detailsParam.SqlDbType = SqlDbType.Structured;
                detailsParam.TypeName = "dbo.InvoiceDetailTVP";

                var invoiceIdParam = invoiceCommand.Parameters.Add("@InvoiceId", SqlDbType.Int);
                invoiceIdParam.Direction = ParameterDirection.Output;

                await invoiceCommand.ExecuteNonQueryAsync();
                var invoiceId = (int)invoiceIdParam.Value;

                transaction.Commit();

                // Retornar la factura creada
                return await GetInvoiceByIdAsync(invoiceId) ?? 
                    throw new DatabaseException(
                        "Error al crear la factura",
                        "CreateInvoice",
                        "La factura se creó pero no se pudo recuperar",
                        new { InvoiceId = invoiceId, InvoiceData = invoiceDto }
                    );
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (SqlException ex)
        {
            throw new DatabaseException(
                "Error al crear factura en la base de datos",
                "CreateInvoice",
                ex.Message,
                new { InvoiceData = invoiceDto, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new DatabaseException(
                "Error inesperado al crear factura",
                "CreateInvoice",
                ex.Message,
                new { InvoiceData = invoiceDto, Error = ex.Message }
            );
        }
    }

    public async Task<IEnumerable<InvoiceDto>> SearchInvoicesAsync(InvoiceSearchRequest searchRequest)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_SearchInvoices", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@SearchType", searchRequest.SearchType);
            command.Parameters.AddWithValue("@SearchValue", searchRequest.SearchValue);

            var invoices = new List<InvoiceDto>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                invoices.Add(new InvoiceDto
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
        catch (SqlException ex)
        {
            throw new DatabaseException(
                "Error al buscar facturas en la base de datos",
                "SearchInvoices",
                ex.Message,
                new { SearchRequest = searchRequest, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new DatabaseException(
                "Error inesperado al buscar facturas",
                "SearchInvoices",
                ex.Message,
                new { SearchRequest = searchRequest, Error = ex.Message }
            );
        }
    }

    public async Task<bool> CheckInvoiceNumberExistsAsync(string invoiceNumber)
    {
        try
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
            return existsParam.Value != DBNull.Value && (bool)existsParam.Value;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException(
                "Error al verificar existencia del número de factura en la base de datos",
                "CheckInvoiceNumberExists",
                ex.Message,
                new { InvoiceNumber = invoiceNumber, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new DatabaseException(
                "Error inesperado al verificar existencia del número de factura",
                "CheckInvoiceNumberExists",
                ex.Message,
                new { InvoiceNumber = invoiceNumber, Error = ex.Message }
            );
        }
    }

    public async Task<InvoiceNumberValidationResult> CheckInvoiceNumberWithDetailsAsync(string invoiceNumber)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new SqlCommand("sp_CheckInvoiceNumberExists", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);

            var invoiceIdParam = command.Parameters.Add("@InvoiceId", SqlDbType.Int);
            invoiceIdParam.Direction = ParameterDirection.Output;

            var clientNameParam = command.Parameters.Add("@ClientName", SqlDbType.NVarChar, 100);
            clientNameParam.Direction = ParameterDirection.Output;

            var invoiceDateParam = command.Parameters.Add("@InvoiceDate", SqlDbType.DateTime);
            invoiceDateParam.Direction = ParameterDirection.Output;

            await command.ExecuteNonQueryAsync();

            var result = new InvoiceNumberValidationResult
            {
                Exists = invoiceIdParam.Value != DBNull.Value,
                InvoiceId = invoiceIdParam.Value != DBNull.Value ? (int)invoiceIdParam.Value : 0,
                ClientName = clientNameParam.Value?.ToString() ?? string.Empty,
                InvoiceDate = invoiceDateParam.Value != DBNull.Value ? (DateTime)invoiceDateParam.Value : DateTime.MinValue
            };

            return result;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException(
                "Error al verificar existencia del número de factura con detalles en la base de datos",
                "CheckInvoiceNumberWithDetails",
                ex.Message,
                new { InvoiceNumber = invoiceNumber, SqlError = ex.Number }
            );
        }
        catch (Exception ex)
        {
            throw new DatabaseException(
                "Error inesperado al verificar existencia del número de factura con detalles",
                "CheckInvoiceNumberWithDetails",
                ex.Message,
                new { InvoiceNumber = invoiceNumber, Error = ex.Message }
            );
        }
    }

    public async Task<int> GetTotalInvoicesCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("SELECT COUNT(*) FROM Invoices", connection);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
