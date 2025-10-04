using Il2CppMonomiPark.SlimeRancher.Persist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.SaveModels
{
    
    public class PlayerListV01 : SaveComponentBase<PlayerListV01>
    {

        public bool TryGetPlayer(Guid guid, out NetPlayerV01 player) 
        {
            player = playerList.FirstOrDefault(x => x.Key.value == guid).Value;
            return player != null;
        }
        
        public override string ComponentIdentifier => "MPLI";
        public override int ComponentVersion => 1;

        public Dictionary<GuidV01, NetPlayerV01> playerList = new Dictionary<GuidV01, NetPlayerV01>();

        public override void WriteComponent()
        {
            WriteDictionary(playerList);
        }

        public override void ReadComponent()
        {
            playerList = ReadDictionary<GuidV01, NetPlayerV01>();
        }

        public override void UpgradeComponent(PlayerListV01 old)
        {
            throw new NotImplementedException();
        }
    }
}
