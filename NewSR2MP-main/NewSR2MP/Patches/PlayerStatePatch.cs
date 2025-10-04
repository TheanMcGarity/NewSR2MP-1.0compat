using HarmonyLib;

using NewSR2MP;
using NewSR2MP;
using NewSR2MP.Component;
using NewSR2MP.Packet;
using System;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Economy;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.Data;
using Il2CppMonomiPark.SlimeRancher.UI.Map;

namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.AddCurrency))]
    internal class PlayerStateAddCurrency
    {
        public static void Postfix(
            PlayerState __instance,
            ICurrency currencyDefinition,
            int adjust,
            bool showUiNotification = true,
            IUIDisplayData sourceOfChange = null)
        {

            if (ClientActive() || ServerActive())
            {
                SetMoneyPacket message = new SetMoneyPacket()
                {
                    newMoney = __instance.GetCurrency(currencyDefinition),
                    currencyId = (byte)currencyDefinition.PersistenceId,
                };
                MultiplayerManager.NetworkSend(message);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.SpendCurrency))]
    internal class PlayerStateSpendCurrency
    {
        public static void Postfix(
            PlayerState __instance,
            ICurrency currency, 
            int adjust,
            IUIDisplayData sourceOfChange = null)
        {

            if (ClientActive() || ServerActive())
            {
                SetMoneyPacket message = new SetMoneyPacket()
                {
                    newMoney = __instance.GetCurrency(currency),
                    currencyId = (byte)currency.PersistenceId,
                };
                MultiplayerManager.NetworkSend(message);
            }
        }
    }

}
