using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace ChatGptStoryGenerator
{
    public static class ChatGPTRequest
    {        

        public static async Task<string> MakeChatGPTRequest(string apiKey,string url,string searchContent)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

                httpClient.Timeout = Timeout.InfiniteTimeSpan;

                var body = new DataBody { model = "gpt-3.5-turbo", messages = new List<Message>() };

                body.messages.Add(new Message { role = "user", content = searchContent });

                string json = JsonConvert.SerializeObject(body);

                HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                Console.Write("Now hitting the chat gpt api");
                var response = await httpClient.PostAsync(url, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("\n\nWoohoo we got a 200 response");
                }

                 return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
