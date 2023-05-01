using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatGptStoryGenerator.services
{
    public static class ImageDownloader
    {       
        public static async Task DownloadImageFromNet(string url, string folderPath, string fileName)
        {
            string fullPath = Path.Combine(folderPath, fileName);
            using (HttpClient client = new HttpClient())
            {
                // i need to download the image from the url and save it to the folder path 
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    using (HttpContent content = response.Content)
                    {
                        // read the image bytes from the response
                        byte[] imageBytes = await content.ReadAsByteArrayAsync();
                        // write the image bytes to the file
                        File.WriteAllBytes(fullPath, imageBytes);
                    }
                }            
            }
        }
    }
}
