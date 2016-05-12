using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Iot
{
    public class SASTokenGenerator
    {
        public static string GetSASToken(string uri, string expiry, string key, string policyName)
        {
            string uriStringEncoded = HttpUtility.UrlEncode(uri);
            string stringToSign = uriStringEncoded + "\n" + expiry;
            var signature = Sign(stringToSign, key);
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sig={0}&se={1}&skn={2}&sr={3}", HttpUtility.UrlEncode(signature), expiry, policyName, uri);
            return sasToken;
        }
        private static string Sign(string requestString, string key)
        {
            string result;
            using (HMACSHA256 hMACSHA = new HMACSHA256(Convert.FromBase64String(key)))
            {
                result = Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(requestString)));
            }
            return result;
        }
        public static string ConnectionString(string uri, string key, string policyName)
        {
            uri = RemoveHttps(uri);
            var connString = String.Format("HostName={0};SharedAccessKeyName={1};SharedAccessKey={2}",
                    uri, policyName, key);
            return connString;
           
        }
        public static string ContainerSaSUri(string storageName,string folderName, string storageKey)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(GetStorageConnectionString(storageName,storageKey));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(folderName);
            container.CreateIfNotExists();
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Delete;

            //Generate the shared access signature on the container, setting the constraints directly on the signature.
            string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;


        }
        public static string GetStorageConnectionString(string storageName, string storageKey)
        {
            return String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageName, storageKey);

        }
        private static string RemoveHttps(string uri)
        {
            if (uri.ToLower().Contains("https:"))
                uri = uri.Substring(8);
            return uri;
            //REMOVE everything behind .net
           // var uri2 = uri.Substring(0, uri.IndexOf(".net/") + 4);
            
        }
    }

    public class EpochGenerator
    {
        public static int GetEpochTime(int AddDays)
        {
            TimeSpan t = (DateTime.UtcNow).AddDays(AddDays) - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            return secondsSinceEpoch;
            
        }
    }
}
