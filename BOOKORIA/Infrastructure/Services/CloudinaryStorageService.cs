using BOOKORIA.Application.Abstractions;
using BOOKORIA.Infrastructure.Options;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace BOOKORIA.Infrastructure.Services;

public class CloudinaryStorageService : ICloudinaryStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _options;

    public CloudinaryStorageService(IOptions<CloudinaryOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.CloudName) ||
            string.IsNullOrWhiteSpace(_options.ApiKey) ||
            string.IsNullOrWhiteSpace(_options.ApiSecret))
        {
            throw new InvalidOperationException("Cloudinary is not configured. Please set Cloudinary:CloudName, ApiKey, ApiSecret.");
        }

        var account = new Account(_options.CloudName, _options.ApiKey, _options.ApiSecret);
        _cloudinary = new Cloudinary(account)
        {
            Api = { Secure = true }
        };
    }

    public async Task<CloudinaryStoredFileResult> UploadBookCoverAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = _options.BookCoverFolder,
            Type = "upload",
            AccessMode = "public",
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        EnsureUploadSucceeded(result.Error, result.PublicId);

        return new CloudinaryStoredFileResult(result.SecureUrl.AbsoluteUri, result.PublicId, CloudinaryAssetType.Image);
    }

    public async Task<CloudinaryStoredFileResult> UploadBookPdfAsync(
        Stream fileStream,
        string fileName,
        bool isSample,
        CancellationToken cancellationToken = default)
    {
        var folder = isSample ? _options.BookSamplePdfFolder : _options.BookPdfFolder;

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            Type = "upload",
            AccessMode = "public",
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        EnsureUploadSucceeded(result.Error, result.PublicId);

        return new CloudinaryStoredFileResult(result.SecureUrl.AbsoluteUri, result.PublicId, CloudinaryAssetType.Raw);
    }

    public async Task DeleteAsync(
        string publicId,
        CloudinaryAssetType resourceType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = MapResourceType(resourceType)
        };

        await _cloudinary.DestroyAsync(deleteParams);
    }

    private static ResourceType MapResourceType(CloudinaryAssetType resourceType)
    {
        return resourceType == CloudinaryAssetType.Image
            ? ResourceType.Image
            : ResourceType.Raw;
    }

    private static void EnsureUploadSucceeded(Error? error, string? publicId)
    {
        if (error is not null)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {error.Message}");
        }

        if (string.IsNullOrWhiteSpace(publicId))
        {
            throw new InvalidOperationException("Cloudinary upload failed: empty public id.");
        }
    }
}
