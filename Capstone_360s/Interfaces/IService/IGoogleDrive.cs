namespace Capstone_360s.Interfaces.IService
{
    public interface IGoogleDrive
    {
        public Task<string> CreateFolderAsync(string folderName, string parentFolderId);
        public Task<IList<Google.Apis.Drive.v3.Data.File>> QueryForAllFiles(string folderId, string query);
        public IList<string> GetFileIds(IList<Google.Apis.Drive.v3.Data.File> files);
        //public Task<Google.Apis.Drive.v3.Data.File> QueryForOneFile(string query);
        public Task<IList<string>> GetAllFileIdsFromQuery(string folderId, string query);
        public Task<(Google.Apis.Drive.v3.Data.File, MemoryStream)> GetOneFile(string fileId, bool download);
        //public Task<(MemoryStream, Google.Apis.Drive.v3.Data.File)> DownloadFile(string fileId);
        public Task<string> UploadFile(IFormFile file, string folderId);
        public Task DeleteFile(string fileId);
    }
}