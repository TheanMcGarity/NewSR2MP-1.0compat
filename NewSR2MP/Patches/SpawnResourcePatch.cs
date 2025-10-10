using HarmonyLib;


namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(SpawnResource), nameof(SpawnResource.Awake))]
    internal class SpawnResourceAwake
    {
        public static void Postfix(SpawnResource __instance)
        {
            __instance.gameObject.AddComponent<NetworkGarden>();
        }
    }
}
