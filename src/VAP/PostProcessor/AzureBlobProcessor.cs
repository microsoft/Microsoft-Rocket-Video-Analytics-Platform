// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PostProcessor
{
    public class AzureBlobProcessor
    {
        private static CloudStorageAccount storageAccount;
        private static CloudBlobClient cloudBlobClient;

        public AzureBlobProcessor()
        {
            // Retrieve the connection string from app.config
            string storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    cloudBlobClient = storageAccount.CreateCloudBlobClient();
                }
                catch (StorageException ex)
                {
                    Console.WriteLine("Error returned from the service: {0}", ex.Message);
                }
            }
        }

        public async Task<string> CreateContainerAsync(string container = "")
        {
            try
            {
                // Create a container (appending a GUID value to it to make the name unique). 
                if (container == "")
                {
                    container = Guid.NewGuid().ToString();
                }
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(container);
                await cloudBlobContainer.CreateIfNotExistsAsync();
                Console.WriteLine("Created container '{0}'", cloudBlobContainer.Name);
                Console.WriteLine();

                // Set the permissions so the blobs are public. 
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await cloudBlobContainer.SetPermissionsAsync(permissions);

            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
            }

            return container;
        }

        public async Task DeleteContainerAsync(string containerName)
        {
            try
            {
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
                if (cloudBlobContainer != null)
                {
                    await cloudBlobContainer.DeleteIfExistsAsync();
                }
            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
            }
        }

        public async Task<string> UploadFileAsync(string containerName, string blobName, string sourceFile)
        {
            string blobUri = null;
            try
            {
                // Get a reference to the blob address, then upload the file to the blob.
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
                await cloudBlockBlob.UploadFromFileAsync(sourceFile);
                blobUri = cloudBlobContainer.Uri.AbsoluteUri + "/" + blobName;
            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
            }

            return blobUri;
        }

        public async Task DownloadFileAsync(string containerName, string blobName, string destinationFile)
        {
            try
            {
                // Download the blob to a local file. 
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
                Console.WriteLine("Downloading blob {0} to {1}", blobName, destinationFile);
                Console.WriteLine();
                await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);
            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
            }
        }

        public async Task ListBlobAsync(string containerName)
        {
            try
            {
                // List the blobs in the container.
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
                Console.WriteLine("Listing blobs in container {0}.", containerName);
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    // Get the value of the continuation token returned by the listing call.
                    blobContinuationToken = results.ContinuationToken;
                    foreach (IListBlobItem item in results.Results)
                    {
                        Console.WriteLine(item.Uri);
                    }
                } while (blobContinuationToken != null); // Loop while the continuation token is not null.
                Console.WriteLine();
            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
            }
        }
    }
}
