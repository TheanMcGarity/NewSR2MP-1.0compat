using SR2E;
using SR2E.Buttons;
using SR2E.Managers;

namespace NewSR2MP.Patches;

//[HarmonyPatch(typeof(GameContext), nameof(GameContext.Start))]
public class GameContextStart
{
    public static void Postfix(GameContext __instance)
    {
        var label = SR2ELanguageManger.AddTranslationFromSR2E("button.multiplayer", "b.multiplayer", "UI");
        
        new CustomPauseMenuButton(label, 1, () => { });
        new CustomMainMenuButton(label, ConvertEUtil.Texture2DToSprite(LoadImage("MultiplayerButton")), 1, () => { });
    }
}