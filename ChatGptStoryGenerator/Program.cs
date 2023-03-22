using System;
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
        string url = "https://api.openai.com/v1/chat/completions";

        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

            var body = new DataBody { model = "gpt-3.5-turbo", messages = new List<Message>() };

            body.messages.Add(new Message { role = "user", content = $"create a ~300 word children's story about {topic}" });

            string json = JsonConvert.SerializeObject(body);

            HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            Console.Write("Now hitting the chat gpt api");
            var response = await httpClient.PostAsync(url, content);

            if(response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("\n\nWoohoo we got a 200 response");
            }

            var storyContent = await response.Content.ReadAsStringAsync();

            // Parse the JSON response to get the image URLs
            dynamic result = JsonConvert.DeserializeObject(storyContent);

            string story = result?.choices[0]?.message?.content ?? "There was some kind of error";

            Console.WriteLine(story);

            string[] strings = story.Split('\n');

            strings = strings.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var speechKey = "f5c6b2a2ee8343a6a2a4adf91b8d83c9";
            var regionKey = "eastus";

            var speechConfig = SpeechConfig.FromSubscription(speechKey, regionKey);

            speechConfig.SpeechSynthesisVoiceName = "en-US-AnaNeural";

            int count = 0;            

            foreach(string s in strings )
            {
                count++;
                var audioFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{topic}-{count}.wav";
                var imageFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\AutomationImages\\cat_1.jpg";
                var imageWithTextFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\AutomationImages\\cat_1-with-text.jpg";
                var videoFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{count}-thing.mp4";
                var logFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{count}-storylog.txt";
                var storyPartFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{count}-story.txt";

                // create a text file of the current story chunk
                using (StreamWriter writer = new StreamWriter(logFilePath))
                {
                    writer.WriteLine(s);
                }

                // add the text to the image
                AddTextToImage.AddTextToStaticImage(s, imageFilePath, imageWithTextFilePath);

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
                FFMpeg.PosterWithAudio(imageWithTextFilePath, audioFilePath, videoFilePath);

                // delete the images and files once they are no longer needed
          
            }
            // track the list of used animals and the date/time associated with each            
            using (StreamWriter writer = new StreamWriter("C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\animallog.txt", append: true))
            {             
                writer.WriteLine($"{topic} - {DateTime.Now}");
            }           
        }
        Console.WriteLine("Done!");
    }
}