﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatGptStoryGenerator.services
{
    public static class ImageDownloader
    {
        public static async Task DownloadImage(string url, string folderPath, string fileName)
        {
            string fullPath = Path.Combine(folderPath, fileName);

            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(url, fullPath);
            }
        }
    }
}