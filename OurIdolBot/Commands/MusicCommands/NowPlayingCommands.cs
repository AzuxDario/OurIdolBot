using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using OurIdolBot.Containers.MusicContainers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OurIdolBot.Commands.MusicCommands
{
    class NowPlayingCommands
    {

        private List<EnabledChannel> enabledChannels;
        private string currentPlayingSong;

        private Timer refreshCurrentSongTimer;
        private int refreshCurrentSongInterval;

        public NowPlayingCommands()
        {
            refreshCurrentSongInterval = 1000 * 15;    // every 15 seconds
            refreshCurrentSongTimer = new Timer(RefreshCurrentSongMessages, null, refreshCurrentSongInterval, Timeout.Infinite);
            enabledChannels = new List<EnabledChannel>();
            currentPlayingSong = "Waiting for first check...";
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
                if (currentPlayingSong != string.Empty)
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
            await ctx.RespondAsync("", false, CreateEmbed());
        }

        private async void PostNewSongInfo(EnabledChannel channel)
        {
            // Post new info about song
            channel.lastMessage = await channel.discordChannel.SendMessageAsync("", false, CreateEmbed());
        }

        private async void RepostSongInfo(EnabledChannel channel)
        {
            // Try find last message
            try
            {
                var message = await channel.discordChannel.GetMessagesAsync(1);
                // If last message found, edit it
                if(message.FirstOrDefault() == channel.lastMessage)
                {
                    await channel.lastMessage.ModifyAsync("", CreateEmbed());
                    return;
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
                await channel.discordChannel.DeleteMessageAsync(channel.lastMessage);
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
                PostNewSongInfo(channel);
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

        private DiscordEmbed CreateEmbed()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#5588EE")
            };
            embed.AddField("Radio Anison FM", "Current song: " + currentPlayingSong + "\nLast update: " + DateTime.UtcNow.ToString(@"HH:mm:ss") + " UTC");
            return embed;
        }

        private async void GetSongInfo()
        {
            try
            {
                var client = new WebClient();
                NowPlayingContainer nowPlayingContainer;

                var json = client.DownloadString("http://anison.fm/status.php?widget=true");
                nowPlayingContainer  = JsonConvert.DeserializeObject<NowPlayingContainer>(json);

                currentPlayingSong = ClearString(nowPlayingContainer.On_air);

                if(currentPlayingSong == string.Empty)
                {
                    currentPlayingSong = "I couldn't get song name";
                }
            }
            catch (Exception ie)
            {
                // Something went wrong
                Console.WriteLine("Error: I couldn't get song name or error with Anison web site appeared or error with parsing.");
                currentPlayingSong = "I couldn't get song name";
                Console.WriteLine("Exception: " + ie.Message);
                Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                Console.WriteLine("Stack trace: " + ie.StackTrace);
            }
        }

        private string ClearString(string input)
        {
            string temp = Regex.Replace(input, "<.*?>", String.Empty);
            temp = temp.Replace("В эфире: ", String.Empty);
            temp = temp.Replace("&#151;", "—");

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
