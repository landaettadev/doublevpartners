using Api.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using Common.Errors;

namespace Api.Services;

public class ImageService : IImageService
{
    private readonly string _imagesPath;
    private readonly string _thumbnailsPath;
    private readonly string _baseUrl;
    private readonly int _maxFileSizeMB = 10;
    private readonly int _maxWidth = 1920;
    private readonly int _maxHeight = 1080;
    private readonly int _thumbnailWidth = 300;
    private readonly int _thumbnailHeight = 300;

    public ImageService(IConfiguration configuration)
    {
        _imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
        _thumbnailsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "thumbnails");
        _baseUrl = configuration["BaseUrl"] ?? "http://localhost:5000";
        
        // Crear directorios si no existen
        Directory.CreateDirectory(_imagesPath);
        Directory.CreateDirectory(_thumbnailsPath);
    }

    public async Task<string> SaveImageAsync(IFormFile imageFile, string productName)
    {
        if (imageFile == null || imageFile.Length == 0)
            throw new Common.Errors.ValidationException(
                "El archivo de imagen es requerido",
                "ImageFile",
                "Debe seleccionar un archivo de imagen para continuar.",
                new { ProductName = productName, FileSize = 0 }
            );

        if (!await IsValidImageAsync(imageFile))
            throw new Common.Errors.ImageProcessingException(
                "El archivo no es una imagen válida",
                imageFile.ContentType,
                imageFile.Length,
                "Por favor, seleccione un archivo de imagen válido (JPG, PNG, WebP) con un tamaño máximo de 10MB.",
                new { FileName = imageFile.FileName, ContentType = imageFile.ContentType, FileSize = imageFile.Length }
            );

        try
        {
            var fileName = GenerateFileName(productName, Path.GetExtension(imageFile.FileName));
            var filePath = Path.Combine(_imagesPath, fileName);

            using var image = await Image.LoadAsync(imageFile.OpenReadStream());
            
            // Redimensionar si es muy grande
            if (image.Width > _maxWidth || image.Height > _maxHeight)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(_maxWidth, _maxHeight),
                    Mode = ResizeMode.Max
                }));
            }

            // Guardar con calidad optimizada
            await image.SaveAsync(filePath, new JpegEncoder
            {
                Quality = 85
            });

            return fileName;
        }
        catch (Exception ex)
        {
            throw new Common.Errors.FileOperationException(
                $"Error al guardar la imagen: {ex.Message}",
                Path.Combine(_imagesPath, productName),
                "SaveImage",
                "No se pudo procesar y guardar la imagen. Por favor, intente con otro archivo.",
                new { ProductName = productName, FileName = imageFile.FileName, Error = ex.Message }
            );
        }
    }

    public async Task<string> SaveThumbnailAsync(IFormFile imageFile, string productName)
    {
        if (imageFile == null || imageFile.Length == 0)
            throw new Common.Errors.ValidationException(
                "El archivo de imagen es requerido",
                "ImageFile",
                "Debe seleccionar un archivo de imagen para continuar.",
                new { ProductName = productName, FileSize = 0 }
            );

        try
        {
            var fileName = GenerateFileName(productName, Path.GetExtension(imageFile.FileName));
            var thumbnailFileName = $"thumb_{fileName}";
            var filePath = Path.Combine(_thumbnailsPath, thumbnailFileName);

            using var image = await Image.LoadAsync(imageFile.OpenReadStream());
            
            // Crear thumbnail centrado
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(_thumbnailWidth, _thumbnailHeight),
                Mode = ResizeMode.Crop,
                Position = AnchorPositionMode.Center
            }));

            await image.SaveAsync(filePath, new JpegEncoder
            {
                Quality = 80
            });

            return thumbnailFileName;
        }
        catch (Exception ex)
        {
            throw new Common.Errors.FileOperationException(
                $"Error al crear el thumbnail: {ex.Message}",
                Path.Combine(_thumbnailsPath, productName),
                "SaveThumbnail",
                "No se pudo crear el thumbnail de la imagen. Por favor, intente con otro archivo.",
                new { ProductName = productName, FileName = imageFile.FileName, Error = ex.Message }
            );
        }
    }

    public async Task<bool> DeleteImageAsync(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return false;

        try
        {
            var fullPath = Path.Combine(_imagesPath, imagePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            throw new Common.Errors.FileOperationException(
                $"Error al eliminar la imagen: {ex.Message}",
                imagePath,
                "DeleteImage",
                "No se pudo eliminar la imagen del servidor.",
                new { ImagePath = imagePath, Error = ex.Message }
            );
        }
    }

    public async Task<bool> DeleteThumbnailAsync(string thumbnailPath)
    {
        if (string.IsNullOrEmpty(thumbnailPath))
            return false;

        try
        {
            var fullPath = Path.Combine(_thumbnailsPath, thumbnailPath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            throw new Common.Errors.FileOperationException(
                $"Error al eliminar el thumbnail: {ex.Message}",
                thumbnailPath,
                "DeleteThumbnail",
                "No se pudo eliminar el thumbnail del servidor.",
                new { ThumbnailPath = thumbnailPath, Error = ex.Message }
            );
        }
    }

    public async Task<ProductImageInfo> GetImageInfoAsync(IFormFile imageFile)
    {
        var info = new ProductImageInfo
        {
            FileSize = imageFile.Length,
            ContentType = imageFile.ContentType,
            FileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant(),
            IsValid = false
        };

        try
        {
            if (imageFile.Length > _maxFileSizeMB * 1024 * 1024)
            {
                info.ErrorMessage = $"El archivo excede el tamaño máximo de {_maxFileSizeMB}MB.";
                return info;
            }

            using var image = await Image.LoadAsync(imageFile.OpenReadStream());
            info.Width = image.Width;
            info.Height = image.Height;
            info.IsValid = true;
        }
        catch (Exception ex)
        {
            info.ErrorMessage = $"Error al procesar la imagen: {ex.Message}";
        }

        return info;
    }

    public async Task<bool> IsValidImageAsync(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return false;

        // Validar tamaño
        if (imageFile.Length > _maxFileSizeMB * 1024 * 1024)
            return false;

        // Validar tipo de contenido
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(imageFile.ContentType.ToLowerInvariant()))
            return false;

        // Validar extensión
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return false;

        try
        {
            using var image = await Image.LoadAsync(imageFile.OpenReadStream());
            return image.Width > 0 && image.Height > 0;
        }
        catch
        {
            return false;
        }
    }

    public Task<string> GenerateImageUrlAsync(string fileName)
    {
        return Task.FromResult($"{_baseUrl}/images/products/{fileName}");
    }

    public Task<string> GenerateThumbnailUrlAsync(string fileName)
    {
        return Task.FromResult($"{_baseUrl}/images/thumbnails/{fileName}");
    }

    public Task<string> GetDefaultImageUrlAsync()
    {
        return Task.FromResult($"{_baseUrl}/images/defaults/no-image.jpg");
    }

    public Task<string> GetDefaultThumbnailUrlAsync()
    {
        return Task.FromResult($"{_baseUrl}/images/defaults/no-image-thumb.jpg");
    }

    private string GenerateFileName(string productName, string extension)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var sanitizedName = SanitizeFileName(productName);
        return $"{sanitizedName}_{timestamp}{extension}";
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = invalidChars.Aggregate(fileName, (current, invalidChar) => current.Replace(invalidChar, '_'));
        return sanitized.Replace(" ", "_").ToLowerInvariant();
    }
}
