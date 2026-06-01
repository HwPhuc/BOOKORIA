namespace BOOKORIA.Application.Abstractions;

public interface ICloudinaryStorageService
{
    Task<CloudinaryStoredFileResult> UploadBookCoverAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<CloudinaryStoredFileResult> UploadBookPdfAsync(
        Stream fileStream,
        string fileName,
        bool isSample,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string publicId,
        CloudinaryAssetType resourceType,
        CancellationToken cancellationToken = default);
}

public enum CloudinaryAssetType
{
    Image = 1,
    Raw = 2
}

public sealed record CloudinaryStoredFileResult(string Url, string PublicId, CloudinaryAssetType ResourceType);
