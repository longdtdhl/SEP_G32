using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCBS.Application.Interfaces.Services;

namespace OPCBS.Infrastructure.Services;

/// <summary>
/// Cloudinary settings bound from configuration
/// </summary>
public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}

/// <summary>
/// Real file storage service using Cloudinary for production uploads.
/// Supports images and raw files (PDF, etc.)
/// </summary>
public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryFileStorageService> _logger;

    public CloudinaryFileStorageService(
        IOptions<CloudinarySettings> options,
        ILogger<CloudinaryFileStorageService> logger)
    {
        _logger = logger;
        var settings = options.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var result = await UploadFileAsync(fileStream, fileName, folder, cancellationToken);
        return result.Url;
    }

    public async Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var isPdf = extension == ".pdf";
        var uniqueId = $"{Guid.NewGuid():N}";
        var targetFolder = $"opcbs/{folder.Trim('/')}";

        if (isPdf)
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = targetFolder,
                PublicId = uniqueId,
                Overwrite = true
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary raw upload failed for {FileName}: {Error}", fileName, result.Error.Message);
                throw new InvalidOperationException($"File upload failed: {result.Error.Message}");
            }

            _logger.LogInformation("Cloudinary uploaded raw {FileName} to {Folder} → {Url}", fileName, targetFolder, result.SecureUrl);
            return new FileUploadResult
            {
                Url = result.SecureUrl.ToString(),
                PublicId = result.PublicId ?? $"{targetFolder}/{uniqueId}"
            };
        }
        else
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = targetFolder,
                PublicId = uniqueId,
                Overwrite = true,
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary image upload failed for {FileName}: {Error}", fileName, result.Error.Message);
                throw new InvalidOperationException($"File upload failed: {result.Error.Message}");
            }

            _logger.LogInformation("Cloudinary uploaded image {FileName} to {Folder} → {Url}", fileName, targetFolder, result.SecureUrl);
            return new FileUploadResult
            {
                Url = result.SecureUrl.ToString(),
                PublicId = result.PublicId ?? $"{targetFolder}/{uniqueId}"
            };
        }
    }

    public async Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null)
        {
            _logger.LogWarning("Cloudinary delete failed for {PublicId}: {Error}", publicId, result.Error.Message);
            return false;
        }

        _logger.LogInformation("Cloudinary deleted {PublicId}, result: {Result}", publicId, result.Result);
        return result.Result == "ok";
    }
}
