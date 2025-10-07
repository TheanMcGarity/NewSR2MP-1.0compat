using PlortDepositor = Il2Cpp.PlortDepositor;

namespace NewSR2MP.Patches;

[HarmonyPatch(typeof(PlortDepositor), nameof(PlortDepositor.OnFilledChanged))]
public class PlortDepositorPatch
{
    static void Postfix(PlortDepositor __instance)
    {
        if (handlingPacket)
            return;
        
        MultiplayerManager.NetworkSend(new PlortDepositPacket
        {
            id = __instance._id,
            count = (ushort)__instance._model.AmountDeposited,
            max = (ushort)__instance._fillAmount
        });
    }
}