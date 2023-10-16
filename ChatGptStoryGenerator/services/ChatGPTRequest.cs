using Newtonsoft.Json;
using System.Net;
using System.Text;
using static Program;

namespace ChatGptStoryGenerator.services
{
    public static class ChatGPTRequest
    {
        public static async Task<string> MakeChatGPTCompletionRequestAsync(string apiKey, string url, string searchContent)
        {
            int retries = 5;

            while (retries > 0)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

                        var body = new DataBody { model = "gpt-3.5-turbo", messages = new List<Message>() };

                        body.messages.Add(new Message { role = "user", content = searchContent });

                        string json = JsonConvert.SerializeObject(body);

                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        Console.Write("Now hitting the chat gpt completion api");
                        var response = await httpClient.PostAsync(url, content);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Console.WriteLine("\n\nGot 200 response");
                            return await response.Content.ReadAsStringAsync();
                        }
                        if(response.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            Thread.Sleep(20000);
                        }
                        else if (retries > 1)
                        {
                            Console.WriteLine($"Request failed with status code {response.StatusCode}. Retrying {retries - 1} more times.");
                            retries--;
                        }
                        else
                        {
                            Console.WriteLine($"Request failed with status code {response.StatusCode} after retrying 5 times.");
                            return null;
                        }
                    }
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    if (retries > 1)
                    {
                        Console.WriteLine($"Request timed out. Retrying {retries - 1} more times.");
                        retries--;
                    }
                    else
                    {
                        Console.WriteLine("Request timed out after retrying 5 times.");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return null;
                }
            }

            Console.WriteLine("Failed to get a response after retrying 5 times.");
            return null;
        }

        public static async Task<string> MakeChatGPTImageGenrationRequestAsync(string apiKey, string url, string prompt)
        {
            Thread.Sleep(20000);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

                var imagePrompt = new ImagePrompt { n = 1, prompt = prompt, size = "1024x1024" };

                string json = JsonConvert.SerializeObject(imagePrompt);

                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.Write("Now hitting the chat gpt image generator api");
                var response = await httpClient.PostAsync(url, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("\n\ngot a 200 response");
                    return await response.Content.ReadAsStringAsync();
                }               
                else
                {
                    Console.WriteLine($"Request failed with status code {response.StatusCode} after retrying 5 times.");
                    return null;
                }
            }
        }
    }
}
