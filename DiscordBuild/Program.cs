using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace DiscordBuild
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private DiscordSocketClient client;
        private List<Command> commands;

        public async Task MainAsync()
        {
            commands = new List<Command>();

            using (client = new DiscordSocketClient())
            {
                client.Log += LogAsync;
                client.Ready += ReadyAsync;
                client.MessageReceived += MessageReceivedAsync;

                await client.LoginAsync(TokenType.Bot, ConfigurationService.DiscordToken);
                await client.StartAsync();

                await Task.Delay(Timeout.Infinite);

                commands = null;
                client = null;
            }
        }

        public ISocketMessageChannel Channel => client.GetChannel(ConfigurationService.DiscordChannelId) as ISocketMessageChannel;

        private async Task LogAsync(LogMessage log)
        {
            File.AppendAllLines("DiscordBuild.log", new string[] { log.ToString() });
            Console.WriteLine(log.ToString());
            await Task.CompletedTask;
        }


        private async Task ReadyAsync()
        {
            var parsedCommands = JObject.Parse(await File.ReadAllTextAsync("Commands.json")).ToObject<Dictionary<string, string>>();

            foreach (var command in parsedCommands)
            {
                commands.Add(new Command(command.Key, command.Value));
            }

            await Channel.SendMessageAsync("DotaBotBuilder ready!");

            await Task.CompletedTask;
        }


        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id != ConfigurationService.AdminUserId)
                return;

            if (!message.Content.StartsWith(ConfigurationService.CommandPrefix))
                return;

            string commandCaller = message.Content.Substring(ConfigurationService.CommandPrefix.Length);

            if (commandCaller == "commands")
            {
                await Channel.SendMessageAsync($"Commands: {string.Join(", ", commands.Select(c => c.Caller).OrderBy(c => c))}");
            }

            foreach (var command in commands)
            {
                if (command.Caller == commandCaller)
                {
                    await command.Execute(Channel);
                }
            }
        }
    }
}
