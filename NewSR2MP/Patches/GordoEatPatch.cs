﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

using Il2CppMonomiPark.SlimeRancher.DataModel;
using NewSR2MP.Component;
using NewSR2MP.Packet;
using UnityEngine;
namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(GordoEat), nameof(GordoEat.DoEat))]
    public class GordoDoEat
    {
        public static void Postfix(GordoEat __instance, GameObject obj)
        {
            try
            {
                if ((ServerActive() || ClientActive()) && !handlingPacket)
                {
                    var packet = new GordoEatPacket()
                    {
                        id = __instance._id,
                        count = __instance.GordoModel.GordoEatenCount
                    };

                    MultiplayerManager.NetworkSend(packet);
                }
            }
            catch { }
        }

    }
    [HarmonyPatch(typeof(GordoEat._ReachedTarget_d__46), nameof(GordoEat._ReachedTarget_d__46.MoveNext))]
    public class GordoEatImmediateReachedTarget
    {
        public static void Postfix(GordoEat._ReachedTarget_d__46 __instance)
        {
            try
            {
                if (__instance.__1__state != 0)
                    return;
                
                if ((ServerActive() || ClientActive()) && !handlingPacket)
                {
                    MultiplayerManager.NetworkSend(new GordoBurstPacket
                    {
                        id = __instance.__4__this._id,
                        ident = GetIdentID(__instance.__4__this.GordoModel.identifiableType)
                    });
                }
            }
            catch { }
        }

    }
}
