using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Microsoft.Live;

namespace Finance.Managers
{
    public abstract class OneDriveManager
    {
        public static async Task<string> CreateDirectoryAsync(LiveConnectClient client, string folderName,
            string parentFolder)
        {
            string folderId = null;

            // Retrieves all the directories.
            var queryFolder = parentFolder + "/files?filter=folders,albums";
            var opResult = await client.GetAsync(queryFolder);
            dynamic result = opResult.Result;

            foreach (var folder in result.data)
            {
                // Checks if current folder has the passed name.
                if (folder.name.ToLowerInvariant() == folderName.ToLowerInvariant())
                {
                    folderId = folder.id;
                    break;
                }
            }

            if (folderId == null)
            {
                // Directory hasn't been found, so creates it using the PostAsync method.
                var folderData = new Dictionary<string, object> {{"name", folderName}};
                opResult = await client.PostAsync(parentFolder, folderData);
                result = opResult.Result;

                // Retrieves the id of the created folder.
                folderId = result.id;
            }

            return folderId;
        }

        public static async void UploadFileAsync(LiveConnectClient client, string localFolderName, string localFileName,
            string webFolderName, string webFileName)
        {
            var skyDriveFolder = await CreateDirectoryAsync(client, webFolderName, "me/skydrive");
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var isfs = isf.OpenFile(localFolderName + "\\" + localFileName, FileMode.Open,
                    FileAccess.Read);
                await client.UploadAsync(skyDriveFolder, webFileName, isfs, OverwriteOption.Overwrite);
                isfs.Close();
            }
        }

        public static async Task<string> DownloadFileAsync(LiveConnectClient client, string folderName, string fileName)
        {
            var skyDriveFolder = await CreateDirectoryAsync(client, folderName, "me/skydrive");
            await client.DownloadAsync(skyDriveFolder);

            var operation = await client.GetAsync(skyDriveFolder + "/files");

            var items = operation.Result["data"] as List<object>;
            var id = string.Empty;

            // Search for the file - add handling here if File Not Found
            if (items != null)
            {
                foreach (var item in items)
                {
                    var file = item as IDictionary<string, object>;
                    if (file != null && file["name"].ToString() == fileName)
                    {
                        id = file["id"].ToString();
                        break;
                    }
                }
            }

            var downloadResult = await client.DownloadAsync(string.Format("{0}/content", id));

            var reader = new StreamReader(downloadResult.Stream);
            var text = await reader.ReadToEndAsync();
            return text;
        }
    }
}