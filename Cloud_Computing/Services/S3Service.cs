using Amazon.S3;
using Amazon;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;

    public S3Service()
    {
        // No keys needed. AWS SDK automatically uses EC2 instance role
        _s3Client = new AmazonS3Client(RegionEndpoint.APSoutheast1);
    }

    public async Task<List<string>> ListBucketsAsync()
    {
        var response = await _s3Client.ListBucketsAsync();
        return response.Buckets.Select(b => b.BucketName).ToList();
    }

    public async Task UploadFileAsync(string bucketName, string key, Stream fileStream)
    {
        var putRequest = new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = fileStream
        };
        await _s3Client.PutObjectAsync(putRequest);
    }
}
