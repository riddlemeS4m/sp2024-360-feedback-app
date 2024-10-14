using Google.Apis.Download;
using Google.Apis.Upload;
using Newtonsoft.Json.Linq;

namespace Capstone_360s.Utilities
{
    public class GoogleDriveUtility
    {
        public static string SerializeCredentials(IConfigurationSection googleCredentialSection)
        {
            var json = new JObject();
            foreach (var child in googleCredentialSection.GetChildren())
            {
                json.Add(child.Key, child.Value);
            }

            return json.ToString();
        }

        //alternatively...
        //public static string SerializeCredentials(IConfigurationSection googleCredentialSection)
        //{

        //    var googleCredentialJson = new
        //    {
        //        type = googleCredentialSection["type"],
        //        project_id = googleCredentialSection["project_id"],
        //        private_key_id = googleCredentialSection["private_key_id"],
        //        private_key = googleCredentialSection["private_key"],
        //        client_email = googleCredentialSection["client_email"],
        //        client_id = googleCredentialSection["client_id"],
        //        auth_uri = googleCredentialSection["auth_uri"],
        //        token_uri = googleCredentialSection["token_uri"],
        //        auth_provider_x509_cert_url = googleCredentialSection["auth_provider_x509_cert_url"],
        //        client_x509_cert_url = googleCredentialSection["client_x509_cert_url"]
        //    };

        //    return JsonSerializer.Serialize(googleCredentialJson);
        //}

        public static void Upload_ProgressChanged(IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    // Optionally, log the progress of the upload
                    Console.WriteLine($"Uploading: {progress.BytesSent} bytes sent.");
                    break;
                case UploadStatus.Completed:
                    // Optionally, log the completion of the upload
                    Console.WriteLine("Upload completed successfully.");
                    break;
                case UploadStatus.Failed:
                    // Optionally, log the failure of the upload
                    Console.WriteLine($"Upload failed: {progress.Exception.Message}");
                    break;
            }
        }

        public static void Download_ProgressChanged(IDownloadProgress progress)
        {
            switch (progress.Status)
            {
                case DownloadStatus.Downloading:
                    // Optionally, log the progress of the download
                    Console.WriteLine($"Downloading: {progress.BytesDownloaded} bytes downloaded.");
                    break;
                case DownloadStatus.Completed:
                    // Optionally, log the completion of the download
                    Console.WriteLine("Download completed successfully.");
                    break;
                case DownloadStatus.Failed:
                    // Optionally, log the failure of the download
                    Console.WriteLine($"Download failed: {progress.Exception.Message}");
                    break;
            }
        }
    }
}
