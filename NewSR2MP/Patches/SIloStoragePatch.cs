using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using NewSR2MP.Component;
using UnityEngine;
namespace NewSR2MP.Patches
{
    internal class SiloStorageInitAmmoPostfixDelayed
    {
        public static IEnumerator Delayed(SiloStorage storage)
        {
            yield return null;
            yield return null;
            
            try
            {
                storage.RegisterAmmoPointerUsingModel();
                
            }
            catch (Exception e)
            {
                SRMP.Error($"Error in network ammo!\n{e}\nThis can cause major desync!");
            }
        }
    }
    [HarmonyPatch(typeof(SiloStorage), nameof(SiloStorage.InitAmmo))]
    public class SiloStorageInitAmmo
    {
        public static void Postfix(SiloStorage __instance)
        {
            MelonCoroutines.Start(SiloStorageInitAmmoPostfixDelayed.Delayed(__instance));
        }
    }
}
