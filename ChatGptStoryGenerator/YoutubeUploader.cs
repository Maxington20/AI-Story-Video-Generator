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
        public static async void UploadVideo(string title, string description, string videoPath)
        {                        
            // Set the metadata for the video.
            Video video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = title;
            video.Snippet.Description = description;
            video.Snippet.ChannelId = "UCxcdczLSSuc8a2dWwd25aLg";
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
                    "max.herrington@gmail.com",
                    CancellationToken.None,
                    new FileDataStore("youtube-uploader-app")
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
        }

        private static void VideosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            Console.WriteLine($"Bytes sent: {progress.BytesSent}");
        }

        private static void VideosInsertRequest_ResponseReceived(Video video)
        {
            Console.WriteLine($"Video id '{video.Id}' was successfully uploaded.");
        }

        public static async void UploadVideoProper()
        {

            UserCredential credential;

            using (var stream = new FileStream("C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\client_secret_71511348519-9r709uh7nuhq1urvlt3slgo3cp2vkhnv.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None
                    );
                    //new FileDataStore("YouTube.Auth.Store")).Result;
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "YourApplicationName"
            });

            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = "Test video";
            video.Snippet.Description = "This is a test video.";
            video.Snippet.Tags = new string[] { "test", "video" };
            video.Snippet.CategoryId = "22"; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "public"; // "private", "public", or "unlisted"

            using (var fileStream = new FileStream("C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\final-thing.mp4", FileMode.Open))
            {
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;
                var uploadResponse = videosInsertRequest.Upload();
                Console.WriteLine("Video was successfully uploaded with ID: " + uploadResponse);
            }

        }
    }    
}
