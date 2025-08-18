using Microsoft.AspNetCore.Http;

namespace Api.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile imageFile, string productName);
    Task<string> SaveThumbnailAsync(IFormFile imageFile, string productName);
    Task<bool> DeleteImageAsync(string imagePath);
    Task<bool> DeleteThumbnailAsync(string thumbnailPath);
    Task<ProductImageInfo> GetImageInfoAsync(IFormFile imageFile);
    Task<bool> IsValidImageAsync(IFormFile imageFile);
    Task<string> GenerateImageUrlAsync(string fileName);
    Task<string> GenerateThumbnailUrlAsync(string fileName);
    Task<string> GetDefaultImageUrlAsync();
    Task<string> GetDefaultThumbnailUrlAsync();
}

public class ProductImageInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
