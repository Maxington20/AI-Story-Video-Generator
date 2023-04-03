using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Imaging;
using System.Net;
using System.Drawing.Imaging;

namespace ChatGptStoryGenerator
{
    public static class GoogleImageSearch
    {
        public static async Task SearchAndDownloadImage(string topic, string number)
        {
            // Use Google Custom Search API to search for images related to the topic
            string apiKey = "AIzaSyDMg_4iX_gQd9E7WksgsWdfLMT2D4w9o-I";
            string cx = "11764fb6d23c841e5";
            string imgType = "stock";
            string url = $"https://www.googleapis.com/customsearch/v1?q=cartoon%20{topic}&cx={cx}&key={apiKey}&searchType=image&imgType={imgType}&num={number}&fileType=jpg";

            using (var httpClient = new HttpClient())
            {
                // Retry up to 5 times if there is a timeout
                int retries = 0;
                while (retries < 5)
                {
                    try
                    {
                        var response = await httpClient.GetAsync(url);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
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

                                    EnsureEvenSize(Path.Combine(downloadPath, fileName));
                                }
                                
                                count++;
                            }
                            Console.WriteLine("Done!");
                            return;
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            Console.WriteLine($"Error: Could not find resource at {url}");
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"Error: Unexpected response from server ({response.StatusCode})");
                            retries++;
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine($"Error: Request failed ({e.Message})");
                        retries++;
                    }
                    catch (TaskCanceledException e)
                    {
                        Console.WriteLine($"Error: Request timed out ({e.Message})");
                        retries++;
                    }
                    await Task.Delay(1000);
                }
                Console.WriteLine("Failed to download images.");
            }
        }
        
        public static void ResizeImage(string filePath)
        {
            using (Aspose.Imaging.Image image = Aspose.Imaging.Image.Load(filePath))
            {
                var newHeight = image.Height % 2 == 0 ? image.Height : image.Height + 1;
                var newWidth = image.Width % 2 == 0 ? image.Width : image.Width + 1;
                image.Resize(newWidth, newHeight);                
                image.Save(filePath);
            }
        }

        public static void EnsureEvenSize(string imagePath)
        {
            using (var image = new Bitmap(imagePath))
            {
                if (image.Width % 2 != 0)
                {
                    using (var newImage = new Bitmap(image.Width - 1, image.Height))
                    {
                        using (var g = System.Drawing.Graphics.FromImage(newImage))
                        {
                            g.DrawImage(image, 0, 0, image.Width - 1, image.Height);
                        }
                        image.Dispose();
                        newImage.Save(imagePath, ImageFormat.Jpeg);
                    }
                }
                if (image.Height % 2 != 0)
                {
                    using (var newImage = new Bitmap(image.Width, image.Height - 1))
                    {
                        using (var g = System.Drawing.Graphics.FromImage(newImage))
                        {
                            g.DrawImage(image, 0, 0, image.Width, image.Height - 1);
                        }
                        image.Dispose();
                        newImage.Save(imagePath, ImageFormat.Jpeg);
                    }
                }
            }
        }
    }
}
