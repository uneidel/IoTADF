using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.DataFactories.Runtime;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;


namespace Iot
{
   

    public class IOTHubExportActivity : Microsoft.DataFactories.Runtime.IDotNetActivity
    {
       
       
        private IActivityLogger _logger;
        private string iothubName;
        private string uri;
        private string expiry;
        private string key;
        private string policyName;
        private string dataStorageContainer, dataStorageAccountName, dataStorageAccountKey;


       
     
        private void GatherDataFromIotHub()
        {
            Uri storageAccountUri = new Uri("http://" + dataStorageAccountName + ".blob.core.windows.net/");
            string jsonFile = string.Format("iotdevices{0}.json",DateTime.Now.ToFileTime());
            var uri2 = uri.Substring(0, uri.IndexOf(".net/") + 4);
            uri2 = uri2.Substring(8);
            var SasToken = SASTokenGenerator.GetSASToken(uri2, expiry, key, policyName);
            _logger.Write(System.Diagnostics.TraceEventType.Information, SasToken);
            try
            {
                _logger.Write(System.Diagnostics.TraceEventType.Information, "Creating");
                File.WriteAllText(jsonFile, DownloadIdentitiesFromRest(uri,SasToken));
                _logger.Write(System.Diagnostics.TraceEventType.Information, "Uploading to Blob: ..");
                CloudBlobClient blobClient = new CloudBlobClient(storageAccountUri, new StorageCredentials(dataStorageAccountName, dataStorageAccountKey));

                string blobPath = string.Format(CultureInfo.InvariantCulture, "iothubdevices.json");

                CloudBlobContainer container = blobClient.GetContainerReference(dataStorageContainer);
                container.CreateIfNotExists();

                var blob = container.GetBlockBlobReference(blobPath);
                blob.UploadFromFile(jsonFile, FileMode.OpenOrCreate);
            }
            catch (Exception ex)
            {
                _logger.Write(System.Diagnostics.TraceEventType.Error, "Error occurred : {0}", ex);
                throw;
            }
            finally
            {
                if (File.Exists(jsonFile))
                {
                    File.Delete(jsonFile);
                }
            }
        }

       
        private string DownloadIdentitiesFromRest(string uri, string sasToken)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", sasToken);
            var jsonresult = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {   
                    jsonresult= reader.ReadToEnd();
                }
            }
            return jsonresult;
        }
        private async void BulkExport()
        {
            
            string uri = String.Format("{0}.azure-devices.net", iothubName);
            var connstring = SASTokenGenerator.ConnectionString(uri, key, policyName);
            var storageSAS = SASTokenGenerator.ContainerSaSUri(dataStorageAccountName, iothubName, dataStorageAccountKey);
            Devices d = new Devices();
            bool result = await d.ExportAllDevices(connstring, storageSAS);
            
            if (!result)
            {
                _logger.Write(System.Diagnostics.TraceEventType.Error, "Failure exporting.");
            }
            else
                _logger.Write(System.Diagnostics.TraceEventType.Information, "Successfully exported.");
        }
        public IDictionary<string, string> Execute(IEnumerable<ResolvedTable> inputTables, 
            IEnumerable<ResolvedTable> outputTables, IDictionary<string, string> extendedProperties, IActivityLogger logger)
        {
            // to get extended properties (for example: SliceStart)
            _logger = logger;
            uri = extendedProperties["uri"];
            iothubName = extendedProperties["iothubname"];
            expiry = EpochGenerator.GetEpochTime(1).ToString();
            key = extendedProperties["key"];
            policyName = extendedProperties["policyName"];
            dataStorageAccountName = extendedProperties["dataStorageAccountName"];
            dataStorageContainer = extendedProperties["dataStorageContainer"];
            dataStorageAccountKey = extendedProperties["dataStorageAccountKey"];
            _logger = logger;
            //GatherDataFromIotHub();
            BulkExport();
            
            return new Dictionary<string, string>();
        }
    }
}
