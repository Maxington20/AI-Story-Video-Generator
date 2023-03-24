using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Imaging;

namespace ChatGptStoryGenerator
{
    public static class GoogleImageSearch
    {
        public static async         Task
SearchAndDownloadImage(string topic, string number)
        {
            // Use Google Custom Search API to search for images related to the topic
            string apiKey = "AIzaSyDMg_4iX_gQd9E7WksgsWdfLMT2D4w9o-I";
            string cx = "11764fb6d23c841e5";
            string imgType = "stock";
            string url = $"https://www.googleapis.com/customsearch/v1?q=cartoon%20{topic}&cx={cx}&key={apiKey}&searchType=image&imgType={imgType}&num={number}&fileType=jpg";

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

                    using (Aspose.Imaging.Image image = Aspose.Imaging.Image.Load($"{downloadPath}\\{topic}_{count}.jpg"))
                    {
                        var newHeight = image.Height % 2 == 0 ? image.Height : image.Height + 1;
                        var newWidth = image.Width % 2 == 0 ? image.Width : image.Width + 1;

                        image.Resize(newWidth, newHeight);

                        image.Save($"{downloadPath}\\{topic}_{count}.jpg");
                    }                 
                    count++;
                }
            }

            Console.WriteLine("Done!");
        }
    }
}
