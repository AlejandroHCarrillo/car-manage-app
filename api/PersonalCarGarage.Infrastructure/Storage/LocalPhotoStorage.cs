using Microsoft.Extensions.Options;
using PersonalCarGarage.Application.Abstractions;
using PersonalCarGarage.Application.Options;

namespace PersonalCarGarage.Infrastructure.Storage;

public class LocalPhotoStorage(IOptions<PhotoStorageOptions> options) : IPhotoStorage
{
    private readonly string rootPath = options.Value.RootPath;

    public Task<string> SaveAsync(Guid vehicleId, Guid photoId, string extension, Stream content, CancellationToken cancellationToken = default) =>
        SaveInFolderAsync(vehicleId, photoId, extension, string.Empty, content, cancellationToken);

    public async Task<string> SaveInFolderAsync(Guid vehicleId, Guid fileId, string extension, string folder, Stream content, CancellationToken cancellationToken = default)
    {
        var directory = string.IsNullOrWhiteSpace(folder)
            ? Path.Combine(rootPath, "uploads", "vehicles", vehicleId.ToString())
            : Path.Combine(rootPath, "uploads", "vehicles", vehicleId.ToString(), folder);
        Directory.CreateDirectory(directory);

        var fileName = $"{fileId}{extension}";
        var fullPath = Path.Combine(directory, fileName);

        await using var file = File.Create(fullPath);
        await content.CopyToAsync(file, cancellationToken);

        return string.IsNullOrWhiteSpace(folder)
            ? Path.Combine("uploads", "vehicles", vehicleId.ToString(), fileName).Replace('\\', '/')
            : Path.Combine("uploads", "vehicles", vehicleId.ToString(), folder, fileName).Replace('\\', '/');
    }

    public void Delete(string storagePath)
    {
        var fullPath = Path.Combine(rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public string GetPublicUrl(string storagePath) => "/" + storagePath.TrimStart('/');
}
