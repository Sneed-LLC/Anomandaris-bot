using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AnomandarisBotApp.Models;

namespace AnomandarisBotApp
{
    public class Dota2OpenApi
    {
        private Regex _regex;
        private Regex _regexKda;
        private readonly HttpClient _client;
        private readonly DiscordBot _discordBot;
        private readonly SavedGames _savedRecords;

        private object lockObj = new object();

        //todo move as config and add templating
        private const int NikichaId = 170026947;

        static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(2, 2);

        private const string SpecificMatchUrl = "https://api.opendota.com/api/matches/";
        private const string ProfileUrl = "https://api.opendota.com/api/players/170026947";
        private const string DotabuffMatchesReferalUrl = "https://www.dotabuff.com/players/170026947";
        private const string DotabuffMatchesUrl = "https://www.dotabuff.com/players/170026947/matches";
        public const string DotabuffMatchUrlTemplate = "https://www.dotabuff.com/matches/";

        private string RegexPattern = @"matches\/([0-9]+)";
        private string RegexKdaPattern = @"<span class=""kda-record""><span class=""value"">([0-9]+)<\/span>\/<span class=""value"">([0-9]+)<\/span>\/<span class=""value"">([0-9]+)<\/span><\/span>";

        private const int DelayMs = 60000; // 1 min

        public Dota2OpenApi(DiscordBot discordBot,
            SavedGames savedGames)
        {
            this._client = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(1000)
            };

            _regex = new Regex(RegexPattern);
            _regexKda = new Regex(RegexKdaPattern);

            this._discordBot = discordBot;
            this._savedRecords = savedGames;
        }
        public async Task Run(CancellationToken token)
        {
            this._discordBot.ConfigureDotaApi(this);

            while (!token.IsCancellationRequested)
            {
                var lastGames = await this.GetUserMatches(15);
                if (_savedRecords.PlayersMatchesIds.Any() && _savedRecords.PlayersMatchesIds.ContainsKey(NikichaId) && _savedRecords.PlayersMatchesIds[NikichaId].Any())
                {
                    lastGames = lastGames.Where(g => !_savedRecords.PlayersMatchesIds[NikichaId].Contains(g.Item1)).ToList();
                }

                if (!_savedRecords.PlayersMatchesIds.Any())
                {
                    _savedRecords.PlayersMatchesIds[NikichaId] = lastGames.Select(g => g.Item1).ToHashSet();
                    await _discordBot.Notify($"Started tracking new games after: {lastGames.First().Item1}");
                    PersistSavedRecordsSynchronous();
                    await Task.Delay(DelayMs);
                    continue;
                }

                if (lastGames.Any())
                {
                    var lastMatchesTasks = lastGames.Select(g => this.GetMatchDetails(g.Item1, g.Item2));
                    var matchesDetails = await Task.WhenAll(lastMatchesTasks);

                    foreach (var match in matchesDetails.Where(md => md != null))
                    {
                        await _discordBot.Notify(match);
                    }

                    foreach (var game in lastGames)
                    {
                        _savedRecords.PlayersMatchesIds[NikichaId].Add(game.Item1);
                    }

                    PersistSavedRecordsSynchronous();
                }

                await Task.Delay(DelayMs);
            }

            await _discordBot.Notify("Merc out");
        }
       
        private void PersistSavedRecordsSynchronous()
        {
            lock (lockObj)
            {
                File.WriteAllText(this._savedRecords.SavedRecordsRoute, JsonConvert.SerializeObject(this._savedRecords));
            }
        }

        public async Task<List<(long, KdaDto)>> GetUserMatches(int matchesCount)
        {
            string respRaw = "";
            bool completed = false;
            while (!completed)
            {
                try
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(DotabuffMatchesUrl),
                    };

                    request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
                    request.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.Add("accept-language", "en-US,en;q=0.9,bg;q=0.8");
                    request.Headers.Add("cookie", "_tz=Europe%2FSofia; _sess=U3N4aUJxR2U1KzBYOWR6STdJR3ZHQWhSeVdabVRtdHo0Mjl4V09UR0FYVE14WFFGUGlpRm5JZ0lYdkpXditGNk9hMXZLVVdmVmNSMUJIV3VJUkppbzErd0hhdXlxQnhyR1hGNk1ZaGFvcHk0M3NmcDQwRWVWN0xmTWpyYktXOVJzN3owaDFYaUtwN3Z5dWxLckc1a2lmSGFmSXphZE01T2E2L3EzRi8yczVMS0h6NjdYOXp6eFNZYjJDQUpUR0EvLS1tcmNKN2VaaDY5clFZa21nTm85Qnl3PT0%3D--5300e5fa6c09cb1eb0e128861efae8238f6e84e8; _hi=1675701235719");
                    request.Headers.Add("referer", DotabuffMatchesReferalUrl);

                    var ggs = await this._client.SendAsync(request);
                    ggs.EnsureSuccessStatusCode();
                    respRaw = await ggs.Content.ReadAsStringAsync();
                    completed = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(1000);
            }

            List<(long, KdaDto)> final = new List<(long, KdaDto)>();
            List<long> matchIds = new List<long>();
            List<KdaDto> kdaResults = new List<KdaDto>();

            var matchesResult = _regex.Matches(respRaw).Take(matchesCount * 2);
            var kdaMatchesResults = _regexKda.Matches(respRaw).Take(matchesCount);

            bool skipNext = false;
            foreach (Match match in matchesResult)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                matchIds.Add(long.Parse(match.Groups[1].Value));
                skipNext = true;
            }

            foreach (Match match in kdaMatchesResults)
            {
                kdaResults.Add(new KdaDto
                {
                    Kills = int.Parse(match.Groups[1].Value),
                    Deaths = int.Parse(match.Groups[2].Value),
                    Assists = int.Parse(match.Groups[3].Value),
                });
            }

            for (int i = 0; i < matchIds.Count; i++)
            {
                final.Add((matchIds[i], kdaResults[i]));
            }

            return final;
        }

        public async Task<DotaMatchDetailsDto> GetMatchDetails(long matchId, KdaDto kdaDto)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();

                string respRaw = "";
                bool completed = false;                
                var attempt = 0;
                while (!completed && attempt < 3)
                {
                    using var client = new HttpClient();
                    try
                    {
                        respRaw = await client.GetStringAsync(string.Format(SpecificMatchUrl + matchId.ToString()));
                        completed = true;
                        var matchResult = JsonConvert.DeserializeObject<DotaMatchDetailsDto>(respRaw);
                        matchResult.Players = matchResult.Players
                            .Where(p => p.Assists == kdaDto.Assists && p.Deaths == kdaDto.Deaths && p.Kills == kdaDto.Kills).OrderBy(p => p.LastHits)
                            .ToArray();

                        return matchResult;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Potential empty game: {SpecificMatchUrl}{matchId}");
                        Console.WriteLine(ex.Message);
                        attempt += 1;
                    }

                    await Task.Delay(100);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
            return null;
        }

        public async Task<Dota2PlayerProfile> GetPlayerProfileData()
        {
            try
            {
                var respRaw = await this._client.GetStringAsync(string.Format(ProfileUrl));
                var dota2PlayerResp = JsonConvert.DeserializeObject<Dota2PlayerProfile>(respRaw);
                return dota2PlayerResp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when fetching player's data {ex.Message}");
                return null;
            }
        }
    }
}
