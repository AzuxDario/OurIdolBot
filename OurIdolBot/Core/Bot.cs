﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OurIdolBot.Attributes;
using OurIdolBot.Commands.ManagementCommands;
using OurIdolBot.Commands.MusicCommands;
using OurIdolBot.Commands.OtherCommands;
using OurIdolBot.Services.PicturesServices;
using OurIdolBot.Services.RolesServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Core
{
    class Bot
    {
#if DEBUG
        readonly string botname = "Our Idol Test";
#else
        readonly string botname = "Our Idol";
#endif

        public struct ConfigJson
        {
            [JsonProperty("token")]
            public string Token { get; private set; }

            [JsonProperty("prefix")]
            public string CommandPrefix { get; private set; }

            [JsonProperty("developer")]
            public ulong Developer { get; private set; }
        }


        public static DiscordClient DiscordClient { get; set; }
        private CommandsNextExtension _commands { get; set; }
        public static ConfigJson configJson { get; private set; }

        public void Run()
        {
            Connect();
            SetNetworkParameters();
        }

        private async void Connect()
        {
            var json = "";
            string settingsFile;
#if DEBUG
            settingsFile = "debug.json";
#else
            settingsFile = "release.json";
#endif
            using (var fs = File.OpenRead(settingsFile))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var connectionConfig = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.All
            };

            DiscordClient = new DiscordClient(connectionConfig);

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { configJson.CommandPrefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                CaseSensitive = false,
                Services = BuildDependencies()
            };

            _commands = DiscordClient.UseCommandsNext(commandsConfig);
            _commands.SetHelpFormatter<CustomHelpFormatter>();
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandErrored;
            RegisterCommands();

            await DiscordClient.ConnectAsync();
        }

        private ServiceProvider BuildDependencies()
        {
            return new ServiceCollection()
                .AddScoped<AssignRolesService>()
                .AddScoped<NekosLifeImageService>()
                .BuildServiceProvider();
        }

        private void SetNetworkParameters()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private void RegisterCommands()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyTypes = assembly.GetTypes();

            var registerCommandsMethod = _commands.GetType().GetMethods()
                .FirstOrDefault(p => p.Name == "RegisterCommands" && p.IsGenericMethod);

            foreach (var type in assemblyTypes)
            {
                var attributes = type.GetCustomAttributes();
                if (attributes.Any(p => p.GetType() == typeof(CommandsGroupAttribute)))
                {
                    var genericRegisterCommandMethod = registerCommandsMethod.MakeGenericMethod(type);
                    genericRegisterCommandMethod.Invoke(_commands, null);
                }
            }
        }

        private Task Commands_CommandExecuted(CommandsNextExtension extension, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.Log(LogLevel.Information, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

            return Task.FromResult(0);
        }

        private async Task Commands_CommandErrored(CommandsNextExtension extension, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.Log(LogLevel.Error, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}.");

            switch (e.Exception)
            {
                case Checks​Failed​Exception ex:
                    {
                        StringBuilder messageToSend = new StringBuilder();
                        messageToSend.Append("Not enough permissions to complete the action.").AppendLine();

                        var failedChecks = ex.FailedChecks;
                        foreach (var failedCheck in failedChecks)
                        {
                            if (failedCheck is RequireBotPermissionsAttribute failBot)
                            {
                                messageToSend.Append("I need: ");
                                messageToSend.Append(failBot.Permissions.ToPermissionString());
                                messageToSend.AppendLine();
                            }
                            else if (failedCheck is RequireUserPermissionsAttribute failUser)
                            {
                                messageToSend.Append("You need: ");
                                messageToSend.Append(failUser.Permissions.ToPermissionString());
                                messageToSend.AppendLine();
                            }
                            else if (failedCheck is RequireOwnerAttribute)
                            {
                                messageToSend.Append("This command can be used only by bot owner.");
                                messageToSend.AppendLine();
                            }
                        }

                        await e.Context.Channel.SendMessageAsync(messageToSend.ToString());
                        break;
                    }
                case UnauthorizedException _:
                    {
                        await e.Context.Member.SendMessageAsync("Sorry, I don't have enough permissions to perform this action.");
                        break;
                    }
                
                default:
                    {
                        break;
                    }
            }
        }


    }
}
