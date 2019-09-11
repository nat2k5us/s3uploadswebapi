using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Util;
using System.Net;
namespace fileuploads3.AwsUtils
{
    public interface IS3Service
    {
        Task<S3Response> CreateBucketAsync(string bucketName);
        Task<S3Response> DeleteBucketAsync(string bucketName);
        Task<S3Response> UploadFileAsync(string bucketName, string filePath);
        Task<S3Response> GetObjectFromS3Async(string bucketName, string keyName);
        Task<S3Response> DeleteObjectFromS3Async(string bucketName, string keyName);
    }
}
