namespace PersonalCarGarage.Application.Abstractions;

public interface IPhotoStorage
{
    Task<string> SaveAsync(Guid vehicleId, Guid photoId, string extension, Stream content, CancellationToken cancellationToken = default);
    Task<string> SaveInFolderAsync(Guid vehicleId, Guid fileId, string extension, string folder, Stream content, CancellationToken cancellationToken = default);
    void Delete(string storagePath);
    string GetPublicUrl(string storagePath);
}
