using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonViz.Utils
{
    class OpenAiClient
    {
        private const string ENDPOINT = "https://api.openai.com/v1/chat/completions";

        readonly HttpClient Client;

        public OpenAiClient()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
        }
        public string QueryChatGPT(OpenAIQueryObject requestBody)
        {
            string json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            //Console.Write(json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Task<HttpResponseMessage> response = Client.PostAsync(ENDPOINT, content);
            response.Wait();
            Task<string> responseString = response.Result.Content.ReadAsStringAsync();
            responseString.Wait();
            string message =
                JsonDocument.Parse(responseString.Result)
                    .RootElement
                    .GetProperty("choices")
                    .EnumerateArray()
                    .FirstOrDefault()
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "NULL";

            return message;
        }
    }
}
