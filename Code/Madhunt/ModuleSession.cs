using System.Collections.Generic;

namespace Celeste.Mod.Madhunt {
    public class ModuleSession : EverestModuleSession {
        public HashSet<EntityID> HiderBerryTokens { get; set; } = new HashSet<EntityID>();
    }
}