﻿using DSharpPlus.CommandsNext;
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

        [Command("np")]
        [Description("Check current playing song.")]
        [Aliases("nowPlaying")]
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
            }
            // Try delete last message
            try
            {
                await channel.discordChannel.DeleteMessageAsync(channel.lastMessage);
            }
            catch (Exception ie)
            {
                //Bot couldn't find message. Maybe someone deleted it.
            }
            PostNewSongInfo(channel);
        }

        private DiscordEmbed CreateEmbed()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#5588EE")
            };
            embed.AddField("Radio Anison FM", "Current playing: " + currentPlayingSong);
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
                currentPlayingSong = "I couldn't get song name";
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
