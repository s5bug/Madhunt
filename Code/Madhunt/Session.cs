using System.Collections.Generic;

namespace Celeste.Mod.Madhunt {
    public class MadhuntSession : EverestModuleSession {
        public bool WonLastRound { get; set; }
        
        public HashSet<EntityID> HiderBerryTokens { get; set; } = new HashSet<EntityID>();
    }
}