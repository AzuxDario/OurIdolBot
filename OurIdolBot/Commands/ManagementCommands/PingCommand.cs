using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace OurIdolBot.Commands.OtherCommands
{
    public class PingCommand
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
