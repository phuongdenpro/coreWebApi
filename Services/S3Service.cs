using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

public class S3Service
{
    private readonly IConfiguration _config;
    private readonly IAmazonS3 _s3Client;

    public S3Service(IConfiguration config)
    {
        _config = config;

        var accessKey = _config["AWS:AccessKey"];
        var secretKey = _config["AWS:SecretKey"];
        var region = _config["AWS:Region"];

        _s3Client = new AmazonS3Client(
            accessKey,
            secretKey,
            RegionEndpoint.GetBySystemName(region)
        );
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName = "products")
    {
        if (file == null || file.Length == 0)
            throw new Exception("File không hợp lệ");

        var bucketName = _config["AWS:BucketName"];

        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"{folderName}/{Guid.NewGuid()}{fileExtension}";

        using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = fileName,
            InputStream = stream,
            ContentType = file.ContentType
        };

        await _s3Client.PutObjectAsync(request);

        return $"https://{bucketName}.s3.{_config["AWS:Region"]}.amazonaws.com/{fileName}";
    }
}