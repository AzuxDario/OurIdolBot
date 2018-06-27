using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using OurIdolBot.Attributes;
using System.Globalization;

namespace OurIdolBot.Commands.ConversionCommans.cs
{
    [CommandsGroup("Conversion")]
    class ConversionCommands
    {
        [Command("kmToMiles")]
        [Description("Convert kilometers to miles.")]
        public async Task kmToMiles(CommandContext ctx, [Description("Value to convert.")] double value)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(value.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " km is " + (value * 0.6213712).ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " miles");
        }

        [Command("milesToKm")]
        [Description("Convert miles to kilometers.")]
        public async Task milesToKm(CommandContext ctx, [Description("Value to convert.")] double value)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(value.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " miles is " + (value * 1.609344).ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " km"); 
        }

        [Command("cToF")]
        [Description("Convert Celsius to Fahrenheit.")]
        public async Task cToF(CommandContext ctx, [Description("Value to convert.")] double value)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(value.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " °C is " + (value * 9.0/5.0 + 32.0).ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " °F");
        }

        [Command("fToC")]
        [Description("Convert Fahrenheit to Celsius.")]
        public async Task fToC(CommandContext ctx, [Description("Value to convert.")] double value)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(value.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " °F is " + ((value - 32.0) * 5.0/9.0).ToString(CultureInfo.CreateSpecificCulture("en-GB")) + " °C");
        }
    }
}

