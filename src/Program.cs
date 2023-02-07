using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnomandarisBotApp
{
    class Program
    {
        private static DiscordBot _bot;
        static string dir = string.Empty;
        static DiscordConfigJson discordConfig;
        static SavedGames savedRecords;
        private static bool isConfigured = false;

        static async Task Main()
        {
            while (!isConfigured)
            {
                try
                {
                    SetupDirectoryConfigs();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            await SetupDiscordBot(discordConfig);

            var dotaBot = new Dota2OpenApi(_bot, savedRecords);

            var infPoll = dotaBot.Run();
            infPoll.GetAwaiter().GetResult();

            Console.WriteLine("Main Thread shutting down");
        }

        private static Task SetupDiscordBot(DiscordConfigJson configParsed)
        {
            _bot = new DiscordBot(configParsed);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            return _bot.Init();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _bot.Shutdown().GetAwaiter().GetResult();
        }

        private static void SetupDirectoryConfigs()
        {
            Console.WriteLine("configure from current dir ? y/n (Default n)");
            var configSetup = Console.ReadLine();
            bool goBack = true;
            if (!string.IsNullOrEmpty(configSetup) && configSetup.Contains("y", StringComparison.InvariantCulture))
            {
                goBack = false;
            }

            dir = goBack ? @"..\..\..\..\" : "";
            var discordConfigRoute = $"{dir}discordConfig.json";

            var discordCfg = File.ReadAllText(discordConfigRoute);
            discordConfig = JsonConvert.DeserializeObject<DiscordConfigJson>(discordCfg);

            var savedRecordsRoute = $"{dir}savedGames.json";
            var savedRecordsStr = File.ReadAllText(savedRecordsRoute);
            savedRecords = JsonConvert.DeserializeObject<SavedGames>(savedRecordsStr) ?? new SavedGames();
            savedRecords.SavedRecordsRoute = savedRecordsRoute;
            isConfigured = true;

        }
    }
}
