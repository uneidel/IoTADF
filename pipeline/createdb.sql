Create TABLE DeviceIdentities
(
   ID   nvarchar(255) ,
   Status VARCHAR (20),
   primaryKey nvarchar(255),
   secondaryKey nvarchar(255)
   ,PRIMARY KEY (ID)
);


Create TYPE [dbo].[DeviceType] AS TABLE
(
   [id]   nvarchar(255) ,
   [status] VARCHAR (20)    ,
   [authentication.symmetricKey.primaryKey] nvarchar(255) ,
   [authentication.symmetricKey.secondaryKey] nvarchar(255)  
)

Create PROCEDURE dbo.UpsertDeviceIdentity 
	 @DeviceIdentities [dbo].DeviceType READONLY
AS
BEGIN
  MERGE INTO [dbo].DeviceIdentities as Target
  using @DeviceIdentities as Source
	on [Source].id = [target].ID
	
  WHEN MATCHED THEN 
  UPDATE SET [Target].[primaryKey] = [Source].[authentication.symmetricKey.primaryKey], 
			   [Target].[secondaryKey] = [Source].[authentication.symmetricKey.secondaryKey], 
			   [Target].[Status] = [Source].[status]
 
 WHEN NOT MATCHED THEN 
	 INSERT(id, [status],primaryKey,secondaryKey) 
	 VALUES ([Source].[Id], 
			 [Source].[status], 
			 [Source].[authentication.symmetricKey.primaryKey],
			 [Source].[authentication.symmetricKey.secondaryKey]);
END
GO


-- Drop Table DeviceIdentities
-- go
-- Drop procedure UpsertDeviceIdentity
--	go
--	exec sp_droptype [DeviceType]

	