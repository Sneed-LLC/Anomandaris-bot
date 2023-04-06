using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnomandarisBotApp
{
    class Program
    {
        private static DiscordBot _discordbot;
        static string dir = string.Empty;
        static DiscordConfigJson discordConfig;
        static SavedGames savedRecords;
        private static bool isConfigured = false;
        private static CancellationToken _cancellationToken;
        public static CancellationTokenSource _tokenSource { get; private set; }


        static async Task Main()
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken());
            AppDomain.CurrentDomain.ProcessExit += AppDomain_ProcessExit;

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

            var dotaBot = new Dota2OpenApi(_discordbot, savedRecords);
            var infPoll = dotaBot.Run(_tokenSource.Token);
            infPoll.GetAwaiter().GetResult();

            Console.WriteLine("Main Thread shutting down");
        }

        private static void AppDomain_ProcessExit(object sender, EventArgs e)
        {
            _discordbot.Notify("Merc out").Wait();
            _tokenSource.Cancel();
            Thread.Sleep(600);

            Environment.Exit(0);
        }

        private static Task SetupDiscordBot(DiscordConfigJson configParsed)
        {
            _discordbot = new DiscordBot(configParsed);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            return _discordbot.Init();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _discordbot.Shutdown().GetAwaiter().GetResult();
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
