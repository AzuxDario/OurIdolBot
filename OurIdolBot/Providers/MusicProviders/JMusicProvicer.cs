using Newtonsoft.Json;
using OurIdolBot.Const.MusicConst;
using OurIdolBot.Containers.MusicContainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Providers.MusicProviders
{
    class JMusicProvicer
    {
        public static async Task<string> GetJMusicSongInfo()
        {
            string currentJMusicPlayingSong = "";
            string json = "";
            try
            {
                var client = new WebClient();
                NowPlayingJMusicContainer nowPlayingContainer;

                json = client.DownloadString(RadiosLinksDataConst.JMusic);
                if (json.Length > 0)
                {
                    nowPlayingContainer = JsonConvert.DeserializeObject<NowPlayingJMusicContainer>(json);

                    currentJMusicPlayingSong = nowPlayingContainer.Song;

                    if (currentJMusicPlayingSong == string.Empty)
                    {
                        currentJMusicPlayingSong = "I couldn't get song name";
                    }
                }
                else
                {
                    currentJMusicPlayingSong = "I couldn't get song name";
                }
            }
            catch (Exception ie)
            {
                // Something went wrong
                Console.WriteLine("Error: I couldn't get song name or error with JMusic web site appeared or error with parsing.");
                currentJMusicPlayingSong = "I couldn't get song name";
                Console.WriteLine("Exception: " + ie.Message);
                Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                Console.WriteLine("Stack trace: " + ie.StackTrace);
                using (var streamWriter = new StreamWriter("jsons.txt", true, Encoding.UTF8))
                {
                    streamWriter.WriteLine("Error: I couldn't get song name or error with JMusic web site appeared or error with parsing.");
                    streamWriter.WriteLine("Exception: " + ie.Message);
                    streamWriter.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                    streamWriter.WriteLine("Stack trace: " + ie.StackTrace);
                    streamWriter.WriteLine("JSON: " + json);
                }
            }
            return currentJMusicPlayingSong;
        }
    }
}
