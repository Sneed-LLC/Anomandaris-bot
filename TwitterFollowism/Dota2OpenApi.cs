using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TwitterFollowism.Models;
using static TwitterFollowism.Models.Enums;

namespace TwitterFollowism
{
    public class Dota2OpenApi
    {
        private Regex _regex;
        private Regex _regexKda;
        private readonly HttpClient _client;
        private readonly DiscordBot _discordBot;
        private readonly SavedRecords _savedRecords;

        private object lockObj = new object();

        private const int NikichaId = 170026947;

        static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(2, 2);

        private const string AllMatchesUrl = "https://api.opendota.com/api/players/170026947/matches";
        private const string SpecificMatchUrl = "https://api.opendota.com/api/matches/";
        private const string ProfileUrl = "https://api.opendota.com/api/players/170026947";
        private const string DotabuffMatchesUrl = "https://www.dotabuff.com/players/170026947/matches";

        private string RegexPattern = @"matches\/([0-9]+)";
        private string RegexKdaPattern = @"<span class=""kda-record""><span class=""value"">([0-9]+)<\/span>\/<span class=""value"">([0-9]+)<\/span>\/<span class=""value"">([0-9]+)<\/span><\/span>";


        private const int DelayMs = 1000000; // 15 mins

        public Dota2OpenApi(DiscordBot discordBot,
            SavedRecords savedEntities)
        {
            this._client = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(1000)
            };

            _regex = new Regex(RegexPattern);
            _regexKda = new Regex(RegexKdaPattern);

            this._discordBot = discordBot;
            this._savedRecords = savedEntities;
        }
        public async Task Run()
        {
            this._discordBot.ConfigureDotaApi(this);

            //await InitUsers();

            while (true)
            {

                //var usersFriends = await Task.WhenAll(this._config.UsersToTrack.Select(x => GetUserFriends(x)).ToArray());

                // parallel foreach ?
                //foreach (var userWithFriends in usersFriends)
                //{
                //    var user = userWithFriends.user;
                //    var newUserFriends = userWithFriends.friends;

                //    var oldUserFriends = this._savedRecords.UserAndFriends[user];

                //    var newFriends = newUserFriends.Except(oldUserFriends).ToArray();
                //    var removedFriends = oldUserFriends.Except(newUserFriends).ToArray();

                //    var friendsChanges = newFriends.Union(removedFriends).ToArray();

                //    if (!friendsChanges.Any())
                //    {
                //        continue;
                //    }

                //    await SendDiscordMessages(user, newFriends, removedFriends, friendsChanges);
                //    this._savedRecords.UserAndFriends[user] = newUserFriends;
                //    PersistSavedRecordsBlocking();
                //}

                await Task.Delay(DelayMs);
            }
        }

        //public AddUserCode ContinueTrackingUser(string user)
        //{
        //    if (this._config.UsersToTrack.Contains(user))
        //    {
        //        return AddUserCode.AlreadyAdded;
        //    }

        //    if (!this._savedRecords.UserAndFriends.ContainsKey(user))
        //    {
        //        return AddUserCode.NotConfigured;
        //    }

        //    this._config.UsersToTrack.Add(user);
        //    return AddUserCode.Success;
        //}

        //public async Task<AddUserCode> AddUser(string user)
        //{
        //    if (this._config.UsersToTrack.Contains(user) || this._savedRecords.UserAndFriends.ContainsKey(user))
        //    {
        //        return AddUserCode.AlreadyAdded;
        //    }

        //    var userExists = await UserExistsByUsername(user);
        //    if (!userExists)
        //    {
        //        return AddUserCode.DoesNotExist;
        //    }

        //    if (!this._savedRecords.UserAndFriends.ContainsKey(user))
        //    {
        //        this._savedRecords.UserAndFriends.Add(user, new HashSet<long>());
        //    }
        //    this._config.UsersToTrack.Add(user);

        //    if (!this._savedRecords.IsInitialSetup.ContainsKey(user))
        //    {
        //        this._savedRecords.IsInitialSetup.Add(user, true);
        //    }

        //    var friends = await GetUserFriends(user);
        //    this._savedRecords.UserAndFriends[user] = friends.friends;
        //    this._savedRecords.IsInitialSetup[user] = false;

        //    PersistSavedRecordsBlocking();
        //    return AddUserCode.Success;
        //}

        //public RemoveUserCode RemoveUser(string user)
        //{
        //    var userExists = this._config.UsersToTrack.Contains(user) || this._savedRecords.UserAndFriends.ContainsKey(user);

