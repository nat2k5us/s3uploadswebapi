using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using fileuploads3.AwsUtils;
using fileuploads3.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace fileuploads3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private IHostingEnvironment hostingEnvironment;
        private readonly IOptions<AppSettings> appsettings;
        readonly ILogger<FileUploadController> log;
        private IS3Service s3Service;
        public FileUploadController(IHostingEnvironment _hostingEnvironment,
            IOptions<AppSettings> _appsettings,
            ILogger<FileUploadController> logger,
            IS3Service _s3Service)
        {
            this.hostingEnvironment = _hostingEnvironment;
            this.appsettings = _appsettings;
            this.log = logger;
            s3Service = _s3Service;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var message = "Credentials are read from the local system - not from appSettings.json";
            var test = new
            {
                AWS = "AWS",
                Profile = "Nat2k5us",
                Region = "us-west-2"
            };

            string json = JsonConvert.SerializeObject(test);
            Console.WriteLine(json); // single line JSON string
            string jsonFormatted = JValue.Parse(json).ToString(Formatting.Indented);
            return new string[] { message, json };

            // try
            // {
            //     var credentials = new BasicAWSCredentials(appsettings.Value.AWSId, appsettings.Value.AWSSecrect);
            //     new AmazonS3Client(credentials, RegionEndpoint.USWest1);
            //     message = "Login to AWS was Sucessfull";
            // }
            // catch (System.Exception ex)
            // {
            //     message = "Login to AWS Failed";
            // }

        }
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }
        /// <summary>
        /// Uploads a resource
        /// </summary>
        /// <param name="file">choose file</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post([FromForm]IFormFile file)
        {
            try
            {
                var path = $"images{Path.DirectorySeparatorChar}UploadFile";
                //string webRootPath = Path.Combine(this.hostingEnvironment.WebRootPath, path);
                string contentRootPath = Path.Combine(this.hostingEnvironment.ContentRootPath, path);
                if (!Directory.Exists(contentRootPath))
                {
                    Directory.CreateDirectory(contentRootPath);
                }
                List<string> extensions = this.appsettings.Value.FileExtensions;
                string extension = Path.GetExtension(file.FileName);
                string hash = string.Empty;
                if (!extensions.Contains(extension.ToLower()))
                    return BadRequest();
                else
                {
                    path = Path.Combine(contentRootPath, file.FileName);
                    //save files to content folder
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                       // await s3Service.CreateBucketAsync("test1232414");
                        await s3Service.UploadFileAsync("test1232414", path);
                    }
                    return this.Ok();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("File Name already exists"))
                    return BadRequest(ex.Message);
                log.LogError(ex.StackTrace);
                throw ex;
            }
        }

        [HttpPost("CreateBucket/{bucketName}")]
        public async Task<IActionResult> CreateBucket([FromRoute] string bucketName)
        {
            var response = await s3Service.CreateBucketAsync(bucketName);
            return Ok(response);
        }
        [HttpPost("DeleteBucket/{bucketName}")]
        public async Task<IActionResult> DeleteBucket([FromRoute] string bucketName)
        {
            var response = await s3Service.DeleteBucketAsync(bucketName);
            return Ok(response);
        }
        [HttpPost("AddFile/{bucketName}")]
        public async Task<IActionResult> AddFileToS3([FromRoute] string bucketName)
        {
            await s3Service.UploadFileAsync(bucketName, string.Empty);
            return Ok();
        }
        [HttpPost("GetFile/{bucketName}/{keyName}")]
        public async Task<IActionResult> GetObjectFromS3([FromRoute] string bucketName, string keyName = null)
        {
            await s3Service.GetObjectFromS3Async(bucketName, keyName);
            return Ok();
        }
        [HttpPost("DeleteFile/{bucketName}/{keyName}")]
        public async Task<IActionResult> DeleteObjectFromS3([FromRoute] string bucketName, string keyName = null)
        {
            await s3Service.DeleteObjectFromS3Async(bucketName, keyName);
            return Ok();
        }
    }
}
