using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
//using System.Speech.Synthesis;
using Microsoft.CognitiveServices.Speech;

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

        // Use Google Custom Search API to search for images related to the topic
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
                Console.WriteLine("Woohoo we got a 200 response");
            }

            var storyContent = await response.Content.ReadAsStringAsync();

            // Parse the JSON response to get the image URLs
            dynamic result = JsonConvert.DeserializeObject(storyContent);

            string story = result?.choices[0]?.message?.content ?? "There was some kind of error";

            Console.WriteLine(story);

            var  speechKey = "f5c6b2a2ee8343a6a2a4adf91b8d83c9";
            var  regionKey = "eastus";

            var speechConfig = SpeechConfig.FromSubscription(speechKey, regionKey);

            speechConfig.SpeechSynthesisVoiceName = "en-US-AnaNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                var speechResult = await speechSynthesizer.SpeakTextAsync(story);

                Console.WriteLine("Saving the audio to a file");
                using var stream = AudioDataStream.FromResult(speechResult);
                await stream.SaveToWaveFileAsync($"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{topic}.wav");
            }        

            // track the list of used animals and the date/time associated with each            
            using (StreamWriter writer = new StreamWriter("C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\animallog.txt", append: true))
            {             
                writer.WriteLine($"{topic} - {DateTime.Now}");
            }

            // track the most recent story
            using (StreamWriter writer = new StreamWriter("C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\storylog.txt"))
            {
                writer.WriteLine(story);
            }
        }

        Console.WriteLine("Done!");
    }
}