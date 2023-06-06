namespace AnomandarisBotApp
{
    public class DiscordConfigJson
    {
        public string Token { get; set; }
        public string Prefix { get; set; }

        public ulong ServerId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
