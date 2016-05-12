using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot
{
    internal class Devices
    {

        public async Task<bool> ExportAllDevices(string connString,string containerSasUri)
        {
            bool bret = false;
            // Call an export job on the IoT Hub to retrieve all devices
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connString);
            JobProperties exportJob = await registryManager.ExportDevicesAsync(containerSasUri, false);

            // Wait until job is finished
            while (true)
            {
                exportJob = await registryManager.GetJobAsync(exportJob.JobId);
                if (exportJob.Status == JobStatus.Completed ||
                    exportJob.Status == JobStatus.Failed ||
                    exportJob.Status == JobStatus.Cancelled)
                {
                    if (exportJob.Status == JobStatus.Completed)
                        bret = true;
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            return bret; 
        }
        public async Task<bool> ImportAllDevices(string connString, string containerSasUri)
        {
            bool bret = false;
            // Call an export job on the IoT Hub to retrieve all devices
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connString);
            JobProperties exportJob = await registryManager.ImportDevicesAsync(containerSasUri, containerSasUri);

            // Wait until job is finished
            while (true)
            {
                exportJob = await registryManager.GetJobAsync(exportJob.JobId);
                if (exportJob.Status == JobStatus.Completed ||
                    exportJob.Status == JobStatus.Failed ||
                    exportJob.Status == JobStatus.Cancelled)
                {
                    if (exportJob.Status == JobStatus.Completed)
                        bret = true;
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            return bret;
        }
    }
}
