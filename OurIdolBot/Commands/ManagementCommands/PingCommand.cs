using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using OurIdolBot.Attributes;

namespace OurIdolBot.Commands.OtherCommands
{
    [CommandsGroup("Management")]
    public class PingCommand : BaseCommandModule
    {
        [Command("ping")]
        [Description("Show ping.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync($"{ctx.Client.Ping} ms");
        }
    }
}
