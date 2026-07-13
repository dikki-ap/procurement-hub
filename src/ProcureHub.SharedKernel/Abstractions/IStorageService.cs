namespace ProcureHub.SharedKernel.Abstractions;

public interface IStorageService
{
    Task<string>               UploadAsync(string bucket, string objectKey, Stream content, string contentType, CancellationToken ct = default);
    Task<string>               GetPresignedUrlAsync(string bucket, string objectKey, TimeSpan expiry, CancellationToken ct = default);
    Task<(Stream Content, string ContentType)> DownloadAsync(string bucket, string objectKey, CancellationToken ct = default);
    Task                       DeleteAsync(string bucket, string objectKey, CancellationToken ct = default);
    Task                       EnsureBucketExistsAsync(string bucket, CancellationToken ct = default);
}
