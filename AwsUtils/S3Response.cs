using System.Net;

namespace fileuploads3.AwsUtils
{
    public class S3Response
    {
        public HttpStatusCode Status { get; set; }
        public string Message { get; set; }
    }
}