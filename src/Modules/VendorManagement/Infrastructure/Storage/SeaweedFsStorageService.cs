using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Storage;

public class SeaweedFsStorageService : IStorageService
{
    private readonly IAmazonS3 _s3;

    public SeaweedFsStorageService(IConfiguration config)
    {
        var endpoint  = config["SeaweedFS:S3Endpoint"]  ?? throw new InvalidOperationException("SeaweedFS:S3Endpoint is not configured.");
        var accessKey = config["SeaweedFS:AccessKey"]   ?? throw new InvalidOperationException("SeaweedFS:AccessKey is not configured.");
        var secretKey = config["SeaweedFS:SecretKey"]   ?? throw new InvalidOperationException("SeaweedFS:SecretKey is not configured.");

        _s3 = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
        {
            ServiceURL            = endpoint,
            ForcePathStyle        = true,
            UseHttp               = !endpoint.StartsWith("https", StringComparison.OrdinalIgnoreCase),
            DisableHostPrefixInjection = true,
        });
    }

    public async Task<string> UploadAsync(string bucket, string objectKey, Stream content, string contentType, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName  = bucket,
            Key         = objectKey,
            InputStream = content,
            ContentType = contentType,
        };

        await _s3.PutObjectAsync(request, ct);

        return objectKey;
    }

    public async Task<(Stream Content, string ContentType)> DownloadAsync(string bucket, string objectKey, CancellationToken ct = default)
    {
        var request  = new GetObjectRequest { BucketName = bucket, Key = objectKey };
        var response = await _s3.GetObjectAsync(request, ct);
        return (response.ResponseStream, response.Headers.ContentType ?? "application/octet-stream");
    }

    public Task<string> GetPresignedUrlAsync(string bucket, string objectKey, TimeSpan expiry, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key        = objectKey,
            Expires    = DateTime.UtcNow.Add(expiry),
        };

        return Task.FromResult(_s3.GetPreSignedURL(request));
    }

    public async Task DeleteAsync(string bucket, string objectKey, CancellationToken ct = default)
    {
        await _s3.DeleteObjectAsync(bucket, objectKey, ct);
    }

    public async Task EnsureBucketExistsAsync(string bucket, CancellationToken ct = default)
    {
        try
        {
            await _s3.GetBucketLocationAsync(bucket, ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await _s3.PutBucketAsync(bucket, ct);
        }
    }
}
