using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;

class Program
{
    static async Task Main(string[] args)
    {
        // List of topics to choose from
        List<string> topics = new List<string>
        {
            "cat", "dog", "ferret", "chinchilla", "pickle"
        };

        // Choose a random topic from the list
        Random rand = new Random();
        string topic = topics[rand.Next(topics.Count)];

        // Use Google Custom Search API to search for images related to the topic
        string apiKey = "AIzaSyDMg_4iX_gQd9E7WksgsWdfLMT2D4w9o-I";
        string cx = "11764fb6d23c841e5";
        string imgType = "stock";
        string url = $"https://www.googleapis.com/customsearch/v1?q=cartoon%20{topic}&cx={cx}&key={apiKey}&searchType=image&imgType={imgType}";     

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            // Parse the JSON response to get the image URLs
            dynamic result = JsonConvert.DeserializeObject(content);
            List<string> imageUrls = new List<string>();
            foreach (var item in result.items)
            {
                imageUrls.Add(item.link.ToString());
            }

            // Download the images to a local directory
            string downloadPath = "C:\\Users\\maxhe\\OneDrive\\Pictures\\Saved Pictures\\AutomationImages";
            int count = 1;
            foreach (string imageUrl in imageUrls)
            {
                using (var imageResponse = await httpClient.GetAsync(imageUrl))
                {
                    var imageContent = await imageResponse.Content.ReadAsStreamAsync();
                    var fileName = $"{topic}_{count}.jpg";
                    using (var fileStream = new FileStream(Path.Combine(downloadPath, fileName), FileMode.Create))
                    {
                        await imageContent.CopyToAsync(fileStream);
                    }
                    Console.WriteLine($"Downloaded image: {fileName}");
                }
                count++;
            }
        }

        Console.WriteLine("Done!");
    }
}
