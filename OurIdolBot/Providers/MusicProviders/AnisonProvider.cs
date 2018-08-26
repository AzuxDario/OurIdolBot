using Newtonsoft.Json;
using OurIdolBot.Const.MusicConst;
using OurIdolBot.Containers.MusicContainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OurIdolBot.Providers.MusicProviders
{
    class AnisonProvider
    {
        public static async Task<string> GetAnisonSongInfo()
        {
            string currentAnisonPlayingSong = "";
            string json = "";
            try
            {
                var client = new WebClient();
                NowPlayingAnisonContainer nowPlayingContainer;

                json = client.DownloadString(RadiosLinksDataConst.AnisonFm);
                if (json.Length > 0)
                {
                    nowPlayingContainer = JsonConvert.DeserializeObject<NowPlayingAnisonContainer>(json);

                    currentAnisonPlayingSong = ClearAnisonString(nowPlayingContainer.On_air);

                    if (currentAnisonPlayingSong == string.Empty)
                    {
                        currentAnisonPlayingSong = "I couldn't get song name";
                    }
                }
                else
                {
                    currentAnisonPlayingSong = "I couldn't get song name";
                }
            }
            catch (Exception ie)
            {
                // Something went wrong
                Console.WriteLine("Error: I couldn't get song name or error with Anison web site appeared or error with parsing.");
                currentAnisonPlayingSong = "I couldn't get song name";
                Console.WriteLine("Exception: " + ie.Message);
                Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                Console.WriteLine("Stack trace: " + ie.StackTrace);
                using (var streamWriter = new StreamWriter("jsons.txt", true, Encoding.UTF8))
                {
                    streamWriter.WriteLine("Error: I couldn't get song name or error with Anison web site appeared or error with parsing.");
                    streamWriter.WriteLine("Exception: " + ie.Message);
                    streamWriter.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                    streamWriter.WriteLine("Stack trace: " + ie.StackTrace);
                    streamWriter.WriteLine("JSON: " + json);
                }
            }

            return currentAnisonPlayingSong;
        }

        private static string ClearAnisonString(string input)
        {
            string temp = Regex.Replace(input, "<.*?>", String.Empty);
            temp = temp.Replace("В эфире: ", String.Empty);
            temp = temp.Replace("&#151;", "-");

            return temp;
        }
    }
}
