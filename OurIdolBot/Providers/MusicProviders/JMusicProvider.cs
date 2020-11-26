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
    class JMusicProvider
    {
        public async Task<string> GetJMusicSongInfo()
        {
            string currentJMusicPlayingSong = "";
            string site = "";
            try
            {
                var client = new WebClient();
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.92 Safari/537.36");
                site = client.DownloadString(RadiosLinksDataConst.JMusic);
                if (site.Length > 0)
                {
                    currentJMusicPlayingSong = ClearJMusicString(site);

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
            }
            return currentJMusicPlayingSong;
        }

        private string ClearJMusicString(string input)
        {
            Regex regex = new Regex("Playing Now: </td><td><b><a href=\"currentsong\\?sid=1\">(.*)</a></b></td></tr>");
            Match match = regex.Match(input);
            if (match.Success)
            {
                return WebUtility.HtmlDecode(match.Groups[1].Value);
            }
            return string.Empty;
        }
    }
}
