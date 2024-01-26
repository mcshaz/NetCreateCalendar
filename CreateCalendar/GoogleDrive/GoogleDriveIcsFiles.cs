using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CreateCalendar.GoogleDrive
{
    internal sealed class GoogleDriveIcsFiles: IDisposable
    {
        private readonly DriveService _driveService;
        private const string _folderMimeType = "application/vnd.google-apps.folder";
        private const string _calendarMimeType = "text/calendar";

        private GoogleDriveIcsFiles(DriveService driveService)
        {
            _driveService = driveService;
        }
        public string WorkingFolderId { get; set; }
        private string RootFolderId { get; set; }
        public static async Task<GoogleDriveIcsFiles> Create(string user)
        {
            UserCredential usrCred;
            using (var strm = System.IO.File.OpenRead("client_secret.json"))
            {
                GoogleClientSecrets fromStream = await GoogleClientSecrets.FromStreamAsync(strm);
                usrCred = await GoogleWebAuthorizationBroker.AuthorizeAsync(fromStream.Secrets,
                    new[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile },
                    user,
                    CancellationToken.None,
                    new FileDataStore("Drive.Auth.Store"));
            }
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = usrCred,
                ApplicationName = "Roster to Calendar",
            });
            var request = driveService.Files.Get("root");
            request.Fields = "id";
            return new GoogleDriveIcsFiles(driveService)
            {
                RootFolderId = (await request.ExecuteAsync()).Id
            };
        }

        public async Task<Uri> CreateFile(string fileName, Stream fileContents)
        {
            const string calendarFileExt = ".ics";
            var driveFile = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName.EndsWith(calendarFileExt)
                    ? fileName
                    : (fileName + calendarFileExt),
                MimeType = _calendarMimeType,
                Parents = new[] { WorkingFolderId ?? RootFolderId },
            };

            var request = _driveService.Files.Create(driveFile, fileContents, _calendarMimeType);
            request.Fields = "id";

            // insertRequest.ProgressChanged += Upload_ProgressChanged;
            // insertRequest.ResponseReceived += Upload_ResponseReceived;

            await request.UploadAsync();
            var file = request.ResponseBody;
            var pr = _driveService.Permissions.Create(new Permission
            {
                Role = "reader",
                Type = "anyone",
                AllowFileDiscovery = false
            }, file.Id);
            await pr.ExecuteAsync();
            return new Uri($"https://drive.google.com/uc?export=download&id={file.Id}");
        }
        public async Task<Uri> UpdateFile(Google.Apis.Drive.v3.Data.File oldDriveFile, Stream fileContents)
        {
            // only populate fields we want to modify (patch) which is actually only the file content
            var patchedFile = new Google.Apis.Drive.v3.Data.File();
            var request = _driveService.Files.Update(patchedFile, oldDriveFile.Id,fileContents, _calendarMimeType);

            // insertRequest.ProgressChanged += Upload_ProgressChanged;
            // insertRequest.ResponseReceived += Upload_ResponseReceived;

            await request.UploadAsync();
            return new Uri($"https://drive.google.com/uc?export=download&id={oldDriveFile.Id}");
        }
        public Task WriteToStream(string fileId, Stream stream)
        {
            var request = _driveService.Files.Get(fileId);
            return request.DownloadAsync(stream);
        }
        public async Task<IEnumerable<Google.Apis.Drive.v3.Data.File>> ListFiles()
        {
          
            var fileList = _driveService.Files.List();
            fileList.Q = $"mimeType='{_calendarMimeType}' and '{ (WorkingFolderId ?? "root") }' in parents and trashed=false";
            fileList.Fields = "nextPageToken, files(id,name,size)";

            var result = new List<Google.Apis.Drive.v3.Data.File>();
            string pageToken = null;
            do
           {
                fileList.PageToken = pageToken;
                var filesResult = await fileList.ExecuteAsync();
                var files = filesResult.Files;
                pageToken = filesResult.NextPageToken;
                result.AddRange(files);
            } while (pageToken != null);

            return result;
        }
        public async Task<string> GetFolderId(string folderPath)
        {
            var folders = folderPath.Split('/');

            var fileList = _driveService.Files.List();
            fileList.Q = $"mimeType='{_folderMimeType}' and name in '{string.Join("','", folders)}' and trashed=false";
            fileList.Fields = "files(id,name,parents)";
            var filesResult = await fileList.ExecuteAsync();

            string lastFolderId = RootFolderId;
            foreach (var folder in folders)
            {
                var match = filesResult.Files.SingleOrDefault(f => f.Name == folder
                        && lastFolderId == null ? f.Parents == null : f.Parents.Contains(lastFolderId))
                    ?? await CreateFolder(folder, lastFolderId);
                lastFolderId = match.Id;
            }
            return lastFolderId;
        }
        public Task<Google.Apis.Drive.v3.Data.File> CreateFolder(string folderName, string parentFolderId)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = _folderMimeType,
                Parents = new[] { parentFolderId }
            };
            var request = _driveService.Files.Create(fileMetadata);
            request.Fields = "id";
            return request.ExecuteAsync();
        }

        public void Dispose()
        {
            // note as the only way we can instantiate this class is through the static create method
            // we own (and can therefore dispose) driveservice
            ((IDisposable)_driveService).Dispose();
        }
    }
}
