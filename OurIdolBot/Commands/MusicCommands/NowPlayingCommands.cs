using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using OurIdolBot.Attributes;
using OurIdolBot.Const.MusicConst;
using OurIdolBot.Containers.MusicContainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OurIdolBot.Commands.MusicCommands
{
    [CommandsGroup("Music")]
    class NowPlayingCommands
    {

        private List<EnabledChannel> enabledChannels;
        private string currentAnisonPlayingSong;
        private string currentJMusicPlayingSong;
        private string currentBlueIvanaPlayingSong;
        
        CookieContainer cookies;
        HttpClientHandler httpClientHandler;

        private Timer refreshCurrentSongTimer;
        private int refreshCurrentSongInterval;

        public NowPlayingCommands()
        {
            refreshCurrentSongInterval = 1000 * 15;    // every 15 seconds
            refreshCurrentSongTimer = new Timer(RefreshCurrentSongMessages, null, refreshCurrentSongInterval, Timeout.Infinite);
            enabledChannels = new List<EnabledChannel>();
            currentAnisonPlayingSong = "Waiting for first check...";
            currentJMusicPlayingSong = "Waiting for first check...";
            currentBlueIvanaPlayingSong = "Waiting for first check...";

            cookies = new CookieContainer();
            httpClientHandler = new HttpClientHandler();
            httpClientHandler.CookieContainer = cookies;

            GetSongInfo();
        }

        [Command("enableNowPlaying")]
        [Description("Enable auto generated messages about current playing song every 15 seconds.")]
        [Aliases("enableNP")]
        public async Task EnableNowPlaying(CommandContext ctx)
        {
            // Get current channel where command is used
            var currentChannel = enabledChannels.FirstOrDefault(p => p.discordChannel.Id == ctx.Channel.Id);
            // If this channel doesn't exist add it
            if (currentChannel == null)
            {
                currentChannel = new EnabledChannel(ctx.Channel);
                enabledChannels.Add(currentChannel);
                await ctx.RespondAsync("I will inform on this channel about the current song playing by AnisonFM every 15 seconds.");
                // If we don't have current song, don't post info about it
                if (currentAnisonPlayingSong != string.Empty)
                {
                    RepostSongInfo(currentChannel);
                }
            }
            else
            {
                await ctx.RespondAsync("I'm already posting info on this channel.");
            }
            
        }

        [Command("disableNowPlaying")]
        [Description("Disable auto generated messages about current playing song.")]
        [Aliases("disableNP")]
        public async Task DisableNowPlaying(CommandContext ctx)
        {
            // Remove channel from list
            var howManyRemoved = enabledChannels.RemoveAll(p => p.discordChannel.Id == ctx.Channel.Id);
            if (howManyRemoved > 0)
            {
                await ctx.RespondAsync("Auto inform for this channel has been turned off.");
            }
            else
            {
                await ctx.RespondAsync("Auto inform for this channel has not been enabled.");
            }
        }

        [Command("isEnabled")]
        [Description("Shows whether auto generated messages are enabled on this channel.")]
        public async Task IsEnable(CommandContext ctx)
        {
            // Search channel in list
            var channels = enabledChannels.FirstOrDefault(p => p.discordChannel.Id == ctx.Channel.Id);
            if (channels != null)
            {
                await ctx.RespondAsync("Auto inform is enabled on this channel.");
            }
            else
            {
                await ctx.RespondAsync("Auto inform is disabled on this channel.");
            }
        }

        private async void RefreshCurrentSongMessages(object state)
        {
            //download current song
            GetSongInfo();
            // If we have at least one channel repost current song
            if (enabledChannels.Count > 0)
            {
                foreach (var channel in enabledChannels)
                {
                    RepostSongInfo(channel);
                }
            }

            refreshCurrentSongTimer.Change(refreshCurrentSongInterval, Timeout.Infinite);
        }

        [Command("nowPlaying")]
        [Description("Check current playing song.")]
        [Aliases("np")]
        public async Task NowPlaying(CommandContext ctx)
        {
            // Post info about song
            await ctx.RespondAsync("", false, CreateEmbedWithSongData());
        }

        private async void RepostSongInfo(EnabledChannel channel)
        {
            // Try find last message
            try
            {
                var message = await channel.discordChannel.GetMessagesAsync(1);
                // If last messgae exist
                if (channel.lastMessage != null)
                {
                    // If last message on channel is last message bot posted, edit it
                    if (message.FirstOrDefault() == channel.lastMessage)
                    {
                        try
                        {
                            await channel.lastMessage.ModifyAsync("", CreateEmbedWithSongData());
                            return;
                        }
                        catch(Exception ie)
                        {
                            // Something went wrong
                            Console.WriteLine("Error: Somehow I couldn't edit last my last message, even if it was last.");
                            Console.WriteLine("Exception: " + ie.Message);
                            Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                            Console.WriteLine("Stack trace: " + ie.StackTrace);
                        }              
                    }
                }
            }
            catch (Exception ie)
            {
                // Something went wrong
                Console.WriteLine("Error: Somehow I couldn't edit last my last message, even if it was last or I haven't post last message yet.");
                Console.WriteLine("Exception: " + ie.Message);
                Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                Console.WriteLine("Stack trace: " + ie.StackTrace);
            }
            // Try delete last message
            try
            {
                if (channel.lastMessage != null)
                {
                    await channel.discordChannel.DeleteMessageAsync(channel.lastMessage);
                }
            }
            catch (Exception ie)
            {
                //Bot couldn't find message. Maybe someone deleted it.
                Console.WriteLine("Error: Somehow I couldn't find message. Maybe someone deleted it.");
                Console.WriteLine("Exception: " + ie.Message);
                Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                Console.WriteLine("Stack trace: " + ie.StackTrace);
            }
            try
            {
                channel.lastMessage = await channel.discordChannel.SendMessageAsync("", false, CreateEmbedWithSongData());
            }
            catch (Exception ie)
            {
                await channel.discordChannel.SendMessageAsync("Something went wrong.");
                Console.WriteLine("Error: Somehow I couldn't post current song info.");
                Console.WriteLine("Exception: " + ie.Message);
                Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                Console.WriteLine("Stack trace: " + ie.StackTrace);
            }
        }

        private DiscordEmbed CreateEmbedWithSongData()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#5588EE")
            };
            embed.AddField("Current playing songs", "**Radio Anison FM**\n" + currentAnisonPlayingSong +
                "\n\n**Radio Blue Anime Ivana**\n" + currentBlueIvanaPlayingSong +
                "\n\n**Radio JMusic**\n" + currentJMusicPlayingSong +
                "\n\nLast update: " + DateTime.UtcNow.ToString(@"HH:mm:ss") + " UTC");
            return embed;
        }

        private async void GetSongInfo()
        {
            GetAnisonSongInfo();
            GetJMusicSongInfo();
            GetBlueIvanaSongInfo();
        }

        private async void GetAnisonSongInfo()
        {
            string json = "";
            try
            {
                var client = new WebClient();
                NowPlayingAnisonContainer nowPlayingContainer;

                json = client.DownloadString("http://anison.fm/status.php?widget=true");
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
        }

        private async void GetJMusicSongInfo()
        {
            string json = "";
            try
            {
                var client = new WebClient();
                NowPlayingJMusicContainer nowPlayingContainer;

                json = client.DownloadString("http://player.abovecast.com/streamdata.php?h=agnes.torontocast.com&p=8083&i=&f=v2&c=");
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
                Console.WriteLine("Error: I couldn't get song name or error with Anison web site appeared or error with parsing.");
                currentJMusicPlayingSong = "I couldn't get song name";
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
        }

        private async void GetBlueIvanaSongInfo()
        {
            string json = "";
            HttpResponseMessage response;
            try
            {
                var client = new HttpClient(httpClientHandler);
                NowPlayingBlueIvanaContainer nowPlayingContainer;

                response = await client.PostAsync("https://www.radionomy.com/en/OnAir/GetCurrentSongPlayer",
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
                Console.WriteLine("Error: I couldn't get song name or error with Anison web site appeared or error with parsing.");
                currentBlueIvanaPlayingSong = "I couldn't get song name";
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
        }

        private async void GetBlueIvanaCookies()
        {
            string json = "";
            HttpResponseMessage response;
            try
            {
                var client = new HttpClient(httpClientHandler);
                NowPlayingBlueIvanaContainer nowPlayingContainer;

                response = await client.PostAsync("https://www.radionomy.com/en/OnAir/GetCurrentSongPlayer",
                    new StringContent(JsonConvert.SerializeObject(new NowPlayingBlueIvanaConst()), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync();
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
                Console.WriteLine("Error: I couldn't get song name or error with Anison web site appeared or error with parsing.");
                currentBlueIvanaPlayingSong = "I couldn't get song name";
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
        }

        private string ClearAnisonString(string input)
        {
            string temp = Regex.Replace(input, "<.*?>", String.Empty);
            temp = temp.Replace("В эфире: ", String.Empty);
            temp = temp.Replace("&#151;", "—");

            return temp;
        }

        private string ClearBlueIvanaString(string input)
        {
            string temp = input.Replace("\\\"", "\"");
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);

            return temp;
        }
    }

    class EnabledChannel
    {
        public DiscordChannel discordChannel;
        public DiscordMessage lastMessage;

        public EnabledChannel(DiscordChannel channel)
        {
            this.discordChannel = channel;
        }
    }

}
