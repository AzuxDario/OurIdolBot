﻿using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Helpers
{
    static class EmbedHelper
    {
        public static async Task<DiscordEmbed> CreateEmbed(CommandContext ctx, string title = null, string description = null, string imageLink = null, string footerText = null)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#00a8ff"),
                ImageUrl = imageLink,
                Description = description,
                Title = title
            };
            if (footerText != null)
            {
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter();
                embed.Footer.Text = footerText;
            }
            return embed;
            await ctx.RespondAsync(null, false, embed);
        }
    }
}
