﻿using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using Microsoft.CognitiveServices.Speech;
using FFMpegCore;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using ChatGptStoryGenerator;
using System.Security.Cryptography.X509Certificates;

class Program
{
    public class DataBody
    {
        public string model { get; set; }
        public List<Message> messages { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class ImagePrompt
    {
        public string prompt { get; set; }

        // number of images
        public int n { get; set; }
        public string size { get; set; }
    }

    static async Task Main(string[] args)
    {
        // List of topics to choose from
        List<string> topics = new List<string>
        {
            "cat", "dog", "ferret", "chinchilla", "pickle", "tomato", "fish", "octopus", "alligator", "snail", "bumblebee", "shark", "whale", "sloth"
        };

        // Choose a random topic from the list
        Random rand = new Random();
        string topic = topics[rand.Next(topics.Count)];

        // Use chatgpt API to generate the story
        string apiKey = "Bearer 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";
        string conversation_id = Guid.NewGuid().ToString("N");
        string url = "https://api.openai.com/v1/chat/completions";
        string imageUrl = "https://api.openai.com/v1/images/generations";


        // generate the story
        var story = await ChatGPTRequest.MakeChatGPTCompletionRequest(apiKey, url, $"create a ~300 word original children's story about {topic}");

        if (story == null)
        {
            throw new Exception("The story cannot be null");
        }
        dynamic result = JsonConvert.DeserializeObject(story);
        story = result?.choices[0]?.message?.content ?? null;

        // generate a title for the story
        var storyTitle = await ChatGPTRequest.MakeChatGPTCompletionRequest(apiKey, url, $"come up with a name for this story with no heading and no title label ahead of the name: {story}");
        dynamic titleResult = JsonConvert.DeserializeObject(storyTitle);
        storyTitle = titleResult?.choices[0]?.message?.content ?? null;

        // generate the story description
        var storyDescription = await ChatGPTRequest.MakeChatGPTCompletionRequest(apiKey, url, $"~10 word description of this story: {story}");
        dynamic desctiptionResult = JsonConvert.DeserializeObject(storyDescription);
        storyDescription = desctiptionResult?.choices[0]?.message?.content ?? null;

        Console.WriteLine(story);

        string[] strings = story.Split('\n');

        strings = strings.Where(x => !string.IsNullOrEmpty(x)).ToArray();

        var speechKey = "f5c6b2a2ee8343a6a2a4adf91b8d83c9";
        var regionKey = "eastus";

        var speechConfig = SpeechConfig.FromSubscription(speechKey, regionKey);

        speechConfig.SpeechSynthesisVoiceName = "en-US-AnaNeural";

        var videoFilePaths = new string[strings.Length];
        var finalVidFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\final-thing.mp4";

        int count = 0;

        await GoogleImageSearch.SearchAndDownloadImage(topic, strings.Length.ToString());

        foreach(string s in strings )
        {
            count++;

            // use chat gpt to create a relevant prompt for midjourney
            var midjourneyPrompt = await ChatGPTRequest.MakeChatGPTCompletionRequest(apiKey, url, $"~20 word picture description for the following scene: {s}, within the context of the whole story: {story}");
            // Parse the JSON response to get the image URLs
            dynamic promptResult = JsonConvert.DeserializeObject(midjourneyPrompt);

            var finalPrompt = promptResult?.choices[0]?.message?.content ?? null;

            if ( finalPrompt != null )
            {
                finalPrompt = "a highly detailed, child appropriate cartoon of: " + finalPrompt + " with no words";
            }

            // use the prompt to give to midjourney
            Console.WriteLine($"\n{finalPrompt}");

            var chatGPTImagePrompt = await ChatGPTRequest.MakeChatGPTImageGenrationRequest(apiKey, imageUrl, Convert.ToString(finalPrompt));

            dynamic imageResponse = JsonConvert.DeserializeObject(chatGPTImagePrompt);

            var imageToDownloadUrl = Convert.ToString(imageResponse.data[0].url);

            // Download the image
            ImageDownloader.DownloadImage(imageToDownloadUrl, $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\AutomationImages\\", $"{topic}_{count}.png");

            var audioFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{topic}-{count}.wav";
            var imageFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\AutomationImages\\{topic}_{count}.png";
            //var imageWithTextFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\AutomationImages\\{topic}_{count}-with-text.jpg";
            var videoFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{count}-thing.mp4";
            var logFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{count}-storylog.txt";
            var storyPartFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{count}-story.txt";

            // create a text file of the current story chunk
            using (StreamWriter writer = new StreamWriter(logFilePath))
            {
                writer.WriteLine(s);
            }

            // add the text to the image
            // AddTextToImage.AddTextToStaticImage(s, imageFilePath, imageWithTextFilePath);

            // create the audio file using the micosoft speech synthesizer
            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                var speechResult = await speechSynthesizer.SpeakTextAsync(s);
                Console.WriteLine($"Saving the audio for part {count} of {strings.Length} to a file");
                using var stream = AudioDataStream.FromResult(speechResult);
                // saving the audio file
                await stream.SaveToWaveFileAsync(audioFilePath);

                stream.Dispose();                                     
            }

            // create the video that combines the audio and image and text
            // fixing something imageWithTextFilePath
            FFMpeg.PosterWithAudio(imageFilePath, audioFilePath, videoFilePath);

            videoFilePaths[count - 1] = videoFilePath;
        }
        //combine the videos into one video
        Task.Run(() =>
        {
            FFMpeg.Join(finalVidFilePath, videoFilePaths);
        }).Wait();


        await Task.Delay(TimeSpan.FromSeconds(15));        

        //upload the final video to youtube
        if(storyTitle != null  && storyDescription != null)
        {          
            await YoutubeUploader.UploadVideo(storyTitle, storyDescription, finalVidFilePath);          
        }
        else
        {
            throw new Exception("story title and story description are required to publish video to youtube");
        }
        
        
        // track the list of used animals and the date/time associated with each            
        using (StreamWriter writer = new StreamWriter("C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\animallog.txt", append: true))
        {             
            writer.WriteLine($"{topic} - {DateTime.Now}");
        }

        // delete all the uncessary files and just leave the final video file and animal log
        Console.WriteLine("Deleting the unecessary files");
        for (int i = 1; i < count + 1; i++)
        {
            File.Delete($"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\AutomationImages\\{topic}_{i}-with-text.jpg");
            File.Delete($"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{topic}-{i}.wav");
            File.Delete($"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{i}-thing.mp4");
            File.Delete($"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{i}-storylog.txt");
        }
        
        Console.WriteLine("Done!");
    }
}