﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OurIdolBot.Attributes;
using OurIdolBot.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Commands.PicturesCommand
{
    [CommandsGroup("Pictures")]
    class AvatarCommand : BaseCommandModule
    {
        [Command("avatar")]
        [Description("Shows user's or other user's avatar.")]
        public async Task Avatar(CommandContext ctx, [Description("If you want other user's avatar, put his mention here.")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();

            if (member == null)
            {
                var embed = EmbedHelper.CreateEmbed(new DiscordColor("#00a8ff"), "Avatar", ctx.User.Mention, ctx.User.AvatarUrl);
                await ctx.RespondAsync(embed);
            }
            else
            {
                var embed = EmbedHelper.CreateEmbed(new DiscordColor("#00a8ff"), "Avatar", member.Mention, member.AvatarUrl);
                await ctx.RespondAsync(embed);
            }
        }
    }
}
