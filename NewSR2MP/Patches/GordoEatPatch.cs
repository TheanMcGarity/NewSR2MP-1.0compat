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
    [HarmonyPatch(typeof(GordoEat), nameof(GordoEat.ImmediateReachedTarget))]
    public class GordoEatImmediateReachedTarget
    {
        public static void Postfix(GordoEat __instance)
        {
            try
            {
                if ((ServerActive() || ClientActive()) && !handlingPacket)
                {
                    MultiplayerManager.NetworkSend(new GordoBurstPacket
                    {
                        id = __instance._id,
                        ident = GetIdentID(__instance.GordoModel.identifiableType)
                    });
                }
            }
            catch { }
        }

    }
}
