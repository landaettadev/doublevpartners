using Application.Dtos;
using Application.Interfaces;
using Application.Services;
using Common.Errors;
using FluentAssertions;
using Moq;

namespace Backend.Tests;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _mockInvoiceRepository;
    private readonly Mock<ICatalogRepository> _mockCatalogRepository;
    private readonly InvoiceService _invoiceService;

    public InvoiceServiceTests()
    {
        _mockInvoiceRepository = new Mock<IInvoiceRepository>();
        _mockCatalogRepository = new Mock<ICatalogRepository>();
        _invoiceService = new InvoiceService(_mockInvoiceRepository.Object, _mockCatalogRepository.Object);
    }

    [Fact]
    public async Task CreateInvoiceAsync_WithValidData_ShouldCreateInvoice()
    {
        // Arrange
        var invoiceDto = new InvoiceCreateDto
        {
            InvoiceNumber = "FAC-001-2024",
            ClientId = 1,
            InvoiceDate = DateTime.Now,
            Details = new List<InvoiceDetailDto>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 10000, Total = 20000 }
            }
        };

        var client = new ClientDto { Id = 1, Name = "Test Client" };
        var product = new ProductDto { Id = 1, Name = "Test Product", Price = 10000 };

        _mockInvoiceRepository.Setup(x => x.CheckInvoiceNumberExistsAsync(invoiceDto.InvoiceNumber))
            .ReturnsAsync(false);
        _mockCatalogRepository.Setup(x => x.GetClientByIdAsync(invoiceDto.ClientId))
            .ReturnsAsync(client);
        _mockCatalogRepository.Setup(x => x.GetProductByIdAsync(1))
            .ReturnsAsync(product);

        var createdInvoice = new InvoiceDto { Id = 1, InvoiceNumber = "FAC-001-2024" };
        _mockInvoiceRepository.Setup(x => x.CreateInvoiceAsync(invoiceDto))
            .ReturnsAsync(createdInvoice);

        // Act
        var result = await _invoiceService.CreateInvoiceAsync(invoiceDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.InvoiceNumber.Should().Be("FAC-001-2024");
    }

    [Fact]
    public async Task CreateInvoiceAsync_WithDuplicateInvoiceNumber_ShouldThrowException()
    {
        // Arrange
        var invoiceDto = new InvoiceCreateDto
        {
            InvoiceNumber = "FAC-001-2024",
            ClientId = 1,
            InvoiceDate = DateTime.Now,
            Details = new List<InvoiceDetailDto>()
        };

        _mockInvoiceRepository.Setup(x => x.CheckInvoiceNumberExistsAsync(invoiceDto.InvoiceNumber))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => 
            _invoiceService.CreateInvoiceAsync(invoiceDto));
    }

    [Fact]
    public async Task CreateInvoiceAsync_WithInvalidClient_ShouldThrowException()
    {
        // Arrange
        var invoiceDto = new InvoiceCreateDto
        {
            InvoiceNumber = "FAC-001-2024",
            ClientId = 999,
            InvoiceDate = DateTime.Now,
            Details = new List<InvoiceDetailDto>()
        };

        _mockInvoiceRepository.Setup(x => x.CheckInvoiceNumberExistsAsync(invoiceDto.InvoiceNumber))
            .ReturnsAsync(false);
        _mockCatalogRepository.Setup(x => x.GetClientByIdAsync(invoiceDto.ClientId))
            .ReturnsAsync((ClientDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => 
            _invoiceService.CreateInvoiceAsync(invoiceDto));
    }

    [Fact]
    public async Task SearchInvoicesAsync_WithValidSearchType_ShouldReturnResults()
    {
        // Arrange
        var searchType = "Client";
        var searchValue = "Test Client";
        var expectedInvoices = new List<InvoiceListItemDto>
        {
            new() { Id = 1, InvoiceNumber = "FAC-001-2024", ClientName = "Test Client" }
        };

        _mockInvoiceRepository.Setup(x => x.SearchInvoicesAsync(searchType, searchValue))
            .ReturnsAsync(expectedInvoices);

        // Act
        var result = await _invoiceService.SearchInvoicesAsync(searchType, searchValue);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().ClientName.Should().Be("Test Client");
    }

    [Fact]
    public async Task SearchInvoicesAsync_WithInvalidSearchType_ShouldThrowException()
    {
        // Arrange
        var searchType = "Invalid";
        var searchValue = "Test";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _invoiceService.SearchInvoicesAsync(searchType, searchValue));
    }
}
