using Amazon.Lambda.Core;
using Amazon.S3.Model;
using Amazon.S3;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ReceiptApp.Images.Functions;
public class ThumbnailGenerator
{
    private readonly IAmazonS3 _S3Client;

    public ThumbnailGenerator(IAmazonS3 s3Client)
    {
        _S3Client = s3Client;
    }

    public async Task Generate(GetObjectResponse s3Object, string destinationBucketName)
    {
        using (var s3ObjectStream = s3Object.ResponseStream)
        {
            var thumbnail = BuildThumbnail(s3ObjectStream);
            await StoreThumbnail(destinationBucketName, s3Object.Key, thumbnail);
        }
    }

    private static Stream BuildThumbnail(Stream s3ObjectStream)
    {
        var imageStream = new MemoryStream();

        using var image = Image.Load(s3ObjectStream);
        // Create B&W thumbnail
        image.Mutate(ctx => ctx.Grayscale().Resize(200, 200));
        image.Save(imageStream, new JpegEncoder());
        imageStream.Seek(0, SeekOrigin.Begin);
        return imageStream;
    }

    public static string GetThumbnailPath(string objectPath)
    {
        string sub;
        if (objectPath.EndsWith("/"))
        {
            sub = objectPath.Substring(0, objectPath.Length - 1);
        }
        else
        {
            sub = objectPath;
        }
        var lastSlashIndex = sub.LastIndexOf("/");
        var prefix = sub.Substring(0, lastSlashIndex);
        var fileName = sub.Substring(lastSlashIndex + 1);
        var date = DateTime.UtcNow;
        // Creating a new S3 ObjectKey for the thumbnails
        return $"{prefix}/{date.Year}/{date.Month}/{fileName}";
    }

    private async Task StoreThumbnail(string bucketName, string objectPath, Stream imageStream)
    {
        var thumbnailObjectKey = GetThumbnailPath(objectPath);

        LambdaLogger.Log("----> Thumbnail file Key: " + thumbnailObjectKey);

        await _S3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucketName,
            Key = thumbnailObjectKey,
            InputStream = imageStream
        });
    }
}