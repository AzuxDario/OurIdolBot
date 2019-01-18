using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using OurIdolBot.Attributes;
using OurIdolBot.Containers.PicturesContainers;
using OurIdolBot.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OurIdolBot.Const.PicturesConst;
using DSharpPlus.CommandsNext.Attributes;

namespace OurIdolBot.Commands.PicturesCommand
{
    [CommandsGroup("Pictures")]
    public class NekosLifeImage : BaseCommandModule
    {
        private const string footerText = "Powered by nekos.life";

        [Command("catgirl")]
        public async Task CatGirl(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Neko, "Cat girl", member);
        }
        [Command("kiss")]
        public async Task Kiss(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Kiss, "Kiss", member);
        }
        [Command("hug")]
        public async Task Hug(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Hug, "Hug", member);
        }
        [Command("pat")]
        public async Task Pat(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Pat, "Pat", member);
        }
        [Command("cuddle")]
        public async Task Cuddle(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Cuddle, "Cuddle", member);
        }
        [Command("lizard")]
        public async Task Lizard(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Lizard, "Lizard", member);
        }
        [Command("tickle")]
        public async Task Tickle(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Tickle, "Tickle", member);
        }
        [Command("neko")]
        [Aliases("meow")]
        public async Task Neko(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Meow, "Neko", member);
        }
        [Command("poke")]
        public async Task Poke(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Poke, "Poke", member);
        }
        [Command("8ball")]
        public async Task EightBall(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.EightBall, "8ball", member);
        }
        [Command("slap")]
        public async Task Slap(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Slap, "Slap", member);
        }
        [Command("avatar")]
        public async Task Avatar(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Avatar, "Avatar", member);
        }
        [Command("kitsune")]
        [Aliases("foxgirl")]
        public async Task Kitsune(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.FoxGirl, "Kitsune", member);
        }
        [Command("smug")]
        public async Task Smug(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Smug, "Smug", member);
        }
        [Command("wallpaper")]
        public async Task Wallpaper(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Wallpaper, "Wallpaper", member);
        }
        [Command("inu")]
        [Aliases("woof")]
        public async Task Inu(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Woof, "Inu", member);
        }
        [Command("baka")]
        public async Task Baka(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Baka, "Baka", member);
        }
        [Command("feed")]
        public async Task Feed(CommandContext ctx, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();
            await SendImage(ctx, NekosLifePicturesEndpoints.Feed, "Feed", member);
        }

        public async Task SendImage(CommandContext ctx, string endpoint, string title, [Description("Mention")] DiscordMember member = null)
        {
            await ctx.TriggerTypingAsync();

            var client = new WebClient();
            var url = client.DownloadString(endpoint);
            var pictureContainer = JsonConvert.DeserializeObject<NekosFileImage>(url);

            await PostEmbedHelper.PostEmbed(ctx, title, member?.Mention, pictureContainer.Url, footerText);
        }

        public string GetExtension(string url)
        {
            var array = url.Split('.');
            return array[array.Length - 1];
        }
    }
}
