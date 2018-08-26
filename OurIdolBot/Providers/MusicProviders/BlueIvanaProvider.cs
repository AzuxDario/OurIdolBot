using Newtonsoft.Json;
using OurIdolBot.Const.MusicConst;
using OurIdolBot.Containers.MusicContainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Providers.MusicProviders
{
    class BlueIvanaProvider
    {
        public static async Task<string> GetBlueIvanaSongInfo(HttpClientHandler httpClientHandler)
        {
            string currentBlueIvanaPlayingSong = "";
            string json = "";
            HttpResponseMessage response;
            try
            {
                var client = new HttpClient(httpClientHandler);
                NowPlayingBlueIvanaContainer nowPlayingContainer;

                response = await client.PostAsync(RadiosLinksDataConst.BlueIvana,
                    new StringContent(JsonConvert.SerializeObject(new NowPlayingBlueIvanaConst()), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync();
                    json = ClearBlueIvanaString(json);
                    nowPlayingContainer = JsonConvert.DeserializeObject<NowPlayingBlueIvanaContainer>(json);

                    currentBlueIvanaPlayingSong = nowPlayingContainer.Artist + " - " + nowPlayingContainer.Title;

                    if (currentBlueIvanaPlayingSong == string.Empty)
                    {
                        currentBlueIvanaPlayingSong = "I couldn't get song name";
                    }
                }
                else
                {
                    currentBlueIvanaPlayingSong = "I couldn't get song name";
                }
            }
            catch (Exception ie)
            {
                // Something went wrong
                Console.WriteLine("Error: I couldn't get song name or error with Blue Ivana web site appeared or error with parsing.");
                currentBlueIvanaPlayingSong = "I couldn't get song name";
                Console.WriteLine("Exception: " + ie.Message);
                Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                Console.WriteLine("Stack trace: " + ie.StackTrace);
                using (var streamWriter = new StreamWriter("jsons.txt", true, Encoding.UTF8))
                {
                    streamWriter.WriteLine("Error: I couldn't get song name or error with Blue Ivana web site appeared or error with parsing.");
                    streamWriter.WriteLine("Exception: " + ie.Message);
                    streamWriter.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                    streamWriter.WriteLine("Stack trace: " + ie.StackTrace);
                    streamWriter.WriteLine("JSON: " + json);
                }
            }

            return currentBlueIvanaPlayingSong;
        }

        public static async void GetBlueIvanaCookies(HttpClientHandler httpClientHandler)
        {
            HttpResponseMessage response;
            try
            {
                var client = new HttpClient(httpClientHandler);

                response = await client.PostAsync(RadiosLinksDataConst.BlueIvana,
                    new StringContent(JsonConvert.SerializeObject(new NowPlayingBlueIvanaConst()), Encoding.UTF8, "application/json"));
            }
            catch (Exception ie)
            {
                // Something went wrong
                Console.WriteLine("Error: I couldn't get cookies name or error with Blue Ivana web site appeared or error with parsing.");
                Console.WriteLine("Exception: " + ie.Message);
                Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                Console.WriteLine("Stack trace: " + ie.StackTrace);
            }
        }

        private static string ClearBlueIvanaString(string input)
        {
            string temp = input.Replace("\\\"", "\"");
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);

            return temp;
        }
    }
}
