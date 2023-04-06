using Newtonsoft.Json;

namespace AnomandarisBotApp.Models
{
    public class DotaMatchDetailsDto
    {
        [JsonProperty("match_id")]
        public long MatchId { get; set; }
        public int Duration { get; set; }

        [JsonProperty("radiant_score")]
        public int RadiantScore { get; set; }

        [JsonProperty("dire_score")]
        public int DireScore { get; set; }

        [JsonProperty("players")]
        public PlayerDto[] Players { get; set; }

        [JsonProperty("start_time")]
        public int StartTime { get; set; }
    }

    public class PlayerDto
    {
        [JsonProperty("account_id")]
        public long? AccountId { get; set; }

        public int? Kills { get; set; }
        public int? Assists { get; set; }
        public int? Deaths { get; set; }


        [JsonProperty("last_hits")]
        public int? LastHits { get; set; }
        public int? Denies { get; set; }

        [JsonProperty("net_worth")]
        public int? Networth { get; set; }
        public bool IsRadiant { get; set; }

        [JsonProperty("radiant_win")]
        public bool RadiantWin { get; set; }

    }
}

//"killed_by": {
//    "npc_dota_hero_morphling": 6,
//    "npc_dota_hero_invoker": 2,
//    "npc_dota_hero_rattletrap": 1
//  },

/* todo: wand/stick ??
 "item_usage": {
    "smoke_of_deceit": 1,
    "enchanted_mango": 1,
    "flask": 1,
    "tango": 1,
    "faerie_fire": 1,
    "ward_sentry": 1,
    "infused_raindrop": 1,
    "clarity": 1,
    "ward_observer": 1,
    "boots": 1,
    "tpscroll": 1,
    "wind_lace": 1,
    "ring_of_regen": 1,
    "tranquil_boots": 1,
    "staff_of_wizardry": 1,
    "fluffy_hat": 1
  },
 */