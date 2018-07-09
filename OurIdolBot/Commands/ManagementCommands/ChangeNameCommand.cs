using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using OurIdolBot.Attributes;
using OurIdolBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Commands.ManagementCommands
{
    [CommandsGroup("Management")]
    class ChangeNameCommand
    {
        [Command("changeName")]
        [Description("Change bot descrition.")]
        public async Task ChangeName(CommandContext ctx, [Description("New name.")] string name = null)
        {
            if (ctx.Member.Id == Bot.configJson.Developer)
            {
                await Bot.DiscordClient.EditCurrentUserAsync(name);
            }
        }
    }
}
