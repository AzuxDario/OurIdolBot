using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Net.WebSocket;
using Newtonsoft.Json;
using OurIdolBot.Commands.ManagementCommands;
using OurIdolBot.Commands.MusicCommands;
using OurIdolBot.Commands.OtherCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Core
{
    class Bot
    {
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
        private CommandsNextModule _commands { get; set; }
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
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };

            DiscordClient = new DiscordClient(connectionConfig);

#if DEBUG
            // For Windows 7 I'm using to test
            DiscordClient.SetWebSocketClient<WebSocket4NetClient>();
#else
            // For Mono I'm using to release
            DiscordClient.SetWebSocketClient<WebSocketSharpClient>();
#endif

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefix = configJson.CommandPrefix,
                EnableDms = true,
                EnableMentionPrefix = true,
                CaseSensitive = false
            };

            _commands = DiscordClient.UseCommandsNext(commandsConfig);
            _commands.RegisterCommands<NowPlayingCommands>();
            _commands.RegisterCommands<DescriptionCommand>();
            _commands.RegisterCommands<PingCommand>();
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandErrored;

            await DiscordClient.ConnectAsync();
        }

        private void SetNetworkParameters()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 |
                                                   SecurityProtocolType.Tls12;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            return Task.FromResult(0);
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
        }


    }
}
