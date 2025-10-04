namespace NewSR2MP.Packet;

[HarmonyPatch(typeof(PlayerZoneTracker), nameof(PlayerZoneTracker.OnRegionsChanged))]
public class RegionChangeFixPatch
{
    static Exception Finalizer(Exception __exception)
    {
        return null;
        
    }
}