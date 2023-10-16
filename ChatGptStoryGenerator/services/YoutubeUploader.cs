using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace ChatGptStoryGenerator
{
    public static class YoutubeUploader
    {
        public static async Task<bool> UploadVideo(string title, string description, string videoPath)
        {
            try
            {
                // Set the metadata for the video.
                Video video = new Video();
                video.Snippet = new VideoSnippet();
                video.Snippet.Title = title;
                video.Snippet.Description = description;
                video.Snippet.ChannelId = "test channel id";
                video.Status = new VideoStatus();
                video.Status.PrivacyStatus = "private";

                // Set the authentication credentials.
                UserCredential credential;
                using (var stream = new FileStream("C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\secretkey.apps.googleusercontent.com.json",
                    FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { YouTubeService.Scope.YoutubeUpload },
                        "user",
                        CancellationToken.None,
                        new FileDataStore("Youtube.Auth.Store")
                    );
                }

                // Create the YouTube service.
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "youtube-uploader-app"
                });

                // Create the video insert request.
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", File.Open(videoPath, FileMode.Open), "video/*");
                videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;

                // Upload the video.
                await videosInsertRequest.UploadAsync();
                return true;
            }
            catch (Google.GoogleApiException ex)
            {
                Console.WriteLine($"A Google API error occurred while uploading video: {ex.Message}");
                return false;
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine($"An I/O error occurred while uploading video: {ex.Message}");
                return false;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"The upload operation was cancelled: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while uploading video: {ex.Message}");
                return false;
            }
        }

        private static void VideosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            Console.WriteLine($"Bytes sent: {progress.BytesSent}");
        }

        private static void VideosInsertRequest_ResponseReceived(Video video)
        {
            Console.WriteLine($"Video id '{video.Id}' was successfully uploaded.");
        }
    }
}
