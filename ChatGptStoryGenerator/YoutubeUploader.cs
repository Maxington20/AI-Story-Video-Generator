using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChatGptStoryGenerator
{
    public static class YoutubeUploader
    {
        public static async void UploadVideo(string title, string description)
        {
            // Set the path to the video file.
            string videoPath = @"C:\path\to\video.mp4";

            // Set the metadata for the video.
            Video video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = title;
            video.Snippet.Description = description;
            video.Snippet.ChannelId = "UC7TlFqGEuc0kXcKBwI-THJg";
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "public";

            // Set the authentication credentials.
            UserCredential credential;
            using (var stream = new FileStream("C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\client_secret_71511348519-9r709uh7nuhq1urvlt3slgo3cp2vkhnv.apps.googleusercontent.com.json", 
                FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None,
                    new FileDataStore("YouTubeUploader")
                );
            }

            // Create the YouTube service.
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "YouTubeUploader"
            });

            // Create the video insert request.
            var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", File.Open(videoPath, FileMode.Open), "video/*");
            videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
            videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;

            // Upload the video.
            await videosInsertRequest.UploadAsync();
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
