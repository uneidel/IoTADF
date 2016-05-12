# Syncing IOT Devices with Azure Data Factory


This Sample uses Data Factory Activities for Exporting / Importing Identities to IoT Hub.

Data Flow:

 - DF Custom Activity -> Export All Device Identities via IOTHub API
 - DeviceIdentities are stored as json File in Azure Blob Storage (Note: devices.txt ist fixed - Mai/2016)
 - SQL Import to SQL Server
	 - Note: Currently DocumentDB does not support Upsert Commands
 - SQL Export to Blob Storage
 - DF Custom Activity -> Imports to IOTHub


Merging of Device identities is done in SQL Server. 
In order to merge Identities from multiple IoTHubs please setup a pipeline for each IOTHub with the same SQLDatabase.


![](https://raw.githubusercontent.com/uneidel/IoTADF/master/Architecture.JPG)

Setup the Sample:

1) Setup IoT Hub (Please see my other repository for the corresponding ARM Template)
2) Setup separate ResourceGroup
	```powershell
		$rgName = "IoTADF";
		$ADFName ="iotDF";
		$location ="WEST US"
		Login-AzureRMAccount
		New-AzureRMResourceGroup -Name $rgName -Location $location
	```
3) Setup Azure Data Factory
    ```powershell
       New-AzureRMDataFactory -ResourceGroupName $rgName -Name $ADFName -Location $location
    ```

4) Since we have custom Activities in Place we need to setup Azure Batch Service (currently Pool Creation is not supported by ARM Template)
	```powershell
    $batchName = "IotBatchService";
    $batchPoolName = "IOTPool";
    New-AzureRmBatchAccount -AccountName $batchName -ResourceGroupName $rgName -Location $location
    $keys = Get-AzureRmBatchAccountKeys -AccountName $batchName
    New-AzureBatchPool -Name $batchPoolName -BatchContext $keys -VirtualMachineSize "small" -OSFamily "4" -TargetOSVersion "*" -TargetDedicated 1
    ```powershell

5) setup Storage Account and Store keys for Pipeline
    ```powershell
       $storageName = "SomeStorage" 
       New-AzureRMStorageAccount -Name $storageName -ResourceGroupName  $rgName -Type Standard_LRS -Location $location
       $keys = Get-AzureRmStorageAccountKey -ResourceGroupName $rgName -Name  $storageName
    ```

6) setup SQL 
     Please see [https://azure.microsoft.com/en-us/documentation/templates/?term=SQL](https://azure.microsoft.com/en-us/documentation/templates/?term=SQL "SQL ARM Templates")
	 for DDL please creatdb.sql

7)  Setup Solution
	Open Solution and Compile
    Change to bin/debug || bin/Release and zip content (Exclude Folders zh_*)
    Use Tools like Azure Storage Explorer to upload zip File to Azure Blob (See 5)

8)  Setup Pipeline 
     AzureBatchedLinkedService -> Please see 4
     AzureSQLLinkedService.json -> Please see 6
     AzureStorageLinkedService.json -> Please see 5
     CustomExportpipeline.json/CustomImportpipeline.json -> adapt Activities/ExtendedProperties and Activities/packageName Section - see 7
     *pipeline.json -> Adjust Start / End 
     

8) Deploy Pipeline
   	```powershell
    $df = Get-AzureRMDataFactory -Name uneideladf1 -ResourceGroupName azureiotdatafactory
  	New-AzureRMDataFactoryLinkedService $df -file ./AzureStorageLinkedService.json -Force
	New-AzureRMDataFactoryLinkedService $df -file ./AzureBatchedLinkedService.json -Force
	New-AzureRMDataFactoryLinkedService $df -file ./AzureSQLLinkedservice.json -Force
	New-AzureRMDataFactoryDataSet $df -file ./IdentityBlobTableDataSet.json -Force
	New-AzureRMDataFactoryDataSet $df -file ./inputdataset.json -Force
	New-AzureRMDataFactoryDataSet $df -file ./outputdataset.json -Force
	New-AzureRMDataFactoryDataSet $df -file ./AzureSQLoutDataSet.json -Force
	New-AzureRMDataFactoryDataSet $df -file ./IdentityBlobOutTableDataSet.json -Force
	New-AzureRMDataFactoryPipeline $df -file ./CustomExportpipeline.json -Force
	New-AzureRMDataFactoryPipeline $df -file ./SQLPipeline.json -Force
	New-AzureRMDataFactoryPipeline $df -file ./SQLOutPipeline.json -Force
	New-AzureRMDataFactoryPipeline $df -file ./CustomImportpipeline.json -Force
    ```

9) Ready to Go.


Useful Tools for Monitoring and Debugging:
[https://azure.microsoft.com/en-us/blog/microsoft-azure-storage-explorer-preview-january-update-and-roadmap/](https://azure.microsoft.com/en-us/blog/microsoft-azure-storage-explorer-preview-january-update-and-roadmap/ "Cloud Storage Explorer")
[https://github.com/Azure/azure-batch-samples/tree/master/CSharp/BatchExplorer](https://github.com/Azure/azure-batch-samples/tree/master/CSharp/BatchExplorer "Azure Batch Explorer")
[https://github.com/Azure/azure-iot-sdks/blob/master/tools/DeviceExplorer/doc/how_to_use_device_explorer.md](https://github.com/Azure/azure-iot-sdks/blob/master/tools/DeviceExplorer/doc/how_to_use_device_explorer.md "IoT Hub Device Explorer")
[https://github.com/uneidel/IOTSASTokenGenerator](https://github.com/uneidel/IOTSASTokenGenerator "SAS Token Generator / Export Import Tool")

Note: Document DB is also included in this Sample but documentDB Activity currently does not allow upserts.
Note: Custom Activity also includes REST Version of exporting Identities (only up to 1000)

5/12/2016 5:46:02 PM 