using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
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

        private async Task LoadCommands()
        {
            using (Channel.EnterTypingState())
            {
                commands = new List<Command>();

                var parsedCommands = JObject.Parse(await File.ReadAllTextAsync("Commands.json")).ToObject<Dictionary<string, string>>();

                RestUserMessage lastMessage = null;
                string messageText = null;

                foreach (var command in parsedCommands)
                {
                    commands.Add(new Command(command.Key, command.Value));

                    if (lastMessage == null)
                    {
                        messageText = $"Registered command: {command.Key}";
                        lastMessage = await Channel.SendMessageAsync(messageText);
                    }
                    else
                    {
                        messageText += $", {command.Key}";
                        await lastMessage.ModifyAsync(msg => msg.Content = messageText);
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task LogAsync(LogMessage log)
        {
            File.AppendAllLines("DiscordBuild.log", new string[] { log.ToString() });
            Console.WriteLine(log.ToString());
            await Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            await LoadCommands();

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

            if (commandCaller.Equals("Commands", StringComparison.InvariantCultureIgnoreCase))
            {
                await Channel.SendMessageAsync($"Commands: {string.Join(", ", commands.Select(c => c.Caller).OrderBy(c => c))}");
            }

            if (commandCaller.Equals("ReloadCommands", StringComparison.InvariantCultureIgnoreCase))
            {
                await LoadCommands();
                await Channel.SendMessageAsync("Commands reloaded.");
            }

            if (commandCaller.Equals("Kill", StringComparison.InvariantCultureIgnoreCase))
            {
                await Channel.SendMessageAsync("Goodbye.");
                Environment.Exit(0);
            }

            foreach (var command in commands)
            {
                if (commandCaller.Equals(command.Caller, StringComparison.InvariantCultureIgnoreCase))
                {
                    await command.Execute(Channel);
                }
            }
        }
    }
}
