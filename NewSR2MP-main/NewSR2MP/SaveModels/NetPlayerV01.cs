
using Il2CppMonomiPark.SlimeRancher.Persist;
using System;
using System.Collections.Generic;
using Il2CppSystem.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace NewSR2MP.SaveModels
{
    /// <summary>
    /// TODO: Add health, stamina, and rads.
    /// </summary>
    public class NetPlayerV01 : SaveComponentBase<NetPlayerV01>
    {
        public override string ComponentIdentifier { get; }
        
        public override int ComponentVersion => 1;

        public byte sceneGroup = 1;
        
        public ModdedVector3V01 position = new ModdedVector3V01(541.6466f, 18.646f, 349.3299f);
        public ModdedVector3V01 rotation = new ModdedVector3V01(Vector3.up * 236.8107f);

        public List<NetworkAmmoDataV01> ammo = new List<NetworkAmmoDataV01>();
        
        public override void WriteComponent()
        {
            Write(position);
            Write(rotation);
            
            Write(sceneGroup);
            
            WriteList(ammo);
        }

        public override void ReadComponent()
        {
            position = Read<ModdedVector3V01>();
            rotation = Read<ModdedVector3V01>();
            
            sceneGroup = Read<byte>();
            
            ammo = ReadList<NetworkAmmoDataV01>();
        }

        public override void UpgradeComponent(NetPlayerV01 old)
        {
            throw new NotImplementedException();
        }
    }
}
