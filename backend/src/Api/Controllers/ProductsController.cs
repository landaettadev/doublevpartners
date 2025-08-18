using Api.Interfaces;
using Api.Services;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers;

/// <summary>
/// Controlador para gestión de productos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación para todos los endpoints
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IImageService _imageService;

    public ProductsController(IProductService productService, IImageService imageService)
    {
        _productService = productService;
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        try
        {
            var products = await _productService.GetProductsAsync();
            return Ok(new
            {
                message = "Productos obtenidos exitosamente",
                count = products.Count(),
                products = products,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al obtener productos",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetActiveProducts()
    {
        try
        {
            var products = await _productService.GetActiveProductsAsync();
            return Ok(new
            {
                message = "Productos activos obtenidos exitosamente",
                count = products.Count(),
                products = products,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al obtener productos activos",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    error = "ID inválido",
                    message = "El ID del producto debe ser mayor a 0",
                    timestamp = DateTime.UtcNow
                });
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new
                {
                    error = "Producto no encontrado",
                    message = $"No se encontró el producto con ID {id}",
                    timestamp = DateTime.UtcNow
                });

            return Ok(product);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al obtener el producto",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductCreateDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(new
                {
                    error = "Error de validación del modelo",
                    details = errors,
                    timestamp = DateTime.UtcNow
                });
            }

            var product = await _productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new
            {
                message = "Producto creado exitosamente",
                product = product,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al crear el producto",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpPost("with-image")]
    public async Task<ActionResult<ProductDto>> CreateProductWithImage([FromForm] ProductCreateDto request, IFormFile? image)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(new
                {
                    error = "Error de validación del modelo",
                    details = errors,
                    timestamp = DateTime.UtcNow
                });
            }

            // Validar imagen
            if (image != null)
            {
                if (!await _imageService.IsValidImageAsync(image))
                {
                    return BadRequest(new
                    {
                        error = "Imagen inválida",
                        message = "El archivo no es una imagen válida o excede el tamaño máximo",
                        timestamp = DateTime.UtcNow
                    });
                }
            }

            if (image != null)
            {
                var imageInfo = await _imageService.GetImageInfoAsync(image);
                request.Image = new ProductImageCreateDto
                {
                    FileName = image.FileName,
                    ContentType = image.ContentType,
                    FileSize = image.Length
                };
            }

            var product = await _productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new
            {
                message = "Producto creado exitosamente",
                product = product,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al crear el producto",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] ProductUpdateDto request)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    error = "ID inválido",
                    message = "El ID del producto debe ser mayor a 0",
                    timestamp = DateTime.UtcNow
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(new
                {
                    error = "Error de validación del modelo",
                    details = errors,
                    timestamp = DateTime.UtcNow
                });
            }

            var product = await _productService.UpdateProductAsync(id, request);
            return Ok(new
            {
                message = "Producto actualizado exitosamente",
                product = product,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al actualizar el producto",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    error = "ID inválido",
                    message = "El ID del producto debe ser mayor a 0",
                    timestamp = DateTime.UtcNow
                });
            }

            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted)
            {
                return NotFound(new
                {
                    error = "Producto no encontrado",
                    message = $"No se encontró el producto con ID {id}",
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                message = "Producto eliminado exitosamente",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al eliminar el producto",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult> ToggleProductStatus(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    error = "ID inválido",
                    message = "El ID del producto debe ser mayor a 0",
                    timestamp = DateTime.UtcNow
                });
            }

            var toggled = await _productService.ToggleProductStatusAsync(id);
            if (!toggled)
            {
                return NotFound(new
                {
                    error = "Producto no encontrado",
                    message = $"No se encontró el producto con ID {id}",
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                message = "Estado del producto cambiado exitosamente",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al cambiar el estado del producto",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new
                {
                    error = "Término de búsqueda requerido",
                    message = "El parámetro 'q' es requerido para la búsqueda",
                    timestamp = DateTime.UtcNow
                });
            }

            var products = await _productService.SearchProductsAsync(q);
            return Ok(new
            {
                message = "Búsqueda realizada exitosamente",
                searchTerm = q,
                count = products.Count(),
                products = products,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al buscar productos",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpPost("validate-image")]
    public async Task<ActionResult<object>> ValidateImage(IFormFile image)
    {
        try
        {
            if (image == null)
            {
                return BadRequest(new
                {
                    error = "Imagen requerida",
                    message = "Debe proporcionar un archivo de imagen",
                    timestamp = DateTime.UtcNow
                });
            }

            var isValid = await _imageService.IsValidImageAsync(image);
            var imageInfo = await _imageService.GetImageInfoAsync(image);

            return Ok(new
            {
                fileName = image.FileName,
                isValid = isValid,
                imageInfo = imageInfo,
                message = isValid ? "La imagen es válida" : "La imagen no es válida",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = "Error al validar la imagen",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
