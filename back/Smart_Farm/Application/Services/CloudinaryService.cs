using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var acc = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(acc);
    }

    // Generic upload — keeps existing callers working
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "smart_farm"
        };
        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl.ToString();
    }

    // Targeted upload — used by DiagnoseAsync to place the image alongside its record
    public async Task<string> UploadImageAsync(Stream stream, string fileName, string folder, string publicId)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            PublicId = publicId,
            Overwrite = true
        };
        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    public async Task TryDeleteByUrlAsync(string? url)
    {
        var publicId = ExtractPublicId(url);
        if (string.IsNullOrWhiteSpace(publicId))
            return;

        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }

    private static string? ExtractPublicId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        try
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');
            var uploadIdx = Array.IndexOf(segments, "upload");
            if (uploadIdx < 0)
                return null;

            var start = uploadIdx + 1;
            if (start < segments.Length && segments[start].StartsWith('v') &&
                long.TryParse(segments[start][1..], out _))
                start++;

            var publicIdWithExt = string.Join("/", segments[start..]);
            var dot = publicIdWithExt.LastIndexOf('.');
            return dot >= 0 ? publicIdWithExt[..dot] : publicIdWithExt;
        }
        catch
        {
            return null;
        }
    }
}
