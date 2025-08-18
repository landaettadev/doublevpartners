using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Api.Models;

public class ProductCreateRequest
{
    [Required(ErrorMessage = "El nombre del producto es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio del producto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ProductCreateWithImageRequest
{
    [Required(ErrorMessage = "El nombre del producto es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio del producto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    [Display(Name = "Imagen del producto")]
    public IFormFile? Image { get; set; }
}

public class ProductUpdateRequest
{
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal? Price { get; set; }

    public bool? IsActive { get; set; }
}

public class ProductImageUpdateRequest
{
    [Display(Name = "Nueva imagen")]
    public IFormFile? NewImage { get; set; }

    [Display(Name = "Eliminar imagen actual")]
    public bool RemoveCurrentImage { get; set; } = false;

    [StringLength(255, ErrorMessage = "El texto alternativo no puede exceder 255 caracteres")]
    public string? AltText { get; set; }
}
