using Newtonsoft.Json;

namespace AnomandarisBotApp.Models
{
    public class Dota2PlayerProfile
    {
        [JsonProperty("rank_tier")]
        public double RankTier { get; set; }

        [JsonProperty("mmr_estimate")]
        public MmrEstimate MMREstimate { get; set; }
    }

    public class MmrEstimate
    {
        [JsonProperty("estimate")]
        public double Estimate { get; set; }
    }
}

/*
 {
  "solo_competitive_rank": null,
  "leaderboard_rank": null,
  "rank_tier": 62,
  "competitive_rank": null,
  "mmr_estimate": {
    "estimate": 3882
  },
  "profile": {
    "account_id": 170026947,
    "personaname": "Anomandaris",
    "name": null,
    "plus": false,
    "cheese": 0,
    "steamid": "76561198130292675",
    "avatar": "https://avatars.akamai.steamstatic.com/9b05820ef0518eb3451c5b471b85f3fc053ddb06.jpg",
    "avatarmedium": "https://avatars.akamai.steamstatic.com/9b05820ef0518eb3451c5b471b85f3fc053ddb06_medium.jpg",
    "avatarfull": "https://avatars.akamai.steamstatic.com/9b05820ef0518eb3451c5b471b85f3fc053ddb06_full.jpg",
    "profileurl": "https://steamcommunity.com/profiles/76561198130292675/",
    "last_login": null,
    "loccountrycode": null,
    "status": null,
    "is_contributor": false,
    "is_subscriber": false
  }
}
 */
