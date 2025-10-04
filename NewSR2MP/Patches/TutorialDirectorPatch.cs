using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.UI.IntroSequence;
using NewSR2MP.Component;
using NewSR2MP.Packet;
using UnityEngine;

namespace NewSR2MP.Patches
{
    // You can disable tutorials in sr2!
    
    /// <summary>
    /// Патч для пропуска вступительной заставки (титров) для клиента
    /// </summary>
    [HarmonyPatch(typeof(IntroSequenceUIRoot), nameof(IntroSequenceUIRoot.Start))]
    internal class SkipIntroPatch
    {
        internal static void Postfix(IntroSequenceUIRoot __instance)
        {
            // Пропускаем вступление только для клиента
            if (ClientActive())
            {
                SRMP.Log("IntroSequenceUIRoot: Skipping intro for client");
                __instance.EndSequence();
                Object.Destroy(__instance.gameObject);
            }
        }
    }
}
