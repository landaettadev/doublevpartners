using Application.Dtos;
using Application.Interfaces;
using Common.Errors;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly ICatalogRepository _catalogRepository;

    public ProductService(ICatalogRepository catalogRepository)
    {
        _catalogRepository = catalogRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        try
        {
            var products = await _catalogRepository.GetProductsAsync();
            return await EnhanceProductsWithImagesAsync(products);
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al obtener productos", "GetProducts", ex.Message, new { Operation = "GetProducts" });
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _catalogRepository.GetProductByIdAsync(id);
            if (product == null) return null;

            var enhancedProducts = await EnhanceProductsWithImagesAsync(new[] { product });
            return enhancedProducts.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al obtener producto por ID", "GetProductById", ex.Message, new { ProductId = id, Operation = "GetProductById" });
        }
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateDto productDto)
    {
        try
        {
            // Validar que el nombre del producto sea único
            if (await ProductNameExistsAsync(productDto.Name))
            {
                throw new ConflictException(
                    $"Ya existe un producto con el nombre '{productDto.Name}'",
                    "DUPLICATE_PRODUCT_NAME",
                    $"El nombre '{productDto.Name}' ya está en uso. Por favor, elija un nombre diferente.",
                    new { ProductName = productDto.Name, SuggestedNames = GenerateSuggestedNames(productDto.Name) }
                );
            }

            // Validar precio
            if (productDto.Price <= 0)
            {
                throw new BusinessRuleException(
                    "El precio del producto debe ser mayor a 0",
                    "INVALID_PRODUCT_PRICE",
                    "El precio debe ser un valor positivo mayor a cero.",
                    new { Price = productDto.Price, MinPrice = 0.01m }
                );
            }

            // Crear el producto en la base de datos
            var product = await _catalogRepository.CreateProductAsync(productDto);
            return await GetProductByIdAsync(product.Id) ?? 
                throw new DatabaseException("Error al crear el producto", "CreateProduct", "El producto se creó pero no se pudo recuperar", new { ProductData = productDto });
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al crear el producto", "CreateProduct", ex.Message, new { ProductData = productDto, Operation = "CreateProduct" });
        }
    }

    public async Task<ProductDto> UpdateProductAsync(int id, ProductUpdateDto productDto)
    {
        try
        {
            var existingProduct = await GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                throw new NotFoundException(
                    $"No se encontró el producto con ID {id}",
                    "Product",
                    id,
                    $"El producto con ID {id} no existe en el sistema.",
                    new { ProductId = id, Operation = "UpdateProduct" }
                );
            }

            // Validar nombre único si se está cambiando
            if (!string.IsNullOrEmpty(productDto.Name) && 
                productDto.Name != existingProduct.Name &&
                await ProductNameExistsAsync(productDto.Name, id))
            {
                throw new ConflictException(
                    $"Ya existe un producto con el nombre '{productDto.Name}'",
                    "DUPLICATE_PRODUCT_NAME",
                    $"El nombre '{productDto.Name}' ya está en uso por otro producto. Por favor, elija un nombre diferente.",
                    new { ProductId = id, NewName = productDto.Name, ExistingName = existingProduct.Name }
                );
            }

            // Validar precio si se está cambiando
            if (productDto.Price.HasValue && productDto.Price.Value <= 0)
            {
                throw new BusinessRuleException(
                    "El precio del producto debe ser mayor a 0",
                    "INVALID_PRODUCT_PRICE",
                    "El precio debe ser un valor positivo mayor a cero.",
                    new { ProductId = id, Price = productDto.Price.Value, MinPrice = 0.01m }
                );
            }

            // Actualizar el producto en la base de datos
            var updatedProduct = await _catalogRepository.UpdateProductAsync(id, productDto);
            return await GetProductByIdAsync(id) ?? 
                throw new DatabaseException("Error al actualizar el producto", "UpdateProduct", "El producto se actualizó pero no se pudo recuperar", new { ProductId = id, UpdateData = productDto });
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al actualizar el producto", "UpdateProduct", ex.Message, new { ProductId = id, UpdateData = productDto, Operation = "UpdateProduct" });
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var product = await GetProductByIdAsync(id);
            if (product == null)
            {
                throw new NotFoundException(
                    $"No se encontró el producto con ID {id}",
                    "Product",
                    id,
                    $"El producto con ID {id} no existe en el sistema.",
                    new { ProductId = id, Operation = "DeleteProduct" }
                );
            }

            return await _catalogRepository.DeleteProductAsync(id);
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al eliminar el producto", "DeleteProduct", ex.Message, new { ProductId = id, Operation = "DeleteProduct" });
        }
    }

    public async Task<bool> ToggleProductStatusAsync(int id)
    {
        try
        {
            var product = await GetProductByIdAsync(id);
            if (product == null)
            {
                throw new NotFoundException(
                    $"No se encontró el producto con ID {id}",
                    "Product",
                    id,
                    $"El producto con ID {id} no existe en el sistema.",
                    new { ProductId = id, Operation = "ToggleProductStatus" }
                );
            }

            return await _catalogRepository.ToggleProductStatusAsync(id);
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al cambiar el estado del producto", "ToggleProductStatus", ex.Message, new { ProductId = id, Operation = "ToggleProductStatus" });
        }
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        try
        {
            var products = await _catalogRepository.GetActiveProductsAsync();
            return await EnhanceProductsWithImagesAsync(products);
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al obtener productos activos", "GetActiveProducts", ex.Message, new { Operation = "GetActiveProducts" });
        }
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ValidationException(
                    "El término de búsqueda no puede estar vacío",
                    "SearchTerm",
                    "El término de búsqueda es obligatorio y debe contener al menos un carácter.",
                    new { SearchTerm = searchTerm, MinLength = 1 }
                );
            }

            if (searchTerm.Length < 2)
            {
                throw new ValidationException(
                    "El término de búsqueda debe tener al menos 2 caracteres",
                    "SearchTerm",
                    "Para obtener resultados relevantes, el término de búsqueda debe tener al menos 2 caracteres.",
                    new { SearchTerm = searchTerm, MinLength = 2, CurrentLength = searchTerm.Length }
                );
            }

            var products = await _catalogRepository.SearchProductsAsync(searchTerm);
            return await EnhanceProductsWithImagesAsync(products);
        }
        catch (Common.Errors.ApplicationException)
        {
            throw; // Re-lanzar excepciones de aplicación
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al buscar productos", "SearchProducts", ex.Message, new { SearchTerm = searchTerm, Operation = "SearchProducts" });
        }
    }

    public async Task<bool> ProductExistsAsync(int id)
    {
        try
        {
            return await _catalogRepository.ProductExistsAsync(id);
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al verificar existencia del producto", "ProductExists", ex.Message, new { ProductId = id, Operation = "ProductExists" });
        }
    }

    public async Task<bool> ProductNameExistsAsync(string name, int? excludeId = null)
    {
        try
        {
            return await _catalogRepository.ProductNameExistsAsync(name, excludeId);
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al verificar existencia del nombre del producto", "ProductNameExists", ex.Message, new { ProductName = name, ExcludeId = excludeId, Operation = "ProductNameExists" });
        }
    }

    private async Task<IEnumerable<ProductDto>> EnhanceProductsWithImagesAsync(IEnumerable<ProductDto> products)
    {
        var enhancedProducts = new List<ProductDto>();

        foreach (var product in products)
        {
            var enhancedProduct = await EnhanceProductWithImageAsync(product);
            enhancedProducts.Add(enhancedProduct);
        }

        return enhancedProducts;
    }

    private async Task<ProductDto> EnhanceProductWithImageAsync(ProductDto product)
    {
        // Si el producto tiene una imagen, generar URLs completas
        if (!string.IsNullOrEmpty(product.Image.FileName))
        {
            // Por ahora, usamos URLs relativas que se resolverán en el frontend
            product.Image.Url = $"/images/products/{product.Image.FileName}";
            product.Image.ThumbnailUrl = $"/images/thumbnails/thumb_{product.Image.FileName}";
        }
        else
        {
            // Usar imágenes por defecto
            product.Image.Url = "/images/defaults/no-image.jpg";
            product.Image.ThumbnailUrl = "/images/defaults/no-image-thumb.jpg";
            product.Image.FileName = "no-image.jpg";
            product.Image.AltText = "Imagen no disponible";
        }

        return product;
    }

    private List<string> GenerateSuggestedNames(string baseName)
    {
        var suggestions = new List<string>();
        var random = new Random();
        
        // Agregar números
        for (int i = 1; i <= 3; i++)
        {
            suggestions.Add($"{baseName} {random.Next(100, 999)}");
        }
        
        // Agregar sufijos comunes
        var suffixes = new[] { "Pro", "Plus", "Premium", "Deluxe", "Standard" };
        foreach (var suffix in suffixes)
        {
            suggestions.Add($"{baseName} {suffix}");
        }
        
        return suggestions.Take(5).ToList();
    }
}
