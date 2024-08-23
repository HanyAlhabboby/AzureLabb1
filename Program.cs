using Azure;
using Azure.AI.Language.QuestionAnswering;
using System;
using System.Text;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;



namespace AzureLabb1
{
    internal class Program
    {
        private static string translatorEndpoint = "https://api.cognitive.microsofttranslator.com";
        private static string cogSvcKey;
        private static string cogSvcRegion;
        static async Task Main(string[] args)
        {

            // Get config settings from AppSettings
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            cogSvcKey = configuration["CognitiveServiceKey"];
            cogSvcRegion = configuration["CognitiveServiceRegion"];

            // Set console encoding to unicode
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;


            // This example requires environment variables named "LANGUAGE_KEY" and "LANGUAGE_ENDPOINT"
            Uri endpoint = new Uri("https://languageai123456.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new AzureKeyCredential("22b6c50f8efb4a0ba92b19b21cc2f504");
            string projectName = "LearnFAQ";
            string deploymentName = "production";

            //string question = "How can i learn more about microsoft certifications";

            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);
            Console.WriteLine("Ask a question, type exit to quit.");





            while (true)
            {
                Console.WriteLine("Q: ");
                string question = Console.ReadLine();
                if (question.ToLower() == "exit")
                {
                    break;
                }
                try
                {

                    Response<AnswersResult> response = client.GetAnswers(question, project);
                    foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                    {
                        Console.WriteLine($"Q:{question}");
                        Console.WriteLine($"A:{answer.Answer}");
                        string inputText = answer.Answer.ToString();

                        string language = await GetLanguage(inputText);
                        Console.WriteLine("Language: " + language);

                        // Translate if not already English
                        if (language != "sv")
                        {
                            string translatedText = await Translate(inputText, language);
                            Console.WriteLine("\nTranslation:\n" + translatedText);
                        }


                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Request error: {ex.Message}");
                }
            }
        }

        static async Task<string> GetLanguage(string text)
        {
            // Default language is English
            string language = "sv";

            // Use the Translator detect function
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    // Build the request
                    string path = "/detect?api-version=3.0";
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(translatorEndpoint + path);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", cogSvcKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", cogSvcRegion);

                    // Send the request and get response
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Parse JSON array and get language
                    JArray jsonResponse = JArray.Parse(responseContent);
                    language = (string)jsonResponse[0]["language"];
                }
            }





            // return the language
            return language;
        }

        static async Task<string> Translate(string text, string sourceLanguage)
        {
            string translation = "";

            // Use the Translator translate function
            // Use the Translator translate function
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    // Build the request
                    string path = "/translate?api-version=3.0&from=" + sourceLanguage + "&to=sv";
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(translatorEndpoint + path);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", cogSvcKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", cogSvcRegion);

                    // Send the request and get response
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Parse JSON array and get translation
                    JArray jsonResponse = JArray.Parse(responseContent);
                    translation = (string)jsonResponse[0]["translations"][0]["text"];
                }
            }


            // Return the translation
            return translation;

        }

    }
}
