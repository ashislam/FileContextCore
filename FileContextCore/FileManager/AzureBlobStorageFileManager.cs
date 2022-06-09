using System;
using System.IO;
using System.Text;
using Azure.Storage.Blobs;
using FileContextCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FileContextCore.FileManager
{
    public class AzureBlobStorageFileManager : IFileManager
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly object _thisLock = new object();

        private IEntityType _type;
        private string _filetype;
        private string _path;

        //public AzureBlobStorageFileManager(BlobContainerClient blobContainerClient)
        //{
        //    _blobContainerClient = blobContainerClient;
        //}

        public AzureBlobStorageFileManager()
        {
            _blobContainerClient = new BlobContainerClient("UseDevelopmentStorage=true", "containertest");
        }

        public void Initialize(IFileContextScopedOptions options, IEntityType entityType, string fileType)
        {
            if (string.IsNullOrEmpty(options.Location) && string.IsNullOrEmpty(options.DatabaseName))
            {
                throw new ArgumentNullException(nameof(options.DatabaseName), "When location is not provided the database name cannot be empty");
            }

            _type = entityType;
            _filetype = fileType;

            _path = string.IsNullOrEmpty(options.Location)
                ? Path.Combine("appdata", options.DatabaseName)
                : options.Location;

            _blobContainerClient.CreateIfNotExists();
        }

        public string GetFileName()
        {
            if (_type == null || string.IsNullOrEmpty(_path))
            {
                throw new ArgumentException("File manager is not initialized");
            }

            string name = _type.GetTableName().GetValidFileName();

            return Path.Combine(_path, name + "." + _filetype);
        }

        public string LoadContent()
        {
            lock (_thisLock)
            {
                string path = GetFileName();
                var blobClient = _blobContainerClient.GetBlobClient(path);

                if (blobClient.Exists())
                {
                    using var memoryStream = new MemoryStream();
                    blobClient.DownloadTo(memoryStream);
                    var length = memoryStream.Length;
                    return Encoding.ASCII.GetString(memoryStream.ToArray());
                }

                return string.Empty;
            }
        }

        public void SaveContent(string content)
        {
            lock (_thisLock)
            {
                string path = GetFileName();
                var blobClient = _blobContainerClient.GetBlobClient(path);

                var byteArray = Encoding.ASCII.GetBytes(content);
                var stream = new MemoryStream(byteArray);

                blobClient.Upload(stream, overwrite: true);
            }
        }

        public bool Clear()
        {
            lock (_thisLock)
            {

                string path = GetFileName();
                var blobClient = _blobContainerClient.GetBlobClient(path);

                var response = blobClient.DeleteIfExists();

                return response;
            }
        }

        public bool FileExists()
        {
            lock (_thisLock)
            {
                string path = GetFileName();
                var blobClient = _blobContainerClient.GetBlobClient(path);

                return blobClient.Exists();
            }
        }
    }
}
