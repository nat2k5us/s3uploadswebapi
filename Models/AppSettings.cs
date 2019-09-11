using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
namespace fileuploads3.Models
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public List<string> FileExtensions { get; set; }
        public string FileServer { get; set; }
        public string HttpFileServerContentFolder { get; set; }
        public string BackGrounndSystemResourceFolder { get; set; }
        public string FileServerUrl { get; set; }
        public List<string> ImageExtensions { get; set; }
        public string AWSId { get; set; }
        public string AWSSecrect { get; set; }

    }
}
