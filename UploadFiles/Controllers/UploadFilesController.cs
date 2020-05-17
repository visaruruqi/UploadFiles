using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace UploadFiles.Controllers
{
    public class UploadFilesController : ApiController
    {
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
        private const string bucketName = "bucketName";
        private const string aws_access_key_id = "aws_access_key_id";
        private const string aws_secret_key = "aws_secret_key";

        private static IAmazonS3 client;

        public UploadFilesController()
        {
            client = new AmazonS3Client(aws_access_key_id, aws_secret_key, bucketRegion);
        }

        [HttpPost]
        public async Task<IHttpActionResult> Upload()
        {

            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var req = HttpContext.Current.Request;
            var dec = req.Form["decReqType"];
            var fiscal_no = req.Form["fiscal_no"];
            //var files = req.Files;

            try
            {
                var fileTransferUtility =
                    new TransferUtility(client);


                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                foreach (var file in provider.Contents.Where(x=>x.Headers.ContentType!=null))
                {
                    var details = file.Headers.ContentDisposition;
                    var extension = details.FileName.Trim('"').Split('.');
                    if (extension.Length==0) { extension = new string[] { "" }; }

                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = bucketName,
                        InputStream = await file.ReadAsStreamAsync(),
                        ContentType = file.Headers.ContentType.MediaType,
                        StorageClass = S3StorageClass.Standard,
                        PartSize = file.Headers.ContentLength.Value,
                        CannedACL = S3CannedACL.PublicRead,
                        Key = Guid.NewGuid().ToString() + "." + extension[extension.Length-1],
                    };

                    fileTransferUtilityRequest.Metadata.Add("Name", details.Name.Trim('"'));
                    fileTransferUtilityRequest.Metadata.Add("Filename", details.FileName.Trim('"'));
                    fileTransferUtilityRequest.Metadata.Add("ContentType", file.Headers.ContentType.MediaType.Trim('"'));
                    fileTransferUtilityRequest.Metadata.Add("Size", file.Headers.ContentLength.Value.ToString());

                    await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
                    //CannedACL = S3CannedACL.PublicRead
                };

                return Ok();
            }
            catch (AmazonS3Exception e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }


        }



        [HttpPut]
        public async Task<IHttpActionResult> SingleFileUpload()
        {

            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            try
            {
                var fileTransferUtility =
                    new TransferUtility(client);

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                HttpContent file = provider.Contents[0];

                var details = file.Headers.ContentDisposition;
                var extension = details.FileName.Trim('"').Split('.');
                if (extension.Length == 0) { extension = new string[] { "" }; }

                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    InputStream = await file.ReadAsStreamAsync(),
                    ContentType = file.Headers.ContentType.MediaType,
                    StorageClass = S3StorageClass.Standard,
                    PartSize = file.Headers.ContentLength.Value,
                    Key = Guid.NewGuid().ToString() + "." + extension[extension.Length - 1],
                };

                fileTransferUtilityRequest.Metadata.Add("Name", details.Name.Trim('"'));
                fileTransferUtilityRequest.Metadata.Add("Filename", details.FileName.Trim('"'));
                fileTransferUtilityRequest.Metadata.Add("ContentType", file.Headers.ContentType.MediaType.Trim('"'));
                fileTransferUtilityRequest.Metadata.Add("Size", file.Headers.ContentLength.Value.ToString());

                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);

                return Ok(new { key = fileTransferUtilityRequest.Key, filePath = fileTransferUtilityRequest.FilePath });
            }
            catch (AmazonS3Exception e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }


        }

        private string getNiceFileSize(long inSize=0)
        {
            var size = inSize;

            if (size < 1024)
                return size + " Bytes";

            size /= 1024;

            if (size < 1024)
                return size + " Kb";

            size /= 1024;

            if (size < 1024)
                return size + " Mb";

            size /= 1024;

            if (size < 1024)
                return size + " Gb";

            size /= 1024;

            return size + " Tb";
        }

    }
}
