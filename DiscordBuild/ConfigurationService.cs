using System.Configuration;

namespace DiscordBuild
{
    public static class ConfigurationService
    {
        public static ulong AdminUserId = ulong.Parse(ConfigurationManager.AppSettings.Get("AdminUserId"));

        public static string CommandPrefix = ConfigurationManager.AppSettings.Get("CommandPrefix");

        public static string DiscordToken = ConfigurationManager.AppSettings.Get("DiscordToken");

        public static ulong DiscordChannelId = ulong.Parse(ConfigurationManager.AppSettings.Get("DiscordChannelId"));
    }
}