        //    if (this._config.UsersToTrack.Contains(user))
        //    {
        //        this._config.UsersToTrack.Remove(user);
        //    }

        //    if (this._savedRecords.IsInitialSetup.ContainsKey(user))
        //    {
        //        this._savedRecords.IsInitialSetup.Remove(user);
        //    }

        //    if (this._savedRecords.UserAndFriends.ContainsKey(user))
        //    {
        //        this._savedRecords.UserAndFriends.Remove(user);
        //    }

        //    if (userExists)
        //    {
        //        return RemoveUserCode.Success;
        //    }

        //    return RemoveUserCode.WasNotConfigured;
        //}

        //public RemoveUserCode StopTrackingUser(string user)
        //{
        //    if (!this._config.UsersToTrack.Contains(user))
        //    {
        //        return RemoveUserCode.WasNotConfigured;
        //    }

        //    this._config.UsersToTrack.Remove(user);
        //    return RemoveUserCode.Success;
        //}

        //private async Task SendDiscordMessages(string user, long[] newFriends, long[] removedFriends, long[] friendsChanges)
        //{
        //    List<string> discordMessages = new List<string>(friendsChanges.Length);

        //    var usersMap = await GetUsersBasicDataByIds(user, friendsChanges);

        //    foreach (var newFriend in newFriends)
        //    {
        //        var newFriendDetails = usersMap[newFriend];
        //        discordMessages.Add($"@here {user} Followed: {newFriendDetails.Username} ({newFriendDetails.Name}) . {string.Format(TwitterAccountLink, newFriendDetails.Username)}");
        //    }

        //    foreach (var removedFriend in removedFriends)
        //    {
        //        if (!usersMap.ContainsKey(removedFriend))
        //        {
        //            discordMessages.Add($"@here {user} UnFollowed: twitter account: {removedFriend} which does not exist anymore");
        //        }
        //        else
        //        {
        //            var removedFriendDetails = usersMap[removedFriend];
        //            discordMessages.Add($"@here {user} UnFollowed: {removedFriendDetails.Username} ({removedFriendDetails.Name}) . {string.Format(TwitterAccountLink, removedFriendDetails.Username)}");
        //        }
        //    }

        //    var sucessful = false;
        //    var attempts = 1;
        //    const int maxAttempts = 5;
        //    while (!sucessful && attempts <= maxAttempts)
        //    {
        //        try
        //        {
        //            await this._discordBot.Notify(string.Join(Environment.NewLine, discordMessages));
        //            sucessful = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Exception when notifying changes for {user}. {attempts}/{maxAttempts}");
        //            Console.WriteLine(ex.Message);
        //            attempts += 1;
        //        }
        //    }
        //}

        //private void PersistSavedRecordsBlocking()
        //{
        //    lock (lockObj)
        //    {
        //        File.WriteAllText(this._config.SavedRecordsRoute, JsonConvert.SerializeObject(this._savedRecords));
        //    }
        //}

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
                        RequestUri = new Uri("https://www.dotabuff.com/players/170026947/matches"),
                    };

                    request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
                    request.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.Add("accept-language", "en-US,en;q=0.9,bg;q=0.8");
                    request.Headers.Add("cookie", "_tz=Europe%2FSofia; _sess=U3N4aUJxR2U1KzBYOWR6STdJR3ZHQWhSeVdabVRtdHo0Mjl4V09UR0FYVE14WFFGUGlpRm5JZ0lYdkpXditGNk9hMXZLVVdmVmNSMUJIV3VJUkppbzErd0hhdXlxQnhyR1hGNk1ZaGFvcHk0M3NmcDQwRWVWN0xmTWpyYktXOVJzN3owaDFYaUtwN3Z5dWxLckc1a2lmSGFmSXphZE01T2E2L3EzRi8yczVMS0h6NjdYOXp6eFNZYjJDQUpUR0EvLS1tcmNKN2VaaDY5clFZa21nTm85Qnl3PT0%3D--5300e5fa6c09cb1eb0e128861efae8238f6e84e8; _hi=1675701235719");
                    request.Headers.Add("referer", "https://www.dotabuff.com/players/170026947");

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
                while (!completed)
                {
                    try
                    {
                        respRaw = await this._client.GetStringAsync(string.Format(SpecificMatchUrl + matchId.ToString()));
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
                        return null;
                    }
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
