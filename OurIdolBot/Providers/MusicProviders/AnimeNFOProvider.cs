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
    class AnimeNFOProvider
    {
        public async Task<string> GetAnimeNFOSongInfo()
        {
            string currentAnimeNFOPlayingSong = "";
            string site = "";
            try
            {
                var client = new WebClient();
                
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.92 Safari/537.36");
                site = client.DownloadString(RadiosLinksDataConst.AnimeNFO);
                if (site.Length > 0)
                {
                    currentAnimeNFOPlayingSong = ClearAnimeNFOString(site);

                    if (currentAnimeNFOPlayingSong == string.Empty)
                    {
                        currentAnimeNFOPlayingSong = "I couldn't get song name";
                    }
                }
                else
                {
                    currentAnimeNFOPlayingSong = "I couldn't get song name";
                }
            }
            catch (Exception ie)
            {
                // Something went wrong
                Console.WriteLine("Error: I couldn't get song name or error with AnimeNFO web site appeared or error with parsing.");
                currentAnimeNFOPlayingSong = "I couldn't get song name";
                Console.WriteLine("Exception: " + ie.Message);
                Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                Console.WriteLine("Stack trace: " + ie.StackTrace);
                using (var streamWriter = new StreamWriter("jsons.txt", true, Encoding.UTF8))
                {
                    streamWriter.WriteLine("Error: I couldn't get song name or error with AnimeNFO web site appeared or error with parsing.");
                    streamWriter.WriteLine("Exception: " + ie.Message);
                    streamWriter.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                    streamWriter.WriteLine("Stack trace: " + ie.StackTrace);
                    streamWriter.WriteLine("JSON: " + site);
                }
            }
            return currentAnimeNFOPlayingSong;
        }

        private string ClearAnimeNFOString(string input)
        {
            Regex regex = new Regex("<td>Playing Now: </td><td><b><a href=\"currentsong\\?sid=1\">(.*)</a></b></td>");
            Match match = regex.Match(input);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
    }
}
