using Il2CppMonomiPark.SlimeRancher.SceneManagement;

namespace NewSR2MP.Patches;

[HarmonyPatch(typeof(SceneLoader), nameof(SceneLoader.HandleSceneLoadingError))]
public class ErrorLoggingPatch
{
    static void Prefix(SceneLoader __instance)
    {
        foreach (var err in __instance._loadErrors)
        {
            SRMP.Error($"Error in loading scene!\nType: {err.Exception._message}\nStack: {err.Exception.StackTrace}");
        }
    }
}