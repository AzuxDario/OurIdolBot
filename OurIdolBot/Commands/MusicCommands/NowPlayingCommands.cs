using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OurIdolBot.Attributes;
using OurIdolBot.Const.MusicConst;
using OurIdolBot.Providers.MusicProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OurIdolBot.Commands.MusicCommands
{
    [CommandsGroup("Music")]
    class NowPlayingCommands : BaseCommandModule
    {
        
        private List<EnabledChannel> enabledChannels;
        private string currentAnisonPlayingSong;
        private string currentJMusicPlayingSong;
        private string currentBlueIvanaPlayingSong;
        private string currentAnimeNFOPlayingSong;

        CookieContainer cookies;
        HttpClientHandler httpClientHandler;

        private Timer refreshCurrentSongTimer;
        private int refreshCurrentSongInterval;

        private AnisonProvider anisonProvider;
        private BlueIvanaProvider blueIvanaProvder;
        private JMusicProvider jmusicProvider;
        private AnimeNFOProvider animeNFOProvider;

        public NowPlayingCommands()
        {
            anisonProvider = new AnisonProvider();
            blueIvanaProvder = new BlueIvanaProvider();
            jmusicProvider = new JMusicProvider();
            animeNFOProvider = new AnimeNFOProvider();

            refreshCurrentSongInterval = 1000 * 15;    // every 15 seconds
            refreshCurrentSongTimer = new Timer(RefreshCurrentSongMessages, null, refreshCurrentSongInterval, Timeout.Infinite);
            enabledChannels = new List<EnabledChannel>();
            currentAnisonPlayingSong = "Waiting for first check...";
            currentJMusicPlayingSong = "Waiting for first check...";
            currentBlueIvanaPlayingSong = "Waiting for first check...";
            currentAnimeNFOPlayingSong = "Waiting for first check...";

            cookies = new CookieContainer();
            httpClientHandler = new HttpClientHandler();
            httpClientHandler.CookieContainer = cookies;

            blueIvanaProvder.GetBlueIvanaCookies(httpClientHandler);
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
                await ctx.RespondAsync("I will inform on this channel about the current songs playing every 15 seconds.");
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

        [Command("countUpdatingChannels")]
        [Description("Shows on how much channels idols updating information.")]
        public async Task CountUpdatingChannels(CommandContext ctx)
        {
            await ctx.RespondAsync("Number of channels: " + enabledChannels.Count);
        }

        private async void RefreshCurrentSongMessages(object state)
        {
            try
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
            }
            catch (Exception ie)
            {

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
              
            // If last message exist
            if (channel.lastMessage != null)
            {
                // Try find last message
                try
                {
                    var message = await channel.discordChannel.GetMessagesAsync(1);
                    // If last message on channel is the last message posted by bot, edit it
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
                            Console.WriteLine("Error: Edit last message failed.");
                            Console.WriteLine("Exception: " + ie.Message);
                            Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                            Console.WriteLine("Stack trace: " + ie.StackTrace);
                        }              
                    }
                }
                catch (Exception ie)
                {
                    // Something went wrong
                    Console.WriteLine("Error: Get last message from channel failed.");
                    Console.WriteLine("Exception: " + ie.Message);
                    Console.WriteLine("Inner Exception: " + ie?.InnerException?.Message);
                    Console.WriteLine("Stack trace: " + ie.StackTrace);
                }
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
                Console.WriteLine("Error: Delete bot's last message failed. Message might be already deleted by someone else.");
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
                Console.WriteLine("Error: Send music info failed.");
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
            embed.AddField("Current playing songs", "**Radio Anison FM**\n" + currentAnisonPlayingSong + "\n" + RadiosLinksConst.AnisonFm + 
                "\n\n**Radio Blue Anime Ivana**\n" + currentBlueIvanaPlayingSong + "\n" + RadiosLinksConst.BlueIvana +
                "\n\n**J-Pop Project Radio (JMusic)**\n" + currentJMusicPlayingSong + "\n" + RadiosLinksConst.JMusic +
                "\n\n**Anime NFO Radio**\n" + currentAnimeNFOPlayingSong + "\n" + RadiosLinksConst.AnimeNFO +
                "\n\nLast update: " + DateTime.UtcNow.ToString(@"HH:mm:ss") + " UTC");
            return embed;
        }

        private async void GetSongInfo()
        {
            currentAnisonPlayingSong = await anisonProvider.GetAnisonSongInfo();
            currentJMusicPlayingSong = await jmusicProvider.GetJMusicSongInfo();
            currentBlueIvanaPlayingSong = await blueIvanaProvder.GetBlueIvanaSongInfo(httpClientHandler);
            currentAnimeNFOPlayingSong = await animeNFOProvider.GetAnimeNFOSongInfo();
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
