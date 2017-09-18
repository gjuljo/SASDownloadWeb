using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SASGeneratorWeb
{
    class SASGenerator
    {
        private string _accountName;
        private string _accountKey;
        private string _containerName;

        public SASGenerator(string account, string key, string container)
        {   
            _accountName   = account;
            _accountKey    = key;
            _containerName = container;
            
        }
        public async Task<string> GenerateURI(string ipaddress, string filename)
        {
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                    _accountName, _accountKey), true);

            //Create the blob client object.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            CloudBlobContainer container = blobClient.GetContainerReference(_containerName);
            await container.CreateIfNotExistsAsync();

            CloudBlockBlob blob = container.GetBlockBlobReference(filename);
            bool blobExists = await blob.ExistsAsync();

            if (!blobExists)
            {
                string blobContent = "Ciao, sono un file di test!";
                await blob.UploadTextAsync(blobContent);
            }
    
            //Set the expiry time and permissions for the blob.
            //In this case, the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

            SharedAccessProtocol sasProtocol = new SharedAccessProtocol();
            sasProtocol = SharedAccessProtocol.HttpsOnly;

            //Set the authorized IP address or range
            IPAddressOrRange authorizedIp = new IPAddressOrRange(ipaddress);

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints, null, null, sasProtocol, authorizedIp);

            //Return the URI string for the container, including the SAS token.
            string fullURI = blob.Uri + sasBlobToken;

            return fullURI;
        }
    }   

}