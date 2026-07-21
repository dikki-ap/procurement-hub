namespace ProcureHub.SharedKernel.Abstractions;

public interface IStorageService
{
    Task<string> UploadAsync(string bucket, string objectKey, Stream content, string contentType, CancellationToken ct = default);

    /// <param name="responseContentDisposition">
    /// Optional Content-Disposition baked into the signed URL (e.g. "inline" or "attachment; filename=\"file.pdf\"").
    /// Requires the storage backend to support S3 response-header overrides.
    /// </param>
    Task<string> GetPresignedUrlAsync(string bucket, string objectKey, TimeSpan expiry, string? responseContentDisposition = null, CancellationToken ct = default);

    Task<(Stream Content, string ContentType)> DownloadAsync(string bucket, string objectKey, CancellationToken ct = default);
    Task DeleteAsync(string bucket, string objectKey, CancellationToken ct = default);
    Task EnsureBucketExistsAsync(string bucket, CancellationToken ct = default);
}
