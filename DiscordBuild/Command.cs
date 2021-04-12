using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBuild
{
    public class Command
    {
        public string Caller { get; private set; }

        private readonly string executionCommand;

        public Command(string name, string value)
        {
            Caller = name;
            executionCommand = value;
        }

        public async Task Execute(ISocketMessageChannel channel)
        {
            using (channel.EnterTypingState())
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{executionCommand.Replace("\"", "\\\"")}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = false
                    }
                };

                process.Start();
                process.WaitForExit();

                var temp = "";

                var output = new List<string>();
                while ((temp = process.StandardOutput.ReadLine()) != null) output.Add(temp);

                var errorOutput = new List<string>();
                while ((temp = process.StandardError.ReadLine()) != null) errorOutput.Add(temp);

                int exitCode = process.ExitCode;
                process.Close();

                //string commandText = $"/bin/bash -c \"{executionCommand.Replace("\"", "\\\"")}\"";
                string message = $"Command: `{executionCommand}`\nExit Code: `{exitCode}`\nOutput: ```{(output.Count > 0 ? string.Join("\n", output) : " ")}```\nError: ```{(errorOutput.Count > 0 ? string.Join("\n", errorOutput) : " ")}```";

                await channel.SendMessageAsync(message);
            }

            await Task.CompletedTask;
        }
    }
}
