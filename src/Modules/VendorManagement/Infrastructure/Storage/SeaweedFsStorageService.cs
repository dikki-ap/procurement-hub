using System.Net.Http.Headers;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProcureHub.Modules.VendorManagement.Infrastructure.Storage;

public class SeaweedFsStorageService : IStorageService
{
    // Shared HttpClient for presigned-PUT uploads — avoids socket exhaustion.
    private static readonly HttpClient _http = new();

    private readonly IAmazonS3                         _s3;
    private readonly ILogger<SeaweedFsStorageService>  _logger;
    private readonly string                            _internalBase;
    private readonly string?                           _publicEndpoint;
    private readonly Protocol                          _protocol;

    public SeaweedFsStorageService(IConfiguration config, ILogger<SeaweedFsStorageService> logger)
    {
        _logger = logger;

        var endpoint  = config["SeaweedFS:S3Endpoint"] ?? throw new InvalidOperationException("SeaweedFS:S3Endpoint is not configured.");
        var accessKey = config["SeaweedFS:AccessKey"]  ?? throw new InvalidOperationException("SeaweedFS:AccessKey is not configured.");
        var secretKey = config["SeaweedFS:SecretKey"]  ?? throw new InvalidOperationException("SeaweedFS:SecretKey is not configured.");

        _publicEndpoint = config["SeaweedFS:PublicEndpoint"];
        var uri = new Uri(endpoint);
        _internalBase = $"{uri.Scheme}://{uri.Host}" + (uri.IsDefaultPort ? "" : $":{uri.Port}");
        _protocol     = endpoint.StartsWith("https", StringComparison.OrdinalIgnoreCase) ? Protocol.HTTPS : Protocol.HTTP;

        _s3 = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
        {
            ServiceURL                 = endpoint,
            ForcePathStyle             = true,
            UseHttp                    = !endpoint.StartsWith("https", StringComparison.OrdinalIgnoreCase),
            DisableHostPrefixInjection = true,
        });
    }

    public async Task<string> UploadAsync(string bucket, string objectKey, Stream content, string contentType, CancellationToken ct = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        _logger.LogInformation("SeaweedFS Upload: key={Key} size={Size} first8={First8}",
            objectKey, buffer.Length,
            Convert.ToHexString(buffer.ToArray().Take(8).ToArray()));

        // Use a presigned PUT URL and upload via raw HttpClient instead of PutObjectAsync.
        // AWSSDK.S3 3.7.x wraps the payload in aws-chunked signing format, which SeaweedFS
        // stores verbatim instead of decoding — resulting in corrupted files on download.
        // A raw HttpClient PUT bypasses the SDK's chunked signing entirely.
        var presignedPut = _s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName  = bucket,
            Key         = objectKey,
            Expires     = DateTime.UtcNow.AddMinutes(5),
            Verb        = HttpVerb.PUT,
            ContentType = contentType,
            Protocol    = _protocol,
        });

        using var body = new StreamContent(buffer);
        body.Headers.ContentType   = new MediaTypeHeaderValue(contentType);
        body.Headers.ContentLength = buffer.Length;

        var response = await _http.PutAsync(presignedPut, body, ct);
        response.EnsureSuccessStatusCode();

        return objectKey;
    }

    public async Task<(Stream Content, string ContentType)> DownloadAsync(string bucket, string objectKey, CancellationToken ct = default)
    {
        try
        {
            var request = new GetObjectRequest { BucketName = bucket, Key = objectKey };
            using var response = await _s3.GetObjectAsync(request, ct);
            var contentType = response.Headers.ContentType ?? "application/octet-stream";
            var buf = new MemoryStream();
            await response.ResponseStream.CopyToAsync(buf, ct);
            buf.Position = 0;

            _logger.LogInformation("SeaweedFS Download: key={Key} size={Size} contentType={CT} first8={First8}",
                objectKey, buf.Length, contentType,
                buf.Length >= 8 ? Convert.ToHexString(buf.ToArray().Take(8).ToArray()) : "(too short)");

            return (buf, contentType);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"Object '{objectKey}' was not found in bucket '{bucket}'.", objectKey);
        }
    }

    public Task<string> GetPresignedUrlAsync(string bucket, string objectKey, TimeSpan expiry, string? responseContentDisposition = null, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key        = objectKey,
            Expires    = DateTime.UtcNow.Add(expiry),
            Protocol   = _protocol,
        };

        if (!string.IsNullOrEmpty(responseContentDisposition))
            request.ResponseHeaderOverrides = new ResponseHeaderOverrides
            {
                ContentDisposition = responseContentDisposition,
            };

        var url = _s3.GetPreSignedURL(request);

        if (!string.IsNullOrEmpty(_publicEndpoint))
            url = url.Replace(_internalBase, _publicEndpoint.TrimEnd('/'));

        return Task.FromResult(url);
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
            try
            {
                await _s3.PutBucketAsync(bucket, ct);
            }
            catch (Amazon.S3.Model.BucketAlreadyOwnedByYouException) { }
            catch (Amazon.S3.Model.BucketAlreadyExistsException) { }
        }
    }
}
