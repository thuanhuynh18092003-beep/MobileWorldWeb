namespace ShopMobileWorld.Services;

public class ProductImageStorage(IWebHostEnvironment environment)
{
    private const long MaxLength = 5 * 1024 * 1024;
    private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = ".jpg", [".jpeg"] = ".jpg", [".png"] = ".png", [".webp"] = ".webp"
    };

    private string ImageDirectory => Path.Combine(environment.WebRootPath, "uploads", "products");

    public async Task<string?> ValidateAsync(IFormFile? file)
    {
        if (file is null) return null;
        if (file.Length == 0 || file.Length > MaxLength) return "Ảnh phải có dung lượng từ 1 byte đến 5 MB.";

        var extension = Path.GetExtension(file.FileName);
        if (!Extensions.ContainsKey(extension)) return "Chỉ hỗ trợ ảnh JPG, PNG hoặc WebP.";

        if (file.Length < 12) return "Tệp ảnh không hợp lệ.";
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        await stream.ReadExactlyAsync(header);
        var valid = extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff,
            ".png" => header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".webp" => header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
        return valid ? null : "Nội dung tệp không đúng định dạng ảnh đã chọn.";
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        Directory.CreateDirectory(ImageDirectory);
        var name = $"{Guid.NewGuid():N}{Extensions[Path.GetExtension(file.FileName)]}";
        var path = Path.Combine(ImageDirectory, name);
        try
        {
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(output);
        }
        catch
        {
            File.Delete(path);
            throw;
        }
        return $"/uploads/products/{name}";
    }

    public void Delete(string? imageUrl)
    {
        const string prefix = "/uploads/products/";
        if (imageUrl is null || !imageUrl.StartsWith(prefix, StringComparison.Ordinal)) return;
        var name = imageUrl[prefix.Length..];
        if (name.Length == 0 || name != Path.GetFileName(name)) return;
        File.Delete(Path.Combine(ImageDirectory, name));
    }
}
