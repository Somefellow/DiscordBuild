using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBuild
{
    class Program
    {
        private DiscordSocketClient client;

        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
        }

        public async Task MainAsync()
        {
            using (client = new DiscordSocketClient())
            {
                client.Log += LogAsync;
                client.Ready += ReadyAsync;
                client.MessageReceived += MessageReceivedAsync;

                await client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings.Get("DiscordToken"));
                await client.StartAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            File.AppendAllLines("DiscordBuild.log", new string[] { log.ToString() });
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        // The Ready event indicates that the client has opened a
        // connection and it is now safe to access the cache.
        private Task ReadyAsync()
        {
            Console.WriteLine($"{client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == client.CurrentUser.Id)
                return;

            if (message.Content == "!ping")
                await message.Channel.SendMessageAsync("pong!");
        }
    }
}
