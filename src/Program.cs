using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterFollowism
{
    class Program
    {
        private static DiscordBot _bot;
        static string dir = string.Empty;
        static DiscordConfigJson discordConfig;
        static SavedRecords savedRecords;

        public const int TakeLastGames = 10;
        static async Task Main()
        {
            //SetupDirectoryConfigs();

            //twitterApiConfig.UsersToTrack = usersToTrackArr;
            //Console.WriteLine($"Starting to track {string.Join(',', usersToTrackArr)}");

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

            var savedRecordsRoute = $"{dir}savedRecords.json";
            var savedRecordsStr = File.ReadAllText(savedRecordsRoute);
            savedRecords = JsonConvert.DeserializeObject<SavedRecords>(savedRecordsStr) ?? new SavedRecords();


        }
    }
}
