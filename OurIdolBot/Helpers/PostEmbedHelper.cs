using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OurIdolBot.Helpers
{
    class PostEmbedHelper
    {
        public static async Task PostEmbed(CommandContext ctx, string title = null, string description = null, string imageLink = null, string thumbnailLink = null, DiscordEmbedBuilder.EmbedFooter footer = null,
            string color = null)
        {
            // Discord can't handle links with japanese characters
            if (imageLink != null)
            {
                int pos = imageLink.LastIndexOf("/");
                string toChange = imageLink.Substring(pos + 1);
                string changed = HttpUtility.UrlEncode(toChange, Encoding.UTF8);
                imageLink = imageLink.Remove(pos + 1);
                imageLink += changed;
            }
            if (thumbnailLink != null)
            {
                int posThumbnail = thumbnailLink.LastIndexOf("/");
                string toChangeThumbnail = thumbnailLink.Substring(posThumbnail + 1);
                string changedThumbnail = HttpUtility.UrlEncode(toChangeThumbnail, Encoding.UTF8);
                thumbnailLink = thumbnailLink.Remove(posThumbnail + 1);
                thumbnailLink += changedThumbnail;
            }



            var embed = new DiscordEmbedBuilder
            {
                ImageUrl = imageLink,
                Description = description,
                Title = title,
                Footer = footer
            };

            if (color == null)
            {
                embed.Color = new DiscordColor("#00a8ff");
            }
            else
            {
                embed.Color = new DiscordColor(color);
            }

            if (thumbnailLink != null)
            {
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail();
                embed.Thumbnail.Url = thumbnailLink;
            }

            await ctx.RespondAsync(embed);
        }
    }
}
