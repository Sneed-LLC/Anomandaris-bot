using System.Collections.Generic;

namespace AnomandarisBotApp
{
    public class SavedGames
    {
        public string SavedRecordsRoute { get; set; }

        public SavedGames()
        {
            this.PlayersMatchesIds = new Dictionary<long, HashSet<long>>();
        }

        public Dictionary<long, HashSet<long>> PlayersMatchesIds { get; }
    }
}
