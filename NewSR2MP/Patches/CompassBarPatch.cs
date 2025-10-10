using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.HUD;
using NewSR2MP.Component;

namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(CompassBarUI), nameof(CompassBarUI.Start))]
    public class CompassBarUIStartPatch
    {
        public static void Postfix(CompassBarUI __instance)
        {
            if (!ServerActive() && !ClientActive())
                return;

            try
            {
                // Add multiplayer coordinates display
                //__instance.gameObject.AddComponent<MultiplayerCompassUI>();
                compassBarUIInstance = __instance;
            }
            catch (System.Exception ex)
            {
                SRMP.Error($"Failed to patch CompassBarUI: {ex}");
            }
        }
    }
}








