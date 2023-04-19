using Newtonsoft.Json;
using Microsoft.CognitiveServices.Speech;
using FFMpegCore;
using ChatGptStoryGenerator.services;
using ChatGptStoryGenerator;

partial class Program
{
    static async Task Main(string[] args)
    {
        // List of topics to choose from
        List<string> topics = new List<string>
        {
            "cat", "dog", "ferret", "chinchilla", "pickle", "tomato", "fish", "octopus", "alligator", "snail", "bumblebee", "shark", "whale", "sloth",
            "bear", "horse", "snake", "walrus", "leopard", "lemur", "elk", "deer", "rooster", "zebra", "fish", "parrot", "owl", "pig", "armadillo", "gopher",
            "beaver", "bat", "seal", "mouse", "eagle", "crab", "rat", "mole", "rabbit"
        };

        for(int times = 0; times < 5; times++)
        {
            // Choose a random topic from the list
            Random rand = new Random();
            string topic = topics[rand.Next(topics.Count)];

            // Use chatgpt API to generate the story
            string apiKey = "Bearer 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";
            string conversation_id = Guid.NewGuid().ToString("N");
            string url = "https://api.openai.com/v1/chat/completions";
            string imageUrl = "https://api.openai.com/v1/images/generations";


            // generate the story
            var story = await ChatGPTRequest.MakeChatGPTCompletionRequestAsync(apiKey, url, $"create a ~300 word original children's story about {topic}");

            if (story == null)
            {
                throw new Exception("The story cannot be null");
            }
            dynamic result = JsonConvert.DeserializeObject(story);
            story = result?.choices[0]?.message?.content ?? null;

            // generate a title for the story
            var storyTitle = await ChatGPTRequest.MakeChatGPTCompletionRequestAsync(apiKey, url, $"come up with a name for this story with no heading and no title label ahead of the name: {story}");
            dynamic titleResult = JsonConvert.DeserializeObject(storyTitle);
            storyTitle = titleResult?.choices[0]?.message?.content ?? null;

            // generate the story description
            var storyDescription = await ChatGPTRequest.MakeChatGPTCompletionRequestAsync(apiKey, url, $"~10 word description of this story: {story}");
            dynamic desctiptionResult = JsonConvert.DeserializeObject(storyDescription);
            storyDescription = desctiptionResult?.choices[0]?.message?.content ?? null;

            Console.WriteLine(story);

            string[] strings = story.Split('\n');

            strings = strings.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var videoFilePaths = new string[strings.Length];
            var dateString = DateTime.Now.ToString();
            var finalVidFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{topic}-{dateString}.mp4";

            int count = 0;

            // no longer need to download images from google so commenting it out
            // await GoogleImageSearch.SearchAndDownloadImage(topic, strings.Length.ToString());

            foreach (string s in strings)
            {
                count++;

                // use chat gpt to create a relevant prompt for midjourney
                var midjourneyPrompt = await ChatGPTRequest.MakeChatGPTCompletionRequestAsync(apiKey, url, $"~20 word picture description for the following scene: {s}, within the context of the whole story: {story}");
                // Parse the JSON response to get the image URLs
                dynamic promptResult = JsonConvert.DeserializeObject(midjourneyPrompt);

                var finalPrompt = promptResult?.choices[0]?.message?.content ?? null;

                if (finalPrompt != null)
                {
                    finalPrompt = "a highly detailed, child appropriate picture of: " + finalPrompt;
                }

                var chatGPTImagePrompt = await ChatGPTRequest.MakeChatGPTImageGenrationRequestAsync(apiKey, imageUrl, Convert.ToString(finalPrompt));

                dynamic imageResponse = JsonConvert.DeserializeObject(chatGPTImagePrompt);

                var imageToDownloadUrl = Convert.ToString(imageResponse.data[0].url);

                // Download the image
                ImageDownloader.DownloadImage(imageToDownloadUrl, $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\AutomationImages\\", $"{topic}_{count}.png");

                var audioFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{topic}-{count}.wav";
                var imageFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\AutomationImages\\{topic}_{count}.png";
                var videoFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{count}-thing.mp4";
                var logFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{count}-storylog.txt";
                var storyPartFilePath = $"C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\StoryLogs\\{count}-story.txt";

                // create a text file of the current story chunk
                using (StreamWriter writer = new StreamWriter(logFilePath))
                {
                    writer.WriteLine(s);
                }

                // create the audio file using the micosoft speech synthesizer
                Console.WriteLine($"Saving the audio for part {count} of {strings.Length} to a file");
                await Speech.TextToSpeech(s, audioFilePath);

                // create the video that combines the audio and image and text
                FFMpeg.PosterWithAudio(imageFilePath, audioFilePath, videoFilePath);

                videoFilePaths[count - 1] = videoFilePath;

                // delete the files that are no longer needed
                File.Delete(audioFilePath);
                File.Delete(imageFilePath);
            }
            //combine the videos into one video
            Task.Run(() =>
            {
                FFMpeg.Join(finalVidFilePath, videoFilePaths);
            }).Wait();

            // need to esnure the video is done being created
            await Task.Delay(TimeSpan.FromSeconds(20));

            //upload the final video to youtube
            if (storyTitle != null && storyDescription != null)
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
}