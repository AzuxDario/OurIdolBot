using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Commands.MusicCommands
{
    class NowPlayingCommands
    {

        private List<EnabledServer> enabledChannels;

        [Command("enableNowPlaying")]
        [Description("Example ping command")]
        [Aliases("enableNP")]
        public async Task EmableNowPlaying(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var currentChannel = enabledChannels.FirstOrDefault(p => p.channelId == ctx.Channel.Id);
            if (currentChannel == null)
            {
                currentChannel = new EnabledServer(ctx.Channel.Id);
                enabledChannels.Add(currentChannel);
            }

            await ctx.RespondAsync("Now playing...");
        }
    }

    class EnabledServer
    {
        public ulong channelId;
        public ulong lastMessageID;

        public EnabledServer(ulong channelId)
        {
            this.channelId = channelId;
        }
    }

}
