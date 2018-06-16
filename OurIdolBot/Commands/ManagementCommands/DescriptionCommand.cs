using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OurIdolBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OurIdolBot.Commands.ManagementCommands
{
    class DescriptionCommand
    {
        private Timer refreshDescriptionTimer;
        private int refreshDescriptionInterval;

        private string game;

        public DescriptionCommand()
        {
            refreshDescriptionInterval = 1000 * 60;    // every 1 minute
            refreshDescriptionTimer = new Timer(RefreshDescriptionCallback, null, refreshDescriptionInterval, Timeout.Infinite);

            game = string.Empty;
        }

        [Command("description")]
        [Description("Change bot descrition.")]
        public async Task Description(CommandContext ctx, [Description("New description.")] string description = null)
        {
            if (ctx.Member.Id == Bot.configJson.Developer)
            {
                game = description;

                await ctx.Client.UpdateStatusAsync(new DiscordGame(description));
            }
        }

        private void RefreshDescriptionCallback(object state)
        {
            if (game != string.Empty)
            {
                Bot.DiscordClient.UpdateStatusAsync(new DiscordGame(game));
            }

            refreshDescriptionTimer.Change(refreshDescriptionInterval, Timeout.Infinite);
        }
    }
}
