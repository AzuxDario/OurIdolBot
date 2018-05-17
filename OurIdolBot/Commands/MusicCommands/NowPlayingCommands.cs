using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        [Command("enableNowPlaying")]
        [Description("Enable auto generated messages about current playing song.")]
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
            }
            // If we have current song, don't post info about it
            if(currentPlayingSong != string.Empty)
            {
                RepostSongInfo(currentChannel);
            }
        }

        [Command("disableNowPlaying")]
        [Description("Disable auto generated messages about current playing song.")]
        [Aliases("disableNP")]
        public async Task DisableNowPlaying(CommandContext ctx)
        {
            // Remove channel from list
            enabledChannels.RemoveAll(p => p.discordChannel.Id == ctx.Channel.Id);
        }

        private async void RefreshCurrentSongMessages(object state)
        {
            //download current song
            currentPlayingSong = "Chika";
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

        [Command("np")]
        [Description("Check current playing song.")]
        [Aliases("nowPlaying")]
        public async Task NowPlaying(CommandContext ctx)
        {
            // Create info about song
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#5588EE")
            };
            embed.AddField("Radio Anison FM", "Current playing: " + currentPlayingSong);
            // Post info about song
            await ctx.RespondAsync("", false, embed);
        }

        private async void PostSongInfo(EnabledChannel channel)
        {
            // Create info about song
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#5588EE")
            };
            embed.AddField("Radio Anison FM", "Current playing: " + currentPlayingSong);
            // Post info about song
            channel.lastMessage = await channel.discordChannel.SendMessageAsync("", false, embed);
        }

        private async void RepostSongInfo(EnabledChannel channel)
        {
            // Try delete last message
            try
            {
                await channel.discordChannel.DeleteMessageAsync(channel.lastMessage);
            }
            catch (Exception ie)
            {
                //Bot couldn't find message. Maybe someone deleted it.
            }
            PostSongInfo(channel);

        }

        private async void GetSongInfo()
        {

        }

        private string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
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
