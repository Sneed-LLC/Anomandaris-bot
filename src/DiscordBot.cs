using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnomandarisBotApp
{
    public class DiscordBot
    {
        private readonly DiscordConfigJson _discordCfg;
        private DiscordSocketClient _client;
        private Dota2OpenApi _dotaOpenApi;

        public DiscordBot(DiscordConfigJson configParsed)
        {
            this._discordCfg = configParsed;
        }

        public async Task Init()
        {
            _client = new DiscordSocketClient();

            await Setup();

            _client.Disconnected += Disconnected;
        }

        public void ConfigureDotaApi(Dota2OpenApi bot)
        {
            this._dotaOpenApi = bot;
        }

        private async Task Setup()
        {
            _client.MessageReceived += MessageReceivedAsync;
            _client.Log += _client_Log;

            _client.Connected += Connected;

            await _client.LoginAsync(TokenType.Bot, _discordCfg.Token);
            await _client.StartAsync();

            _client.Ready += async () =>
            {
                await _client.SetActivityAsync(new Game("Gas na kurvite", type: ActivityType.Watching, flags: ActivityProperties.None, null));
                await _client.SetStatusAsync(UserStatus.Online);
            };
        }

        private async Task Disconnected(Exception e)
        {
            Console.WriteLine($"dc'd: {DateTime.Now}");
            const int setupMs = 3000;
            while (true)
            {
                try
                {
                    Console.WriteLine("disposing");
                    this._client.Dispose();
                    Console.WriteLine("disposed");

                    this._client = new DiscordSocketClient();
                    Console.WriteLine("new cli created");

                    await Setup();
                    Console.WriteLine("setup client");

                    _client.Disconnected += Disconnected;
                    Console.WriteLine("setup dc recursively");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not reconnect on dc @ {DateTime.Now}. Try setting it up again in {setupMs} ms.");
                }

                await Task.Delay(setupMs);
            }
        }

        private async Task Connected()
        {
            Console.WriteLine($"Bot: ${_client.CurrentUser.Username}");
        }

        public async Task Shutdown()
        {
            await _client.SetStatusAsync(UserStatus.Offline);
            _client.Dispose();
        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }


        private static bool _isRunningQuery = false;
        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            if (_isRunningQuery)
                return;
            
            _isRunningQuery = true;

            try
            {
                var message = arg as SocketUserMessage;
                var context = new SocketCommandContext(_client, message);
                if (!message.Author.IsBot && message.Author.Id != 164110045951295488)
                {
                    if (message.Content.Contains("ping", StringComparison.OrdinalIgnoreCase))
                    {
                        await context.Channel.SendMessageAsync("Wannussy");
                        return;
                    }
                    else if (message.Content.Contains("recent"))
                    {
                        var recentMatches = await this._dotaOpenApi.GetUserMatches(1);
                        var recentMatchDetails = await this._dotaOpenApi.GetMatchDetails(recentMatches.FirstOrDefault().Item1, recentMatches.FirstOrDefault().Item2);

                        await Print(context, recentMatchDetails);
                    }
                    else if (message.Content.Contains("posledno 10"))
                    {
                        var recentMatches = await this._dotaOpenApi.GetUserMatches(10);
                        var recentMatchDetails = recentMatches.Take(10).Select(match => this._dotaOpenApi.GetMatchDetails(match.Item1, match.Item2));

                        var res = await Task.WhenAll(recentMatchDetails);

                        foreach (var matchDetails in res.Where(res => res != null))
                        {
                            await Print(context, matchDetails);
                        }
                    }
                    else if (message.Content.Contains("puskai"))
                    {
                        var count = int.Parse(message.Content.Split(' ').Last().Trim());
                        if (count > 30)
                        {
                            await Notify("Mamka ti i pedal maxa e 30 nibblet.");
                            return;
                        }

                        var recentMatches = await this._dotaOpenApi.GetUserMatches(count);
                        var recentMatchDetails = recentMatches.Take(count).Select(match => this._dotaOpenApi.GetMatchDetails(match.Item1, match.Item2));

                        var res = await Task.WhenAll(recentMatchDetails);

                        foreach (var matchDetails in res.Where(res => res != null))
                        {
                            await Print(context, matchDetails);
                        }
                    }

                    return;
                }
            }
            finally
            {
                _isRunningQuery = false;
            }
        }

        public async Task Notify(string message)
        {
            Console.WriteLine(message);

            var sendChannelsMessagesTasks = new List<Task<RestUserMessage>>();
            foreach (var guild in this._client.Guilds)
            {
                if (guild.Id == _discordCfg.ServerId)
                {                    
                    sendChannelsMessagesTasks.Add(guild.GetTextChannel(_discordCfg.ChannelId).SendMessageAsync(message));
                    continue;
                }

                sendChannelsMessagesTasks.Add(guild.DefaultChannel.SendMessageAsync(message));
            }

            await Task.WhenAll(sendChannelsMessagesTasks);
        }

        public async Task Notify(Models.DotaMatchDetailsDto recentMatchDetails)
        {
            await Notify(GameFormatting(recentMatchDetails));
        }

        private static async Task Print(SocketCommandContext context, Models.DotaMatchDetailsDto recentMatchDetails)
        {
            await context.Channel.SendMessageAsync(GameFormatting(recentMatchDetails));
        }

        private static string GameFormatting(Models.DotaMatchDetailsDto recentMatchDetails)
        {
            var nikicha = recentMatchDetails.Players.FirstOrDefault();
            if ((nikicha.IsRadiant && nikicha.RadiantWin) || (!nikicha.IsRadiant && !nikicha.RadiantWin))
            {
                return $"Nikicha se razpisa! KDA: {nikicha.Kills}/{nikicha.Deaths}/{nikicha.Assists} NW: {nikicha.Networth} Denies: {nikicha.Denies} " +
                        $"Match: {Dota2OpenApi.DotabuffMatchUrlTemplate}{recentMatchDetails.MatchId} Duration: {TimeSpan.FromSeconds(recentMatchDetails.Duration).ToString(@"hh\:mm\:ss")}";
            }
            else
            {
                return $"Nikicha imashe incident! KDA: {nikicha.Kills}/{nikicha.Deaths}/{nikicha.Assists} NW: {nikicha.Networth} Denies: {nikicha.Denies} " +
                        $"Match: {Dota2OpenApi.DotabuffMatchUrlTemplate}{recentMatchDetails.MatchId} Duration: {TimeSpan.FromSeconds(recentMatchDetails.Duration).ToString(@"hh\:mm\:ss")}";
            }
        }
    }
}
